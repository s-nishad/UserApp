using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using UserApp.Models;

namespace UserApp.Middleware
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Check if the user is authenticated
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                // Get the user's ID from the claims
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    // Find the user in the database
                    var user = await dbContext.Users.FindAsync(int.Parse(userId));

                    // Check if the user exists and is not blocked
                    if (user == null || user.Status == false)
                    {
                        // Sign out the user
                        await context.SignOutAsync();

                        // Redirect to the login page
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                }
            }

            // Continue to the next middleware
            await _next(context);
        }
    }
}