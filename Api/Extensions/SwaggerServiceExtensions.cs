using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace MyBackend.Api.Extensions
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddApiSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Userspace RBAC & Access Management API",
                    Version = "v1.0",
                    Description = """
                    ## Overview
                    Enterprise Role-Based Access Control (RBAC) and User Management API built with Clean Architecture.
                    
                    ### Modules:
                    - **Authentication & Sessions**: Secure JWT bearer token issuance and permission resolution.
                    - **User Directory**: Full lifecycle user management, role assignments, soft deletion, and restoration.
                    - **Role Management**: Define, modify, and audit workspace roles.
                    - **Permission Matrix**: Fine-grained capability mapping and role-permission enforcement.
                    - **Dynamic Navigation Menus**: RBAC-filtered navigation structure for client applications.
                    - **Email Notifications**: Transactional Gmail dispatch for newly provisioned user credentials & OTPs.
                    - **Account Security & Password Recovery**: Secure OTP-based self-service password recovery with strong password enforcement.
                    - **Projects & Audits**: Projects tracking, scheduling, and system audit logs.
                    - **System Settings**: Configurable platform parameters and categories.
                    
                    ### Authentication:
                    Authenticate via `POST /api/auth/login`, copy the resulting JWT token, click **Authorize** at the top right, and paste your token.
                    """,
                    Contact = new OpenApiContact
                    {
                        Name = "RBAC Engineering Team",
                        Email = "dev@example.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Proprietary / Enterprise"
                    }
                });

                // Include generated XML documentation comments if present
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token directly."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return services;
        }
    }
}
