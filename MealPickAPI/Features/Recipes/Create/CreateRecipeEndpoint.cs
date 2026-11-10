using System.Security.Claims;
using MealPickAPI.Data;
using MealPickAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Features.Recipes.Create;

public static class CreateRecipeEndpoint
{
    public static IEndpointRouteBuilder MapCreateRecipe(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/recipes", HandleAsync)
            .RequireAuthorization()
            .WithTags("Recipes");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateRecipeRequest request,
        ClaimsPrincipal user,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var title = request.Title?.Trim() ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var instructions = request.Instructions ?? [];
        var ingredients = request.Ingredients ?? [];
        var categoryIds = (request.CategoryIds ?? []).Distinct().ToList();

        var validationError = Validate(title, description, instructions, ingredients);
        if (validationError is not null)
        {
            return Results.BadRequest(new { error = validationError });
        }

        // Categories are attached to the recipe, so they must be loaded.
        // Ingredients are only referenced by ID, so counting them is enough.
        var ingredientIds = ingredients.Select(ingredient => ingredient.IngredientId).ToList();
        var categories = await dbContext.Categories
            .Where(category => categoryIds.Contains(category.Id))
            .ToListAsync(cancellationToken);
        var existingIngredientCount = await dbContext.Ingredients
            .CountAsync(ingredient => ingredientIds.Contains(ingredient.Id), cancellationToken);

        if (categories.Count != categoryIds.Count || existingIngredientCount != ingredientIds.Count)
        {
            return Results.BadRequest(new { error = "One or more category or ingredient IDs do not exist." });
        }

        var recipe = new Recipe(userId, title, description);

        foreach (var category in categories)
        {
            recipe.Categories.Add(category);
        }

        for (var index = 0; index < ingredients.Count; index++)
        {
            var ingredient = ingredients[index];
            recipe.AddIngredient(ingredient.IngredientId, ingredient.Amount, ingredient.Unit!.Trim(), displayOrder: index);
        }

        for (var index = 0; index < instructions.Count; index++)
        {
            recipe.AddInstruction(instructions[index].Text!.Trim(), stepNumber: index + 1);
        }

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/recipes/{recipe.Id}", CreateRecipeResponse.From(recipe));
    }

    /// <summary>Returns an error message, or <c>null</c> when the request is valid.</summary>
    private static string? Validate(
        string title,
        string? description,
        List<CreateRecipeInstructionRequest> instructions,
        List<CreateRecipeIngredientRequest> ingredients)
    {
        if (title.Length == 0 || title.Length > Recipe.TitleMaxLength)
        {
            return $"Title is required and must be {Recipe.TitleMaxLength} characters or fewer.";
        }

        if (description?.Length > Recipe.DescriptionMaxLength)
        {
            return $"Description must be {Recipe.DescriptionMaxLength} characters or fewer.";
        }

        if (instructions.Any(instruction =>
                instruction is null ||
                string.IsNullOrWhiteSpace(instruction.Text) ||
                instruction.Text.Trim().Length > RecipeInstruction.TextMaxLength))
        {
            return $"Instructions are required and must be {RecipeInstruction.TextMaxLength} characters or fewer.";
        }

        if (ingredients.Any(ingredient =>
                ingredient is null ||
                ingredient.Amount <= 0 ||
                string.IsNullOrWhiteSpace(ingredient.Unit) ||
                ingredient.Unit.Trim().Length > RecipeIngredient.UnitMaxLength))
        {
            return $"Each ingredient must have a positive amount and a unit of {RecipeIngredient.UnitMaxLength} characters or fewer.";
        }

        var distinctIngredientCount = ingredients.Select(ingredient => ingredient.IngredientId).Distinct().Count();
        if (distinctIngredientCount != ingredients.Count)
        {
            return "An ingredient can only be added once to a recipe.";
        }

        return null;
    }
}
