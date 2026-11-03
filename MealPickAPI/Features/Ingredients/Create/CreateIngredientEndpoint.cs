using MealPickAPI.Data;
using MealPickAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Features.Ingredients.Create;

public static class CreateIngredientEndpoint
{
    public static IEndpointRouteBuilder MapCreateIngredient(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/ingredients", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("Ingredients");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateIngredientRequest request,
        AppDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { error = "Name is required and must be 100 characters or fewer." });
        }

        var name = request.Name.Trim();
        if (name.Length > 100)
        {
            return Results.BadRequest(new { error = "Name is required and must be 100 characters or fewer." });
        }

        var alreadyExists = await dbContext.Ingredients
            .AnyAsync(ingredient => ingredient.Name == name);
        if (alreadyExists)
        {
            return Results.Conflict(new { error = "An ingredient with this name already exists." });
        }

        var ingredient = new Ingredient(name);
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/ingredients/{ingredient.Id}", new
        {
            ingredient.Id,
            ingredient.Name
        });
    }
}
