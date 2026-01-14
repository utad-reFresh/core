using Newtonsoft.Json;
using utad.reFresh.core.Models;

public class SpoonacularService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ApplicationDbContext _db;

    public SpoonacularService(HttpClient httpClient, IConfiguration config, ApplicationDbContext db)
    {
        _httpClient = httpClient;
        _apiKey = config["Spoonacular:ApiKey"];
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

public async Task<Recipe> GetRecipeAsync(int id)
{
    var response = await _httpClient.GetAsync($"https://api.spoonacular.com/recipes/{id}/information?apiKey={_apiKey}");
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var dto = JsonConvert.DeserializeObject<SpoonacularRecipeDto>(json);

    // Handle ingredients
    var ingredientIds = dto.ExtendedIngredients?.Select(i => i.Id).ToList() ?? new List<int>();
    var existingIngredients = _db.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToList();
    var ingredients = new List<Ingredient>();
    foreach (var i in dto.ExtendedIngredients ?? new List<SpoonacularIngredientDto>())
    {
        var existing = ingredients.FirstOrDefault(e => e.Id == i.Id) 
                       ?? _db.Ingredients.Local.FirstOrDefault(e => e.Id == i.Id);
        if (existing != null)
        {
            ingredients.Add(existing);
        }
        else
        {
            var newIng = new Ingredient
            {
                Id = i.Id,
                Name = i.NameClean,
                ImageUrl = $"https://spoonacular.com/cdn/ingredients_100x100/{i.Image}"
            };
            _db.Ingredients.Add(newIng);
            ingredients.Add(newIng);
        }
    }

        // Handle equipment
        var equipmentDtos = dto.AnalyzedInstructions?
            .SelectMany(instr => instr.Steps ?? new List<SpoonacularStepDto>())
            .SelectMany(step => step.Equipment ?? new List<SpoonacularEquipmentDto>())
            .GroupBy(eq => eq.Id)
            .Select(g => g.First())
            .ToList() ?? new List<SpoonacularEquipmentDto>();

        var equipmentIds = equipmentDtos.Select(eq => eq.Id).ToList();
        var existingEquipment = _db.Equipment.Where(e => equipmentIds.Contains(e.Id)).ToList();
        var equipment = new List<Equipment>();
        foreach (var eq in equipmentDtos)
        {
            var existingEq = existingEquipment.FirstOrDefault(e => e.Id == eq.Id);
            if (existingEq != null)
            {
                equipment.Add(existingEq);
            }
            else
            {
                var imageUrl = eq.Image.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? eq.Image
                    : $"https://spoonacular.com/cdn/equipment_100x100/{eq.Image}";

                var newEq = new Equipment
                {
                    Id = eq.Id,
                    Name = eq.Name,
                    ImageUrl = imageUrl
                };
                _db.Equipment.Add(newEq);
                equipment.Add(newEq);
            }
        }

        await _db.SaveChangesAsync();
        
        var steps = dto.AnalyzedInstructions?
            .SelectMany(instr => instr.Steps ?? new List<SpoonacularStepDto>())
            .Select((step, idx) => new RecipeStep
            {
                Number = step.Number,
                Step = step.Step
            })
            .ToList() ?? new List<RecipeStep>();

// Build a list to avoid duplicates in-memory
        var recipeIngredients = new List<RecipeIngredient>();

        foreach (var i in dto.ExtendedIngredients ?? new List<SpoonacularIngredientDto>())
        {
            var ingredient = ingredients.FirstOrDefault(e => e.Id == i.Id);

            // Check if already in the in-memory list
            var alreadyInList = recipeIngredients.Any(ri => ri.RecipeId == dto.Id && ri.IngredientId == i.Id);

            // Check if already tracked in the context
            var alreadyTracked = _db.RecipeIngredients.Local.Any(ri => ri.RecipeId == dto.Id && ri.IngredientId == i.Id);

            if (!alreadyInList && !alreadyTracked)
            {
                recipeIngredients.Add(new RecipeIngredient
                {
                    RecipeId = dto.Id,
                    IngredientId = i.Id,
                    Name = i.Name,
                    NameClean = i.NameClean,
                    Original = i.Original,
                    AmountMetric = i.Measures?.Metric?.Amount ?? 0,
                    UnitShortMetric = i.Measures?.Metric?.UnitShort ?? string.Empty,
                    UnitLongMetric = i.Measures?.Metric?.UnitLong ?? string.Empty,
                    AmountImperial = i.Measures?.Us?.Amount ?? 0,
                    UnitShortImperial = i.Measures?.Us?.UnitShort ?? string.Empty,
                    UnitLongImperial = i.Measures?.Us?.UnitLong ?? string.Empty,
                    Ingredient = ingredient
                });
            }
        }
        
        // Map DTO to Recipe domain model
        var recipe = new Recipe
        {
            Id = dto.Id,
            Title = dto.Title,
            Summary = dto.Summary,
            ImageUrl = dto.Image,
            Vegetarian = dto.Vegetarian,
            Vegan = dto.Vegan,
            GlutenFree = dto.GlutenFree,
            DairyFree = dto.DairyFree,
            VeryHealthy = dto.VeryHealthy,
            Cheap = dto.Cheap,
            VeryPopular = dto.VeryPopular,
            Sustainable = dto.Sustainable,
            LowFodmap = dto.LowFodmap,
            PreparationMinutes = dto.PreparationMinutes,
            CookingMinutes = dto.CookingMinutes,
            ReadyInMinutes = dto.ReadyInMinutes,
            Servings = dto.Servings,
            HealthScore = dto.HealthScore,
            AggregateLikes = dto.AggregateLikes,
            WeightWatcherSmartPoints = dto.WeightWatcherSmartPoints,
            CreditsText = dto.CreditsText,
            SourceName = dto.SourceName,
            Cuisines = dto.Cuisines,
            DishTypes = dto.DishTypes,
            Diets = dto.Diets,
            Occasions = dto.Occasions,
            SpoonacularScore = dto.SpoonacularScore,
            SourceUrl = dto.SourceUrl,
            SpoonacularSourceUrl = dto.SpoonacularSourceUrl,
            Steps = steps,
            Equipment = equipment,
            Ingredients = recipeIngredients,
            LastUpdated = DateTime.UtcNow
        };

        return recipe;
        
    }



    public async Task<string> SearchRecipesAsync(string query)
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(10);

        var url = $"https://api.spoonacular.com/recipes/complexSearch?query={query}&sort=popularity&number=20&apiKey={_apiKey}";
        Console.WriteLine(url); // Log the full URL to verify
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            // Log or throw an exception with the error details
            throw new Exception($"Spoonacular API error: {response.StatusCode} - {error}");
        }
        
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<List<Ingredient>> SearchAndSaveIngredientsAsync(string query)
    {
        var url = $"https://api.spoonacular.com/food/ingredients/search?apiKey={_apiKey}&query={query}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = JsonConvert.DeserializeObject<SpoonacularIngredientSearchResponse>(content);

        if (searchResult == null || searchResult.Results == null)
            return new List<Ingredient>();

        var ingredientIds = searchResult.Results.Select(r => r.Id).ToList();
        var existingIngredients = _db.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToList();

        {
            var existing = existingIngredients.FirstOrDefault(i => i.Id == ing.Id);
            if (existing == null)
            {
                var ingredient = new Ingredient
                {
                    Id = ing.Id,
                    Name = ing.Name,
                    ImageUrl = $"https://spoonacular.com/cdn/ingredients_100x100/{ing.Image}"
                };
                _db.Ingredients.Add(ingredient);
            }

        await _db.SaveChangesAsync();

        // Return all (existing + new) ingredients for this search
        return _db.Ingredients.Where(i => ingredientIds.Contains(i.Id)).ToList();
    }
    
    public class SpoonacularRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string ImageType { get; set; }
        public string Summary { get; set; }
        public List<SpoonacularIngredientDto> ExtendedIngredients { get; set; }
        public List<SpoonacularInstructionDto> AnalyzedInstructions { get; set; }
        public string SourceUrl { get; set; }
        public string SpoonacularSourceUrl { get; set; }
        public bool Vegetarian { get; set; }
        public bool Vegan { get; set; }
        public bool GlutenFree { get; set; }
        public bool DairyFree { get; set; }
        public bool VeryHealthy { get; set; }
        public bool Cheap { get; set; }
        public bool VeryPopular { get; set; }
        public bool Sustainable { get; set; }
        public bool LowFodmap { get; set; }
        public int? PreparationMinutes { get; set; }
        public int? CookingMinutes { get; set; }
        public int ReadyInMinutes { get; set; }
        public int Servings { get; set; }
        public double HealthScore { get; set; }
        public int AggregateLikes { get; set; }
        public int WeightWatcherSmartPoints { get; set; }
        public string CreditsText { get; set; }
        public string SourceName { get; set; }
        public List<string> Cuisines { get; set; }
        public List<string> DishTypes { get; set; }
        public List<string> Diets { get; set; }
        public List<string> Occasions { get; set; }
        public double SpoonacularScore { get; set; }
        public string Instructions { get; set; }
    }
    
    public class SpoonacularIngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameClean { get; set; }
        public string Original { get; set; }
        public string Image { get; set; }
        public SpoonacularMeasuresDto Measures { get; set; }
    }

    public class SpoonacularMeasuresDto
    {
        public SpoonacularMeasureDto Us { get; set; }
        public SpoonacularMeasureDto Metric { get; set; }
    }

    public class SpoonacularMeasureDto
    {
        public double Amount { get; set; }
        public string UnitShort { get; set; }
        public string UnitLong { get; set; }
    }

    
    public class SpoonacularIngredientSearchResponse
    {
        public List<SpoonacularIngredientDto> Results { get; set; }
        public int Offset { get; set; }
        public int Number { get; set; }
        public int TotalResults { get; set; }
    }

    public class SpoonacularInstructionDto
    {
        public List<SpoonacularStepDto> Steps { get; set; }
    }

    public class SpoonacularStepDto
    {
        public int Number { get; set; }
        public string Step { get; set; } 
        
        public List<SpoonacularEquipmentDto> Equipment { get; set; }
    }

    public class SpoonacularEquipmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}