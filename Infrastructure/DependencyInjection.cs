using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBackend.Application.Common.Models;
using MyBackend.Application.Interfaces;
using MyBackend.Configuration;
using MyBackend.Domain.Interfaces;
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

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

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
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
