namespace MealPickAPI.Domain;

public class Recipe
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Descripton { get; set; }
    public List<string> Instructions { get; set; } = [];
    public List<Ingredient> Ingredients { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}
