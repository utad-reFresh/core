namespace utad.reFresh.core.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    
    public List<IngredientSearch> IngredientSearches { get; set; } = new();
    
    public ICollection<UserIngredient> UserIngredients { get; set; }


}