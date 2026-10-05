using MealPickAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace MealPickAPI.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Categories)
            .WithMany(c => c.Recipes)
            .UsingEntity(j => j.ToTable("RecipeCategories"));

        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithMany(i => i.Recipes)
            .UsingEntity(j => j.ToTable("RecipeIngredients"));

        modelBuilder.Entity<Recipe>()
            .Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Recipe>()
            .Property(r => r.Descripton)
            .IsRequired(false)
            .HasMaxLength(300);

        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Quantity)
            .IsRequired()
            .HasMaxLength(50);
    }
}
