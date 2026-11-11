using System.Security.Claims;
using MealPickAPI.Data;
using MealPickAPI.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Features.Instructions.Create;

public static class CreateInstructionEndpoint
{
    // SQL Server error numbers for unique index / unique constraint violations.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public static IEndpointRouteBuilder MapCreateInstruction(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/recipes/{recipeId:guid}/instructions", HandleAsync)
            .RequireAuthorization()
            .WithTags("Instructions");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid recipeId,
        CreateInstructionRequest request,
        ClaimsPrincipal user,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var text = request.Text?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > RecipeInstruction.TextMaxLength)
        {
            return Results.BadRequest(new { error = $"Text is required and must be {RecipeInstruction.TextMaxLength} characters or fewer." });
        }

        var recipe = await dbContext.Recipes
            .Where(recipe => recipe.Id == recipeId)
            .Select(recipe => new
            {
                recipe.UserId,
                LastStepNumber = recipe.Instructions.Max(instruction => (int?)instruction.StepNumber)
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

        // New instructions are appended as the next step.
        var instruction = new RecipeInstruction(recipeId, text, stepNumber: (recipe.LastStepNumber ?? 0) + 1);
        dbContext.RecipeInstructions.Add(instruction);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException
        {
            Number: UniqueIndexViolation or UniqueConstraintViolation
        })
        {
            // Another request added a step to this recipe between the query above and this save.
            return Results.Conflict(new { error = "The recipe's instructions were modified concurrently. Please try again." });
        }

        return Results.Created($"/recipes/{recipeId}/instructions/{instruction.Id}", new
        {
            instruction.Id,
            instruction.RecipeId,
            instruction.StepNumber,
            instruction.Text
        });
    }
}
