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
// 1. Centralized Application Configuration (config.json, appsettings, env vars)
// ------------------------------------------------------------------------------
builder.Configuration.AddJsonFile("Config/config.json", optional: true, reloadOnChange: true);
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
// ==============================================================================

var app = builder.Build();

// 4. Centralized Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

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

// 6. Security & Static Files
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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDirectory),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ActiveSessionValidationMiddleware>();

// 7. Endpoints & Controllers
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
