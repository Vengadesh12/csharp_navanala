using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Database context abstraction for the application layer.
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<Department> Departments { get; }
        DbSet<Designation> Designations { get; }
        DbSet<Permission> Permissions { get; }
        DbSet<RolePermission> RolePermissions { get; }
        DbSet<DepartmentPermission> DepartmentPermissions { get; }
        DbSet<UserSession> UserSessions { get; }
        DbSet<Menu> Menus { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<Report> Reports { get; }
        DbSet<ReportCategory> ReportCategories { get; }
        DbSet<Project> Projects { get; }
        DbSet<ProjectCategory> ProjectCategories { get; }
        DbSet<ScheduleEvent> Schedules { get; }
        DbSet<SystemSetting> SystemSettings { get; }
        DbSet<SettingCategory> SettingCategories { get; }
        DbSet<EventType> EventTypes { get; }
        DbSet<ApprovalRequest> Approvals { get; }
        DbSet<Purchase> Purchases { get; }
        DbSet<Invoice> Invoices { get; }
        DbSet<InvoiceItem> InvoiceItems { get; }

        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
