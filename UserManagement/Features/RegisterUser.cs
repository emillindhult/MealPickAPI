using Microsoft.AspNetCore.Identity;
using UserManagement.Data;

namespace UserManagement.Features;

public static class RegisterUser
{
    public record Request(string Email, string Password, bool EnableNotifications = false);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("register", async (
            Request request,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager) =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EnableNotifications = request.EnableNotifications
            };

            IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                return Results.BadRequest(identityResult.Errors);
            }

            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);

            if (!addToRoleResult.Succeeded)
            {
                return Results.BadRequest(addToRoleResult.Errors);
            }

            await transaction.CommitAsync();

            return Results.Ok(user);
        });
    }
}
