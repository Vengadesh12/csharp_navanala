using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Middlewares
{
    /// <summary>
    /// Validates that authenticated incoming requests correspond to an active session in the database.
    /// Rejects requests from terminated/force-logged-out sessions with 401 Unauthorized.
    /// </summary>
    public class ActiveSessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveSessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Skip public or authentication endpoints
            if (path.StartsWith("/swagger") ||
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/logout") ||
                path.StartsWith("/api/auth/forgot-password") ||
                path.StartsWith("/api/auth/verify-otp") ||
                path.StartsWith("/api/auth/reset-password") ||
                path.StartsWith("/api/auth/validate-password") ||
                path.StartsWith("/api/auth/resend-2fa-otp") ||
                path.StartsWith("/api/auth/login-2fa-verify"))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                if (int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) && userId > 0)
                {
                    var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        var session = await dbContext.UserSessions
                            .AsNoTracking()
                            .Where(s => s.UserId == userId && s.SessionToken == token && s.DeletedFlag == 1)
                            .OrderByDescending(s => s.LoginTime)
                            .FirstOrDefaultAsync();

                        if (session != null && (!session.IsActive || session.LogoutTime != null))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"message\":\"Your session has been terminated by an administrator. Please log in again.\"}");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
