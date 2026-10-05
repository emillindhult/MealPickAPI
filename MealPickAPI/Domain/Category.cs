namespace MealPickAPI.Domain;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = [];
}
