using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyBackend.Configuration;

namespace MyBackend.Api.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = Config.JwtKey;

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
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var payload = System.Text.Json.JsonSerializer.Serialize(new MyBackend.Application.Common.DTO.ErrorResponse
                            {
                                Success = false,
                                Message = "Authentication required. Please log in or provide a valid Bearer token.",
                                Data = null
                            }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                            await context.Response.WriteAsync(payload);
                        },
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

            services.AddAuthorization();

            return services;
        }
    }
}
