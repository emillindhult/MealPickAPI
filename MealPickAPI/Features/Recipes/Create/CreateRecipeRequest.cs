namespace MealPickAPI.Features.Recipes.Create;

public sealed class CreateRecipeRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Instructions { get; set; } = [];
    public List<Guid> CategoryIds { get; set; } = [];
    public List<Guid> IngredientIds { get; set; } = [];
}
