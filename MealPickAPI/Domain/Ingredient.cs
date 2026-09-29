namespace MealPickAPI.Domain;

public class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Amount { get; set; }
    public MetricType Metric { get; set; }
    public Guid RecipeId { get; set; }

}

public enum MetricType
{
    G,
    ST,
    L,
    DL,
    CL,
    ML,
    MSK,
    TSK,
    KRM,
}