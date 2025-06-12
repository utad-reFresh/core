namespace utad.reFresh.core.Models;

public class IngredientSearch
{
    public int Id { get; set; }
    public string Query { get; set; }
    
    public List<Ingredient> Ingredients { get; set; } = new();
}

