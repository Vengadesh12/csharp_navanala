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

var app = builder.Build();

// ------------------------------------------------------------------------------
// TOPIC: Exception middleware & Custom middleware
// Catches all unhandled exceptions globally across the request pipeline.
// Translates exceptions into standardized JSON responses (e.g., 400, 401, 403, 404, 500)
// and ensures sensitive internal stack traces are suppressed in production.
// ------------------------------------------------------------------------------
app.UseMiddleware<ExceptionMiddleware>();

// ------------------------------------------------------------------------------
// TOPIC: Custom middleware & Request/Response manipulation
// Injects a unique X-Correlation-ID into HttpContext and response headers.
// Adds HTTP security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection).
// ------------------------------------------------------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();

// ------------------------------------------------------------------------------
// TOPIC: Logging middleware & Custom middleware
// Measures request execution duration, records HTTP method, sanitized path/query string,
// response status code, authenticated user identifier, and correlation ID.
// ------------------------------------------------------------------------------
app.UseMiddleware<RequestLoggingMiddleware>();

// ------------------------------------------------------------------------------
// TOPIC: Request pipeline - Endpoint Routing
// Matches incoming HTTP requests to route endpoints.
// ------------------------------------------------------------------------------
app.UseRouting();

// ------------------------------------------------------------------------------
// TOPIC: Request pipeline - OpenAPI / Swagger Documentation UI
// ------------------------------------------------------------------------------
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

// ------------------------------------------------------------------------------
// TOPIC: Request pipeline - Cross-Origin Resource Sharing (CORS)
// Must precede Authentication and Endpoints to allow cross-origin browser preflight requests.
// ------------------------------------------------------------------------------
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

// ------------------------------------------------------------------------------
// TOPIC: Request pipeline - Static Files
// Serves physical files from disk for avatars and uploaded documents.
// ------------------------------------------------------------------------------
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDirectory),
    RequestPath = "/uploads"
});

// ------------------------------------------------------------------------------
// TOPIC: Authentication middleware
// Validates incoming JWT Bearer tokens, decrypts/verifies signatures, and establishes
// the ClaimsPrincipal (HttpContext.User) for downstream authorization.
// ------------------------------------------------------------------------------
app.UseAuthentication();

// ------------------------------------------------------------------------------
// TOPIC: Custom middleware & Authentication middleware (Session Validation)
// Executes immediately after UseAuthentication to enforce database-backed session state.
// Verifies if an authenticated user's session was terminated/revoked by an administrator.
// ------------------------------------------------------------------------------
app.UseMiddleware<ActiveSessionValidationMiddleware>();

// ------------------------------------------------------------------------------
// TOPIC: Authorization middleware
// Enforces Role-Based Access Control (RBAC) and permission requirements.
// Evaluates [Authorize] attributes and policies on controller actions.
// ------------------------------------------------------------------------------
app.UseAuthorization();

// ------------------------------------------------------------------------------
// TOPIC: Request pipeline - Endpoint Execution
// Terminal stage of pipeline where routed controller actions are executed.
// ------------------------------------------------------------------------------
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
