using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyBackend.Configuration;
using MyBackend.Domain.Models;

namespace MyBackend.Api.Middleware
{
    // ==============================================================================
    // TOPIC: Custom middleware & Authentication middleware
    // ==============================================================================
    // ActiveSessionValidationMiddleware:
    //  - Executes in the Request Pipeline immediately after UseAuthentication().
    //  - Bridges stateless JWT Bearer tokens with stateful database session tracking.
    //  - Enforces immediate session termination if an administrator revokes access,
    //    even if the JWT token has not reached its cryptographic expiration time.
    //  - Enforces automatic session expiration after 5 hours (300 minutes) of login.
    //  - Tracks client IP and heartbeat timestamps to reflect real-time active users.
    // ==============================================================================
    public class ActiveSessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveSessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ==============================================================================
        // TOPIC: Authentication middleware & Request/Response manipulation
        // Inspects Bearer token for authenticated users; short-circuits with 401 JSON
        // response if the session has expired after 5 hours or been revoked.
        // ==============================================================================
        public async Task InvokeAsync(HttpContext context, IUserSessionRepository sessionRepository)
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
                        var session = await sessionRepository.FindActiveSessionByTokenAsync(userId, token);

                        if (session != null)
                        {
                            // 1. Enforce automatic session logout after 5 hours (300 minutes)
                            var maxMinutes = Config.SessionTimeoutMinutes > 0 ? Config.SessionTimeoutMinutes : 300;
                            var sessionAge = DateTime.UtcNow - session.LoginTime;
                            if (sessionAge.TotalMinutes >= maxMinutes)
                            {
                                session.IsActive = false;
                                session.LogoutTime = DateTime.UtcNow;
                                session.UpdatedAt = DateTime.UtcNow;
                                await sessionRepository.TerminateSessionAsync(session.Id);

                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync("{\"message\":\"Your session has expired after 5 hours. Please log in again.\"}");
                                return;
                            }

                            // 2. Request/Response manipulation: Short-circuit pipeline if session is revoked
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
                                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                                await sessionRepository.TouchSessionAsync(session.Id, clientIp);
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

                            var now = DateTime.UtcNow;
                            var newSession = new UserSessionModel
                            {
                                UserId = userId,
                                Email = email.Trim(),
                                UserName = userName.Trim(),
                                IpAddress = string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp.Trim(),
                                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                                SessionToken = token,
                                LoginTime = now,
                                LogoutTime = null,
                                IsActive = true,
                                DeletedFlag = 1,
                                CreatedAt = now,
                                UpdatedAt = now
                            };

                            await sessionRepository.AddSessionAsync(newSession);
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
