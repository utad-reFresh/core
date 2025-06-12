namespace utad.reFresh.core.Models;

public class UserIngredient
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }

    public int Quantity { get; set; }
    
    public bool isFavorite { get; set; } = false;
    public DateTime? ExpirationDate { get; set; }
}