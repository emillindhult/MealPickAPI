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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Recipe>()
            .HasKey(r => r.Id);

        builder.Entity<Recipe>()
            .HasMany(r => r.Categories)
            .WithMany(c => c.Recipes)
            .UsingEntity(j => j.ToTable("RecipeCategories"));

        builder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithMany(i => i.Recipes)
            .UsingEntity(j => j.ToTable("RecipeIngredients"));

        builder.Entity<Recipe>()
            .Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Entity<Recipe>()
            .Property(r => r.Descripton)
            .IsRequired(false)
            .HasMaxLength(300);

        builder.Entity<Category>()
            .HasKey(c => c.Id);

        builder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired();

        builder.Entity<Ingredient>()
            .HasKey(i => i.Id);

        builder.Entity<Ingredient>()
            .Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Entity<Ingredient>()
            .Property(i => i.Quantity)
            .IsRequired()
            .HasMaxLength(50);

        base.OnModelCreating(builder);
    }
}
