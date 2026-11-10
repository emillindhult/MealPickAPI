namespace MealPickAPI.Domain;

public sealed class RecipeInstruction
{
    public const int TextMaxLength = 2000;

    private RecipeInstruction()
    {
    }

    public RecipeInstruction(Guid recipeId, string text, int stepNumber)
    {
        RecipeId = recipeId;
        Text = text;
        StepNumber = stepNumber;
    }

    public Guid Id { get; private set; }

    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; } = null!;

    public int StepNumber { get; private set; }
    public string Text { get; private set; } = string.Empty;
}
