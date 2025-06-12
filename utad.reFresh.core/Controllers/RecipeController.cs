using Microsoft.AspNetCore.Mvc;
using utad.reFresh.core.Models;
using Microsoft.EntityFrameworkCore;

namespace utad.reFresh.core.Controllers;

// Controllers/RecipesController.cs
[ApiController]
[Route("api/[controller]")]
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
        // Include Ingredients and Equipment when fetching from DB
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Equipment)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            // Fetch from Spoonacular
            recipe = await _spoonacularService.GetRecipeAsync(id);
            if (recipe == null) return NotFound();

            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();
        }
        return Ok(recipe);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchRecipes([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query cannot be empty.");
        }

        var results = await _spoonacularService.SearchRecipesAsync(query);
        if (string.IsNullOrEmpty(results))
        {
            return NotFound("No recipes found.");
        }

        return Ok(results);
    }
}