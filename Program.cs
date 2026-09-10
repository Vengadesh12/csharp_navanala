using Microsoft.Extensions.FileProviders;
using MyBackend.Api.Extensions;
using MyBackend.Api.Middleware;
using MyBackend.Application;
using MyBackend.Configuration;
using MyBackend.Infrastructure;
using MyBackend.Infrastructure.Persistence;

// ==============================================================================
// Clean Architecture Composition Root (MyBackend.Api/Program.cs)
// ==============================================================================

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------------------
// Dynamic Cloud / Container Port Configuration
// ------------------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// ------------------------------------------------------------------------------
// 1. Centralized Application Configuration (appsettings, config.json, env vars)
// ------------------------------------------------------------------------------
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("Config/config.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
Config.Load(builder.Configuration);

// ------------------------------------------------------------------------------
// 2. Clean Architecture Layer Dependency Registrations
// ------------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ------------------------------------------------------------------------------
// 3. API Presentation Services (Controllers, Swagger, JWT Auth, CORS)
// ------------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(kvp.Key),
                kvp => kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).ToArray()
            );

        var errorResponse = new MyBackend.Application.Common.DTO.ErrorResponse
        {
            Success = false,
            Message = "Validation failed.",
            Data = errors,
            Errors = errors.SelectMany(kv => kv.Value).ToList()
        };

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(errorResponse);
    };
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024; // 30 MB
});
builder.Services.AddApiSwagger();
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==============================================================================
// HTTP Request Processing Pipeline
// Order:
// 1. Exception Handling
// 2. Correlation ID & Security Headers
// 3. Structured Request Logging
// 4. Routing
// 5. Swagger Documentation
// 6. CORS
// 7. Static Files
// 8. Authentication
// 9. Active Session Validation
// 10. Authorization
// 11. Endpoints / Controllers
// ==============================================================================

var app = builder.Build();

// 1. Centralized Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

// 2. Correlation ID & Security Headers Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// 3. Centralized Structured Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. Routing
app.UseRouting();

// 5. OpenAPI / Swagger Documentation UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Userspace RBAC API v1.0");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Userspace RBAC API Documentation";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.EnableDeepLinking();
    options.EnablePersistAuthorization();
});

// 6. Cross-Origin Resource Sharing
app.UseCors("ReactPolicy");

// Ensure upload & report directories exist
var currentDir = Directory.GetCurrentDirectory();
var uploadsDirectory = Path.Combine(currentDir, "uploads");
if (!Directory.Exists(uploadsDirectory))
{
    Directory.CreateDirectory(uploadsDirectory);
}

var profilesDirectory = Path.Combine(uploadsDirectory, "profiles");
if (!Directory.Exists(profilesDirectory))
{
    Directory.CreateDirectory(profilesDirectory);
}

var reportDirectory = Path.Combine(currentDir, "report");
if (!Directory.Exists(reportDirectory))
{
    Directory.CreateDirectory(reportDirectory);
}

// 7. Static Files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDirectory),
    RequestPath = "/uploads"
});

// 8. Authentication
app.UseAuthentication();

// 9. Active Session Validation Middleware
app.UseMiddleware<ActiveSessionValidationMiddleware>();

// 10. Authorization
app.UseAuthorization();

// 11. Endpoints & Controllers
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "success",
        message = "C# Backend is running successfully!"
    });
});
app.MapControllers();

// 8. Database Auto-Migration & Schema Seed Initialization
await DatabaseInitializer.InitializeAsync(app.Services, app.Logger);

app.Run();
