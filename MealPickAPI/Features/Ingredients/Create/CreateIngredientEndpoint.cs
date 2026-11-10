using MealPickAPI.Data;
using MealPickAPI.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Features.Ingredients.Create;

public static class CreateIngredientEndpoint
{
    // SQL Server error numbers for unique index / unique constraint violations.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static IEndpointRouteBuilder MapCreateIngredient(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/ingredients", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("Ingredients");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateIngredientRequest request,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > Ingredient.NameMaxLength)
        {
            return Results.BadRequest(new { error = $"Name is required and must be {Ingredient.NameMaxLength} characters or fewer." });
        }

        var alreadyExists = await dbContext.Ingredients
            .AnyAsync(ingredient => ingredient.Name == name, cancellationToken);
        if (alreadyExists)
        {
            return DuplicateName();
        }

        var ingredient = new Ingredient(name);
        dbContext.Ingredients.Add(ingredient);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException
        {
            Number: UniqueIndexViolation or UniqueConstraintViolation
        })
        {
            // Another request inserted the same name between the check above and this save.
            return DuplicateName();
        }

        return Results.Created($"/ingredients/{ingredient.Id}", new
        {
            ingredient.Id,
            ingredient.Name
        });
    }

    private static IResult DuplicateName() =>
        Results.Conflict(new { error = "An ingredient with this name already exists." });
}
