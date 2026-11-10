using MealPickAPI.Data;
using MealPickAPI.Features.Ingredients.Create;
using MealPickAPI.Features.Recipes.Create;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// AddIdentityApiEndpoints also registers authentication (bearer token + cookie schemes).
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

var app = builder.Build();

await InitializeDatabaseAsync(app);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
}

app.MapIdentityApi<IdentityUser>();
app.MapCreateIngredient();
app.MapCreateRecipe();

app.Run();

// Development: apply migrations automatically, then seed.
// Other environments: migrations are applied by deployment; seed only once the schema is up to date.
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await dbContext.Database.MigrateAsync();
    }
    else if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
    {
        app.Logger.LogWarning("Skipping database seeding because there are pending migrations.");
        return;
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseSeeder.SeedAsync(dbContext, roleManager);
}
