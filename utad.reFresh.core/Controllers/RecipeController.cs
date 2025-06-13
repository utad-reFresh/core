using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using utad.reFresh.core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;


namespace utad.reFresh.core.Controllers;

// Controllers/RecipesController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RecipeController : ControllerBase
{
    private readonly SpoonacularService _spoonacularService;
    private readonly ApplicationDbContext _db;

    public RecipeController(SpoonacularService spoonacularService, ApplicationDbContext db)
    {
        _spoonacularService = spoonacularService;
        _db = db;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
                .ThenInclude(i => i.Ingredient)
            .Include(r => r.Equipment)
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            recipe = await _spoonacularService.GetRecipeAsync(id);
            if (recipe == null) return NotFound();

            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();
        }
        
        
        // Map to DTO to avoid cycles
        var recipeDto = new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            ImageUrl = recipe.ImageUrl,
            Summary = recipe.Summary,
            Steps = recipe.Steps.Select(s => new RecipeStepDto
            {
                Number = s.Number,
                Step = s.Step
            }).ToList(),
            Ingredients = recipe.Ingredients.Select(i => new IngredientDto
            {
                Id = i.IngredientId,
                Name = i.Name,
                ImageUrl = i.Ingredient.ImageUrl
            }).ToList(),
            Equipment = recipe.Equipment.Select(e => new EquipmentDto
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = e.ImageUrl
            }).ToList(),
            SourceUrl = recipe.SourceUrl,
            SpoonacularSourceUrl = recipe.SpoonacularSourceUrl,
            Vegetarian = recipe.Vegetarian,
            Vegan = recipe.Vegan,
            GlutenFree = recipe.GlutenFree,
            DairyFree = recipe.DairyFree,
            VeryHealthy = recipe.VeryHealthy,
            Cheap = recipe.Cheap,
            VeryPopular = recipe.VeryPopular,
            Sustainable = recipe.Sustainable,
            LowFodmap = recipe.LowFodmap,
            PreparationMinutes = recipe.PreparationMinutes,
            CookingMinutes = recipe.CookingMinutes,
            ReadyInMinutes = recipe.ReadyInMinutes,
            Servings = recipe.Servings,
            HealthScore = recipe.HealthScore,
            AggregateLikes = recipe.AggregateLikes,
            WeightWatcherSmartPoints = recipe.WeightWatcherSmartPoints,
            CreditsText = recipe.CreditsText,
            SourceName = recipe.SourceName,
            Cuisines = recipe.Cuisines,
            DishTypes = recipe.DishTypes,
            Diets = recipe.Diets,
            Occasions = recipe.Occasions,
            SpoonacularScore = recipe.SpoonacularScore,
            LastUpdated = recipe.LastUpdated
        };

        return Ok(recipeDto);
    }
    
    public class SpoonacularRecipeSearchResponse
    {
        public List<SpoonacularRecipeSummaryDto> Results { get; set; }
        public int Offset { get; set; }
        public int Number { get; set; }
        public int TotalResults { get; set; }
    }
    
    public class RecipeStepDto
    {
        public int Number { get; set; }
        public string Step { get; set; }
    }
    
    public class SpoonacularRecipeSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string ImageType { get; set; }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRecipes([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty.");

        var results = await _spoonacularService.SearchRecipesAsync(query);
        
        if (string.IsNullOrEmpty(results))
            return NotFound("No recipes found.");

        #if !DEBUG
            var searchResponse = JsonConvert.DeserializeObject<SpoonacularRecipeSearchResponse>(results);
            var recipeDtos = searchResponse?.Results ?? new List<SpoonacularRecipeSummaryDto>();
            foreach (var dto in recipeDtos)
            {
                var exists = await _db.Recipes.AnyAsync(r => r.Id == dto.Id);
                if (!exists)
                {
                    var recipe = await _spoonacularService.GetRecipeAsync(dto.Id);
                    if (recipe != null)
                    {
                        // Ensure ingredients are saved
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            var ingredientExists = await _db.Ingredients.AnyAsync(i => i.Id == ingredient.Ingredient.Id);
                            if (!ingredientExists)
                            {
                                _db.Ingredients.Add(ingredient.Ingredient);
                            }
                            else
                            {
                                _db.Ingredients.Attach(ingredient.Ingredient);
                            }
                        }
                        // Ensure equipment is saved
                        foreach (var equipment in recipe.Equipment)
                        {
                            var equipmentExists = await _db.Equipment.AnyAsync(e => e.Id == equipment.Id);
                            if (!equipmentExists)
                            {
                                _db.Equipment.Add(equipment);
                            }
                        }
                        _db.Recipes.Add(recipe);
                    }
                }
            }
            await _db.SaveChangesAsync();
        #endif

        return Ok(results);
    }
    
    public class RecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string Summary { get; set; }
        public List<RecipeStepDto> Steps { get; set; }
        public List<IngredientDto> Ingredients { get; set; }
        public List<EquipmentDto> Equipment { get; set; }
        public string? SourceUrl { get; set; }
        public string SpoonacularSourceUrl { get; set; }
        public bool? Vegetarian { get; set; }
        public bool? Vegan { get; set; }
        public bool? GlutenFree { get; set; }
        public bool? DairyFree { get; set; }
        public bool? VeryHealthy { get; set; }
        public bool? Cheap { get; set; }
        public bool? VeryPopular { get; set; }
        public bool? Sustainable { get; set; }
        public bool? LowFodmap { get; set; }
        public int? PreparationMinutes { get; set; }
        public int? CookingMinutes { get; set; }
        public int? ReadyInMinutes { get; set; }
        public int? Servings { get; set; }
        public double? HealthScore { get; set; }
        public int? AggregateLikes { get; set; }
        public int? WeightWatcherSmartPoints { get; set; }
        public string? CreditsText { get; set; }
        public string? SourceName { get; set; }
        public List<string>? Cuisines { get; set; }
        public List<string>? DishTypes { get; set; }
        public List<string>? Diets { get; set; }
        public List<string>? Occasions { get; set; }
        public double? SpoonacularScore { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    
    public class EquipmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
    
    [HttpGet("ingredients")]
    public async Task<IActionResult> GetAllIngredients()
    {
        var ingredients = await _db.Ingredients.ToListAsync();
        var ingredientDtos = ingredients.Select(i => new IngredientDto
        {
            Id = i.Id,
            Name = i.Name,
            ImageUrl = i.ImageUrl
        }).ToList();
        return Ok(ingredientDtos);
    }

    [HttpGet("search-ingredients")]
    public async Task<IActionResult> SearchIngredients([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty.");
        
        var ingredientDtos = new List<IngredientDto>();

        // Check if this query was already searched
        var previousSearch = await _db.IngredientSearches
            .Include(s => s.Ingredients)
            .FirstOrDefaultAsync(s => s.Query == query);

        if (previousSearch != null && previousSearch.Ingredients.Any())
        {
            ingredientDtos = previousSearch.Ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                ImageUrl = i.ImageUrl
            }).ToList();
            return Ok(ingredientDtos);
        }

        // If not, fetch from Spoonacular and save
        var newIngredients = await _spoonacularService.SearchAndSaveIngredientsAsync(query);
        if (newIngredients == null || !newIngredients.Any())
            return NotFound("No ingredients found.");

        // Save the search
        var search = new IngredientSearch { Query = query, Ingredients = newIngredients };
        _db.IngredientSearches.Add(search);
        await _db.SaveChangesAsync();

        ingredientDtos = newIngredients.Select(i => new IngredientDto
        {
            Id = i.Id,
            Name = i.Name,
            ImageUrl = i.ImageUrl
        }).ToList();

        return Ok(ingredientDtos);

    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetRecipeList()
    {
        var recipes = await _db.Recipes
            .OrderByDescending(r => r.SpoonacularScore)
            .Select(r => new
            {
                Id = r.Id,
                Name = r.Title,
                ImageUrl = r.ImageUrl
            })
            .ToListAsync();

        return Ok(recipes);
    }

    
    public class IngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
}