using MealPickAPI.Domain;

namespace MealPickAPI.Features.Recipes.Create;

public sealed record CreateRecipeResponse(
    Guid Id,
    string UserId,
    string Title,
    string? Description,
    IEnumerable<CreateRecipeInstructionResponse> Instructions,
    IEnumerable<Guid> CategoryIds,
    IEnumerable<CreateRecipeIngredientResponse> Ingredients)
{
    public static CreateRecipeResponse From(Recipe recipe) => new(
        recipe.Id,
        recipe.UserId,
        recipe.Title,
        recipe.Description,
        recipe.Instructions
            .OrderBy(instruction => instruction.StepNumber)
            .Select(instruction => new CreateRecipeInstructionResponse(instruction.StepNumber, instruction.Text)),
        recipe.Categories.Select(category => category.Id),
        recipe.RecipeIngredients
            .OrderBy(ingredient => ingredient.DisplayOrder)
            .Select(ingredient => new CreateRecipeIngredientResponse(
                ingredient.IngredientId,
                ingredient.Amount,
                ingredient.Unit,
                ingredient.DisplayOrder)));
}

public sealed record CreateRecipeInstructionResponse(int StepNumber, string Text);

public sealed record CreateRecipeIngredientResponse(Guid IngredientId, decimal Amount, string Unit, int DisplayOrder);
