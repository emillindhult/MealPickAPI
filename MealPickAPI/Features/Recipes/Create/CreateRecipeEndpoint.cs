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
        AppDbContext dbContext)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 100)
        {
            return Results.BadRequest(new { error = "Title is required and must be 100 characters or fewer." });
        }

        if (request.Description is not null && request.Description.Length > 300)
        {
            return Results.BadRequest(new { error = "Description must be 300 characters or fewer." });
        }

        if (request.Instructions.Any(instruction =>
                string.IsNullOrWhiteSpace(instruction.Text) || instruction.Text.Length > 2000))
        {
            return Results.BadRequest(new { error = "Instructions are required and must be 2,000 characters or fewer." });
        }

        if (request.Ingredients.Any(ingredient =>
                ingredient.Amount <= 0 ||
                string.IsNullOrWhiteSpace(ingredient.Unit) ||
                ingredient.Unit.Length > 20))
        {
            return Results.BadRequest(new { error = "Each ingredient must have a positive amount and a unit of 20 characters or fewer." });
        }

        var categoryIds = request.CategoryIds.Distinct().ToList();
        var ingredientRequests = request.Ingredients.ToList();
        var ingredientIds = ingredientRequests.Select(ingredient => ingredient.IngredientId).ToList();

        if (ingredientIds.Count != ingredientIds.Distinct().Count())
        {
            return Results.BadRequest(new { error = "An ingredient can only be added once to a recipe." });
        }

        var categories = await dbContext.Categories
            .Where(category => categoryIds.Contains(category.Id))
            .ToListAsync();
        var ingredients = await dbContext.Ingredients
            .Where(ingredient => ingredientIds.Contains(ingredient.Id))
            .ToListAsync();

        if (categories.Count != categoryIds.Count || ingredients.Count != ingredientIds.Count)
        {
            return Results.BadRequest(new { error = "One or more category or ingredient IDs do not exist." });
        }

        var recipe = new Recipe(userIdClaim, request.Title.Trim(), request.Description?.Trim());

        foreach (var category in categories)
        {
            recipe.Categories.Add(category);
        }

        for (var index = 0; index < ingredientRequests.Count; index++)
        {
            var ingredient = ingredientRequests[index];
            recipe.AddIngredient(
                ingredient.IngredientId,
                ingredient.Amount,
                ingredient.Unit.Trim(),
                index);
        }

        for (var index = 0; index < request.Instructions.Count; index++)
        {
            var instruction = request.Instructions[index];
            recipe.AddInstruction(instruction.Text.Trim(), index + 1);
        }

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/recipes/{recipe.Id}", new
        {
            recipe.Id,
            recipe.UserId,
            recipe.Title,
            Description = recipe.Description,
            Instructions = recipe.Instructions
                .OrderBy(instruction => instruction.StepNumber)
                .Select(instruction => new
                {
                    instruction.StepNumber,
                    instruction.Text
                }),
            CategoryIds = categories.Select(category => category.Id),
            Ingredients = recipe.RecipeIngredients
                .OrderBy(ingredient => ingredient.DisplayOrder)
                .Select(ingredient => new
                {
                    ingredient.IngredientId,
                    ingredient.Amount,
                    ingredient.Unit,
                    ingredient.DisplayOrder
                })
        });
    }
}
