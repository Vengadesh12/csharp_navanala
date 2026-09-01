using Microsoft.EntityFrameworkCore;
using MyBackend.Api.Extensions;
using MyBackend.Api.Middlewares;
using MyBackend.Application.Common.Models;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Services;
using MyBackend.Configuration;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;
using MyBackend.Infrastructure.Repositories;
using MyBackend.Infrastructure.Services;

// ==============================================================================
// Clean Architecture Composition Root (Program.cs)
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
// 1. Centralized Application Configuration
// ------------------------------------------------------------------------------
Config.Load(builder.Configuration);

// ------------------------------------------------------------------------------
// 2. Infrastructure Layer Registration (Persistence, Email, Repositories, UnitOfWork)
// ------------------------------------------------------------------------------
// PostgreSQL DbContext & Interface
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Config.DbConnectionString)
);
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// Email Communication Settings & Service
builder.Services.Configure<EmailSettings>(options =>
{
    var settings = Config.ToEmailSettings();
    options.SmtpServer = settings.SmtpServer;
    options.Port = settings.Port;
    options.SenderName = settings.SenderName;
    options.SenderEmail = settings.SenderEmail;
    options.AppPassword = settings.AppPassword;
    options.EnableSsl = settings.EnableSsl;
});
builder.Services.AddScoped<IEmailService, GmailEmailService>();

// Domain Repository Interfaces & Implementations
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ------------------------------------------------------------------------------
// 3. Application Layer Registration (Domain Business Services & Interfaces)
// ------------------------------------------------------------------------------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportCategoryService, ReportCategoryService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddSingleton<IOtpService, OtpService>();

// ------------------------------------------------------------------------------
// 4. API Presentation Layer Registration (Controllers, Swagger OpenAPI, JWT Auth, CORS)
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

// 5. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 6. OpenAPI / Swagger Documentation UI
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

// 7. Security & Routing Middlewares
app.UseCors("ReactPolicy");

// Static File Hosting for User Uploads (Profile pictures, etc.)
var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsDirectory))
{
    Directory.CreateDirectory(uploadsDirectory);
}

var profilesDirectory = Path.Combine(uploadsDirectory, "profiles");
if (!Directory.Exists(profilesDirectory))
{
    Directory.CreateDirectory(profilesDirectory);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDirectory),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ActiveSessionValidationMiddleware>();

// 8. Endpoints & Controllers
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "success",
        message = "C# Backend is running successfully!"
    });
});
app.MapControllers();

// 9. Database Auto-Migration & Schema Seed Initialization
await DatabaseInitializer.InitializeAsync(app.Services, app.Logger);

app.Run();