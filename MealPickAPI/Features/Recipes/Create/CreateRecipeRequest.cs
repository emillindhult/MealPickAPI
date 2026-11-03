namespace MealPickAPI.Features.Recipes.Create;

public sealed class CreateRecipeRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateRecipeInstructionRequest> Instructions { get; set; } = [];
    public List<Guid> CategoryIds { get; set; } = [];
    public List<CreateRecipeIngredientRequest> Ingredients { get; set; } = [];
}

public sealed class CreateRecipeIngredientRequest
{
    public Guid IngredientId { get; set; }
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public sealed class CreateRecipeInstructionRequest
{
    public string Text { get; set; } = string.Empty;
}
