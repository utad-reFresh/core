namespace  utad.reFresh.core.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageType { get; set; }
    public string Summary { get; set; }
    public List<RecipeIngredient> Ingredients { get; set; }
    public List<Equipment> Equipment { get; set; }
    
    public List<RecipeStep> Steps { get; set; }
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

public class RecipeIngredient
{
    public string Name { get; set; }
    public string NameClean { get; set; }
    public string Original { get; set; }
    public double AmountMetric { get; set; }
    public string UnitShortMetric { get; set; }
    public string UnitLongMetric { get; set; }
    public double AmountImperial { get; set; }
    public string UnitShortImperial { get; set; }
    public string UnitLongImperial { get; set; }
    
    
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }

}

public class RecipeStep
{
    public int Number { get; set; }
    public string Step { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }
    public int Id { get; set; }

}