using MealPickAPI.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : IdentityDbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeInstruction> RecipeInstructions => Set<RecipeInstruction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Recipe>(entity =>
        {
            entity.HasKey(recipe => recipe.Id);

            entity.Property(recipe => recipe.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(recipe => recipe.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(recipe => recipe.Description)
                .HasMaxLength(300);

            entity.HasIndex(recipe => recipe.UserId);

            entity.HasMany(recipe => recipe.Categories)
                .WithMany(category => category.Recipes)
                .UsingEntity(join => join.ToTable("RecipeCategories"));
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);

            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(category => category.Name)
                .IsUnique();
        });

        builder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(ingredient => ingredient.Id);

            entity.Property(ingredient => ingredient.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(ingredient => ingredient.Name)
                .IsUnique();
        });

        builder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(recipeIngredient => new
            {
                recipeIngredient.RecipeId,
                recipeIngredient.IngredientId
            });

            entity.Property(recipeIngredient => recipeIngredient.Amount)
                .HasPrecision(18, 3);

            entity.Property(recipeIngredient => recipeIngredient.Unit)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(recipeIngredient => new
            {
                recipeIngredient.RecipeId,
                recipeIngredient.DisplayOrder
            })
            .IsUnique();

            entity.HasOne(recipeIngredient => recipeIngredient.Recipe)
                .WithMany(recipe => recipe.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(recipeIngredient => recipeIngredient.Ingredient)
                .WithMany(ingredient => ingredient.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RecipeInstruction>(entity =>
        {
            entity.HasKey(instruction => instruction.Id);

            entity.Property(instruction => instruction.Text)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasIndex(instruction => new
            {
                instruction.RecipeId,
                instruction.StepNumber
            })
            .IsUnique();

            entity.HasOne(instruction => instruction.Recipe)
                .WithMany(recipe => recipe.Instructions)
                .HasForeignKey(instruction => instruction.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
