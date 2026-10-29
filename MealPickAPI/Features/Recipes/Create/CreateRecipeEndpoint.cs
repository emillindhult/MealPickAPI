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
        if (!Guid.TryParse(userIdClaim, out var userId))
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

        var categoryIds = request.CategoryIds.Distinct().ToList();
        var ingredientIds = request.IngredientIds.Distinct().ToList();

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

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Descripton = request.Description,
            Instructions = request.Instructions,
            Categories = categories,
            Ingredients = ingredients
        };

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/recipes/{recipe.Id}", new
        {
            recipe.Id,
            recipe.UserId,
            recipe.Title,
            Description = recipe.Descripton,
            recipe.Instructions,
            CategoryIds = categories.Select(category => category.Id),
            IngredientIds = ingredients.Select(ingredient => ingredient.Id)
        });
    }
}
