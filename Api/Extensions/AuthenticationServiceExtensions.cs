using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyBackend.Configuration;

namespace MyBackend.Api.Extensions
{
    // ==============================================================================
    // TOPIC: Authentication middleware & Authorization middleware configuration
    // ==============================================================================
    // Authentication Middleware:
    //  - Configures JWT Bearer authentication scheme.
    //  - Validates cryptographic signing key, issuer, and audience.
    //  - Translates token claims into HttpContext.User (ClaimsPrincipal).
    //  - Intercepts OnChallenge (401 Unauthorized) and OnForbidden (403 Forbidden)
    //    events to return uniform, structured JSON responses.
    //
    // Authorization Middleware:
    //  - Enrolls the ASP.NET Core Authorization service into DI.
    // ==============================================================================
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = Config.JwtKey;

            // TOPIC: Authentication middleware - JWT Bearer configuration
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = Config.JwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = Config.JwtAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }
                            return Task.CompletedTask;
                        },
                        // Request/Response manipulation on Authentication failure (401)
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var isExpired = context.Response.Headers.ContainsKey("Token-Expired");
                            var message = isExpired
                                ? "Your session has expired after 5 hours. Please log in again."
                                : "Authentication required. Please log in or provide a valid Bearer token.";
                            var payload = System.Text.Json.JsonSerializer.Serialize(new MyBackend.Application.Common.DTO.ErrorResponse
                            {
                                Success = false,
                                Message = message,
                                Data = null
                            }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                            await context.Response.WriteAsync(payload);
                        },
                        // Request/Response manipulation on Authorization failure (403)
                        OnForbidden = async context =>
                        {
                            context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            var payload = System.Text.Json.JsonSerializer.Serialize(new MyBackend.Application.Common.DTO.ErrorResponse
                            {
                                Success = false,
                                Message = "Access Denied. You do not possess the required permission to access this resource.",
                                Data = null
                            }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                            await context.Response.WriteAsync(payload);
                        }
                    };
                });

            // TOPIC: Authorization middleware - Dependency registration
            services.AddAuthorization();

            return services;
        }
    }
}
