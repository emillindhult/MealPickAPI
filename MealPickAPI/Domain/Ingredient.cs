namespace MealPickAPI.Domain;

public class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = [];

}