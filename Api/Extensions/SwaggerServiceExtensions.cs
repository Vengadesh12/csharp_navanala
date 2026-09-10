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
                    
                    ### Key Backend Capabilities:
                    - **Request Pipeline & Middleware**: Centralized Exception Handling, Correlation ID tracking (`X-Correlation-ID`), Structured Request Logging, Authentication, Active Session Validation, and Centralized Authorization.
                    - **Hierarchical RBAC with Permission Inheritance**: Role hierarchy (Super Admin -> Admin -> Manager -> Employee), deterministic conflict resolution (`Explicit Child Deny > Explicit Child Allow > Inherited Deny > Inherited Allow > Default Deny`), Menu-level and Action-level permissions, and privilege escalation prevention.
                    - **Dynamic Query Parameters & Composable Filters**: Dynamic filtering (`?status=active&category=internal&department=IT&search=test`), range filtering (`minAmount/maxAmount`, `startDate/endDate`, `minAge/maxAge`, `minSalary/maxSalary`), database-level sorting (`sortBy/sortOrder`), and pagination (`page/pageSize`).
                    - **Dynamic Field Selection**: Selectable whitelisted response fields (`?fields=id,name,email` or `?include=...` / `?exclude=...`) with strict sensitive data redaction.
                    - **Model & Cross-Field Validation**: Consistent 400 Bad Request responses for DataAnnotations, conditional, and cross-field validation failures (`Password != Email`, `StartDate <= EndDate`).
                    
                    ### Standard HTTP Status Codes:
                    - `200 OK`: Request succeeded.
                    - `201 Created`: Resource successfully provisioned.
                    - `400 Bad Request`: Input validation failed or invalid range query (`startDate > endDate` or `min > max`).
                    - `401 Unauthorized`: Authentication token is missing, expired, or invalid.
                    - `403 Forbidden`: Authenticated user lacks authorization or attempted privilege escalation.
                    - `404 Not Found`: Target resource was not found.
                    - `409 Conflict`: Concurrency conflict or duplicate unique resource.
                    - `500 Internal Server Error`: Unhandled server exception (safe response without internal stack trace or credential leaks).
                    
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
