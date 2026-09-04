using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using MyBackend.Infrastructure.Persistence;
using MyBackend.Infrastructure.Repositories;
using MyBackend.Infrastructure.Services;

namespace MyBackend.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // PostgreSQL connection
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Config.DbConnectionString)
            );

            // Email Settings & Service
            services.Configure<EmailSettings>(options =>
            {
                var settings = Config.ToEmailSettings();
                options.SmtpServer = settings.SmtpServer;
                options.Port = settings.Port;
                options.SenderName = settings.SenderName;
                options.SenderEmail = settings.SenderEmail;
                options.AppPassword = settings.AppPassword;
                options.EnableSsl = settings.EnableSsl;
                options.TimeoutSeconds = settings.TimeoutSeconds;
            });

            services.AddScoped<IEmailService, GmailEmailService>();

            // Repositories and Unit of Work
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IApprovalRepository, ApprovalRepository>();
            services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectCategoryRepository, ProjectCategoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IFileService, FileService>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddInfrastructureServices(configuration);
        }
    }
}
