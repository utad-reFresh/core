using Newtonsoft.Json;
using utad.reFresh.core.Models;

public class SpoonacularService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public SpoonacularService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Spoonacular:ApiKey"];
    }

    public async Task<Recipe> GetRecipeAsync(int id)
    {
        var response = await _httpClient.GetAsync($"https://api.spoonacular.com/recipes/{id}/information?apiKey={_apiKey}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<SpoonacularRecipeDto>(json);

        // Flatten equipment from all steps
        var equipment = dto.AnalyzedInstructions?
            .SelectMany(instr => instr.Steps ?? new List<SpoonacularStepDto>())
            .SelectMany(step => step.Equipment ?? new List<SpoonacularEquipmentDto>())
            .GroupBy(eq => eq.Id)
            .Select(g => g.First())
            .Select(eq => new Equipment
            {
                Id = eq.Id,
                Name = eq.Name,
                ImageUrl = eq.Image
            }).ToList() ?? new List<Equipment>();

        return new Recipe
        {
            Id = dto.Id,
            Title = dto.Title,
            ImageUrl = dto.Image,
            Summary = dto.Summary,
            Instructions = dto.Instructions,
            Ingredients = dto.ExtendedIngredients?.Select(i => new Ingredient
            {
                Id = i.Id,
                Name = i.Name,
                ImageUrl = i.Image
            }).ToList() ?? new List<Ingredient>(),
            Equipment = equipment,
            PricePerServing = dto.PricePerServing,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<string> SearchRecipesAsync(string query)
    {
        var url = $"https://api.spoonacular.com/recipes/complexSearch?query={query}&apiKey={_apiKey}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public class SpoonacularRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Summary { get; set; }
        public string Instructions { get; set; }
        public double PricePerServing { get; set; }
        public List<SpoonacularIngredientDto> ExtendedIngredients { get; set; }
        public List<SpoonacularInstructionDto> AnalyzedInstructions { get; set; }
    }

    public class SpoonacularIngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }

    public class SpoonacularInstructionDto
    {
        public List<SpoonacularStepDto> Steps { get; set; }
    }

    public class SpoonacularStepDto
    {
        public List<SpoonacularEquipmentDto> Equipment { get; set; }
    }

    public class SpoonacularEquipmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}