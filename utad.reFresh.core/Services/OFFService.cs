// utad.reFresh.core/Services/OpenFoodFactsService.cs

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using utad.reFresh.core.Models;

public class OFFService
{
    private readonly HttpClient _httpClient;
    private readonly SpoonacularService _spoonacularService;
    private readonly ApplicationDbContext _db;

    public OFFService(HttpClient httpClient, SpoonacularService spoonacularService, ApplicationDbContext db)
    {
        _httpClient = httpClient;
        _spoonacularService = spoonacularService;
        _db = db;
    }

    
   public async Task<OpenFoodFactsProductSearch> GetProductIngredientsAsync(string barcode)
    {
        var cached = _db.OpenFoodFactsProductSearches
            .Include(s => s.Ingredients)
            .FirstOrDefault(s => s.Barcode == barcode);

        if (cached != null)
            return cached;

        var url = $"https://world.openfoodfacts.org/api/v0/product/{barcode}.json";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject(json);
        if (data.status != 1) return null;

        var product = data.product;
        string productNameEn = product.product_name_en;
        string genericNameEn = product.generic_name_en;
        var categories = ((IEnumerable<dynamic>)product.categories_tags)?.Select(c => (string)c).ToList() ?? new List<string>();

        var searchTerms = new List<string>();

        // Add the last three English categories (most specific)
        var enCategories = categories.Where(c => c.StartsWith("en:")).ToList();
        foreach (var cat in enCategories.TakeLast(3))
            searchTerms.Add(cat.Replace("en:", ""));

        // Add English product/generic names if available
        void AddIfNotEmpty(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                searchTerms.Add(value);
        }
        AddIfNotEmpty(productNameEn);
        AddIfNotEmpty(genericNameEn);

        // Remove duplicates
        // Remove empty and duplicate terms, limit to 3
        searchTerms = searchTerms
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
        
        
        var foundIngredients = new List<Ingredient>();
        foreach (var term in searchTerms)
        {
            var spoonacularIngredients = await _spoonacularService.SearchAndSaveIngredientsAsync(term);
            if (spoonacularIngredients != null && spoonacularIngredients.Any())
            {
                foundIngredients.AddRange(spoonacularIngredients);
                break; // Stop after first successful search
            }
        }
        foundIngredients = foundIngredients.GroupBy(i => i.Id).Select(g => g.First()).ToList();

        var search = new OpenFoodFactsProductSearch
        {
            Barcode = (string)product.code ?? string.Empty,
            ProductName = (string)product.product_name ?? string.Empty,
            Ingredients = foundIngredients
        };
        
        if (string.IsNullOrWhiteSpace(search.Barcode))
            throw new ArgumentException("Product code is required.");
        
        if (string.IsNullOrWhiteSpace(search.ProductName))
            throw new ArgumentException("Product name is required.");
        
        _db.OpenFoodFactsProductSearches.Add(search);
        await _db.SaveChangesAsync();
        return search;
    }
            
}