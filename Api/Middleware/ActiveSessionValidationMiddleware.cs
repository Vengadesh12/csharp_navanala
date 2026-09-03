using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Middleware
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
                            .Where(s => s.UserId == userId && s.SessionToken == token && s.DeletedFlag == 1)
                            .OrderByDescending(s => s.LoginTime)
                            .FirstOrDefaultAsync();

                        if (session != null)
                        {
                            if (!session.IsActive || session.LogoutTime != null)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync("{\"message\":\"Your session has been terminated by an administrator. Please log in again.\"}");
                                return;
                            }

                            // Keep active session alive by updating UpdatedAt timestamp (throttled every 15s)
                            var now = DateTime.UtcNow;
                            if (session.UpdatedAt == null || (now - session.UpdatedAt.Value).TotalSeconds >= 15)
                            {
                                session.UpdatedAt = now;
                                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                                if (!string.IsNullOrWhiteSpace(clientIp) && session.IpAddress != clientIp)
                                {
                                    session.IpAddress = clientIp;
                                }
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            // User is validly authenticated with JWT, but session row is missing from database.
                            // Auto-register the active session so the user immediately shows up in active user tracking.
                            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                            var userAgent = context.Request.Headers.UserAgent.ToString();
                            var email = context.User.FindFirstValue(ClaimTypes.Email)
                                ?? context.User.FindFirstValue("email")
                                ?? string.Empty;
                            var userName = context.User.FindFirstValue(ClaimTypes.Name)
                                ?? context.User.FindFirstValue("name")
                                ?? (email.Contains('@') ? email.Split('@')[0] : $"User #{userId}");

                            var newSession = MyBackend.Domain.Entities.UserSession.Start(
                                userId: userId,
                                email: email,
                                userName: userName,
                                ipAddress: string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp,
                                userAgent: string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                                sessionToken: token
                            );

                            dbContext.UserSessions.Add(newSession);
                            try
                            {
                                await dbContext.SaveChangesAsync();
                            }
                            catch
                            {
                                // Ignore concurrency collision if another request created it simultaneously
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
