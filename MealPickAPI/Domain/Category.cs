namespace MealPickAPI.Domain;

public sealed class Category
{
    public const int NameMaxLength = 50;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<Recipe> Recipes { get; } = [];
}
