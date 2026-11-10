namespace MealPickAPI.Domain;

public sealed class Ingredient
{
    public const int NameMaxLength = 100;

    private Ingredient()
    {
    }

    public Ingredient(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<RecipeIngredient> RecipeIngredients { get; } = [];
}
