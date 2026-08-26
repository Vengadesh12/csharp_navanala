using MyBackend.Api.Extensions;
using MyBackend.Api.Middlewares;
using MyBackend.Application;
using MyBackend.Configuration;
using MyBackend.Infrastructure;
using MyBackend.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Bind to dynamic PORT environment variable on cloud (Render/Railway), otherwise default to launchSettings.json (port 5125)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// 1. Initialize centralized application configuration
Config.Load(builder.Configuration);

// 2. Add API Presentation Services (Controllers, Swagger, JWT Auth)
builder.Services.AddControllers();
builder.Services.AddApiSwagger();
builder.Services.AddApiAuthentication(builder.Configuration);

// 3. Register Clean Architecture Layer Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. CORS Policy for Client Application
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

// 5. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 6. OpenAPI / Swagger Documentation
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

// 7. Security & Routing Middleware
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ActiveSessionValidationMiddleware>();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

// 8. Initialize Database Tables and Seeds
await DatabaseInitializer.InitializeAsync(app.Services, app.Logger);

app.Run();