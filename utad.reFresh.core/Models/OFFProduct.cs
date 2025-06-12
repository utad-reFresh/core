// utad.reFresh.core/Models/OpenFoodFactsProduct.cs

using System.ComponentModel.DataAnnotations;
using utad.reFresh.core.Models;

public class OFFProduct
{
    public string Code { get; set; }
    public string ProductName { get; set; }
    public List<string> CategoriesTags { get; set; }
    public List<string> IngredientsText { get; set; }
}

public class OpenFoodFactsProductSearch
{
    [Key]
    public string Barcode { get; set; }
    public string ProductName { get; set; }
    public List<Ingredient> Ingredients { get; set; }
}