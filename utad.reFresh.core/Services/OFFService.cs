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
    string productName = product.product_name;
    string productNameEn = product.product_name_en;
    string genericName = product.generic_name;
    string genericNameEn = product.generic_name_en;
    var categories = ((IEnumerable<dynamic>)product.categories_tags)?.Select(c => (string)c).ToList() ?? new List<string>();

    var searchTerms = new List<string>();
    searchTerms.AddRange(categories.Select(c => c.Replace("en:", "")));

    void AddNameTerms(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            searchTerms.Add(name);
            searchTerms.AddRange(
                name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => word.Length >= 4)
            );
        }
    }

    AddNameTerms(productName);
    AddNameTerms(productNameEn);
    AddNameTerms(genericName);
    AddNameTerms(genericNameEn);

    var foundIngredients = new List<Ingredient>();
    foreach (var term in searchTerms.Distinct())
    {
        var spoonacularIngredients = await _spoonacularService.SearchAndSaveIngredientsAsync(term);
        if (spoonacularIngredients != null)
            foundIngredients.AddRange(spoonacularIngredients);
    }
    foundIngredients = foundIngredients.GroupBy(i => i.Id).Select(g => g.First()).ToList();

    var search = new OpenFoodFactsProductSearch
    {
        Barcode = barcode,
        ProductName = productName,
        Ingredients = foundIngredients
    };
    _db.OpenFoodFactsProductSearches.Add(search);
    await _db.SaveChangesAsync();
    return search;
}
    
    
}