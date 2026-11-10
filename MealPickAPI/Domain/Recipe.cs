namespace MealPickAPI.Domain;

public sealed class Recipe
{
    public const int UserIdMaxLength = 450;
    public const int TitleMaxLength = 100;
    public const int DescriptionMaxLength = 300;

    private Recipe()
    {
    }

    public Recipe(string userId, string title, string? description)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Description = description;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ICollection<RecipeIngredient> RecipeIngredients { get; } = [];
    public ICollection<Category> Categories { get; } = [];
    public ICollection<RecipeInstruction> Instructions { get; } = [];

    public void AddIngredient(Guid ingredientId, decimal amount, string unit, int displayOrder)
    {
        RecipeIngredients.Add(new RecipeIngredient(Id, ingredientId, amount, unit, displayOrder));
    }

    public void AddInstruction(string text, int stepNumber)
    {
        Instructions.Add(new RecipeInstruction(Id, text, stepNumber));
    }
}
