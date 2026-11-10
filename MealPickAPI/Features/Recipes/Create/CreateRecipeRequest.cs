namespace MealPickAPI.Features.Recipes.Create;

// Properties are nullable because clients can send explicit JSON nulls;
// the endpoint validates and normalizes them.
public sealed class CreateRecipeRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<CreateRecipeInstructionRequest>? Instructions { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<CreateRecipeIngredientRequest>? Ingredients { get; set; }
}

public sealed class CreateRecipeIngredientRequest
{
    public Guid IngredientId { get; set; }
    public decimal Amount { get; set; }
    public string? Unit { get; set; }
}

public sealed class CreateRecipeInstructionRequest
{
    public string? Text { get; set; }
}
