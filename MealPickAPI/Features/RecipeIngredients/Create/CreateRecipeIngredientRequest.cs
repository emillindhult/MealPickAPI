namespace MealPickAPI.Features.RecipeIngredients.Create;

public sealed class CreateRecipeIngredientRequest
{
    public Guid IngredientId { get; set; }
    public decimal Amount { get; set; }
    public string? Unit { get; set; }
}
