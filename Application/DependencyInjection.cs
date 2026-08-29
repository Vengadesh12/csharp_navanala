using Microsoft.Extensions.DependencyInjection;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Services;

namespace MyBackend.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Core application & business services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportCategoryService, ReportCategoryService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IUserActivityService, UserActivityService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IPurchaseService, PurchaseService>();
            services.AddScoped<IInvoiceService, InvoiceService>();

            // Singleton in-memory OTP generator/validator
            services.AddSingleton<IOtpService, OtpService>();

            return services;
        }
    }
}
