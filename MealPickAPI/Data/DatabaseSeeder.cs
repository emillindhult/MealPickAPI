using MealPickAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Data;

public static class DatabaseSeeder
{
    private static readonly string[] CommonIngredientNames =
    [
        "All-purpose flour",
        "Black pepper",
        "Butter",
        "Eggs",
        "Garlic",
        "Granulated sugar",
        "Milk",
        "Olive oil",
        "Onion",
        "Salt"
    ];

    public static async Task SeedAsync(
        AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!result.Succeeded && !await roleManager.RoleExistsAsync("Admin"))
            {
                throw new InvalidOperationException("The Admin role could not be created.");
            }
        }

        var existingNames = (await dbContext.Ingredients
                .Select(ingredient => ingredient.Name)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingIngredients = CommonIngredientNames
            .Where(name => existingNames.Add(name))
            .Select(name => new Ingredient(name));

        dbContext.Ingredients.AddRange(missingIngredients);
        await dbContext.SaveChangesAsync();
    }
}
