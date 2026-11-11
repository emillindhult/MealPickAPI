using System.Security.Claims;
using MealPickAPI.Data;
using MealPickAPI.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Features.RecipeIngredients.Create;

public static class CreateRecipeIngredientEndpoint
{
    // SQL Server error numbers for unique index / unique constraint violations.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static IEndpointRouteBuilder MapCreateRecipeIngredient(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/recipes/{recipeId:guid}/ingredients", HandleAsync)
            .RequireAuthorization()
            .WithTags("Recipe Ingredients");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid recipeId,
        CreateRecipeIngredientRequest request,
        ClaimsPrincipal user,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var unit = request.Unit?.Trim();
        if (request.Amount <= 0 || string.IsNullOrEmpty(unit) || unit.Length > RecipeIngredient.UnitMaxLength)
        {
            return Results.BadRequest(new { error = $"Amount must be positive and unit is required and must be {RecipeIngredient.UnitMaxLength} characters or fewer." });
        }

        var recipe = await dbContext.Recipes
            .Where(recipe => recipe.Id == recipeId)
            .Select(recipe => new
            {
                recipe.UserId,
                LastDisplayOrder = recipe.RecipeIngredients.Max(ingredient => (int?)ingredient.DisplayOrder),
                AlreadyAdded = recipe.RecipeIngredients.Any(ingredient => ingredient.IngredientId == request.IngredientId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound(new { error = "Recipe not found." });
        }

        if (recipe.UserId != userId)
        {
            return Results.Forbid();
        }

        if (recipe.AlreadyAdded)
        {
            return Results.Conflict(new { error = "This ingredient has already been added to the recipe." });
        }

        var ingredientName = await dbContext.Ingredients
            .Where(ingredient => ingredient.Id == request.IngredientId)
            .Select(ingredient => ingredient.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (ingredientName is null)
        {
            return Results.BadRequest(new { error = "Ingredient ID does not exist." });
        }

        // New ingredients are appended to the end of the list (display order is zero-based).
        var recipeIngredient = new RecipeIngredient(
            recipeId,
            request.IngredientId,
            request.Amount,
            unit,
            displayOrder: (recipe.LastDisplayOrder ?? -1) + 1);
        dbContext.RecipeIngredients.Add(recipeIngredient);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException
        {
            Number: UniqueIndexViolation or UniqueConstraintViolation
        })
        {
            // Another request added this ingredient, or another ingredient at the same position,
            // between the checks above and this save.
            return Results.Conflict(new { error = "The recipe's ingredients were modified concurrently. Please try again." });
        }

        return Results.Created($"/recipes/{recipeId}/ingredients/{recipeIngredient.IngredientId}", new
        {
            recipeIngredient.RecipeId,
            recipeIngredient.IngredientId,
            Name = ingredientName,
            recipeIngredient.Amount,
            recipeIngredient.Unit,
            recipeIngredient.DisplayOrder
        });
    }
}
