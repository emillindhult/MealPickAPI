namespace MealPickAPI.Domain;

public sealed class Ingredient
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<RecipeIngredient> RecipeIngredients { get; } = [];
}