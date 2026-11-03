namespace MealPickAPI.Domain;

public sealed class RecipeIngredient
{
    private RecipeIngredient()
    {
    }

    public RecipeIngredient(Guid recipeId, Guid ingredientId, decimal amount, string unit, int displayOrder)
    {
        RecipeId = recipeId;
        IngredientId = ingredientId;
        Amount = amount;
        Unit = unit;
        DisplayOrder = displayOrder;
    }

    public Guid RecipeId { get; private set; }
    public Recipe Recipe { get; private set; } = null!;

    public Guid IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string Unit { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }
}