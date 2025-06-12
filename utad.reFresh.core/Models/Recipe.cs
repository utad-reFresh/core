namespace utad.reFresh.core.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ImageUrl { get; set; }
    public string Summary { get; set; }
    public string Instructions { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public List<Equipment> Equipment { get; set; }
    public double PricePerServing { get; set; }
    public DateTime LastUpdated { get; set; }
}