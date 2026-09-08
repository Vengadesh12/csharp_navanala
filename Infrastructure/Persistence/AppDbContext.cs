using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<DesignationModel> Designations { get; set; }
        public DbSet<PermissionModel> Permissions { get; set; }
        public DbSet<RolePermissionModel> RolePermissions { get; set; }
        public DbSet<DepartmentPermissionModel> DepartmentPermissions { get; set; }
        public DbSet<UserSessionModel> UserSessions { get; set; }
        public DbSet<MenuModel> Menus { get; set; }
        public DbSet<AuditLogModel> AuditLogs { get; set; }
        public DbSet<ReportModel> Reports { get; set; }
        public DbSet<ReportCategoryModel> ReportCategories { get; set; }
        public DbSet<ProjectModel> Projects { get; set; }
        public DbSet<ProjectCategoryModel> ProjectCategories { get; set; }
        public DbSet<ScheduleEventModel> Schedules { get; set; }
        public DbSet<SystemSettingModel> SystemSettings { get; set; }
        public DbSet<SettingCategoryModel> SettingCategories { get; set; }
        public DbSet<EventTypeModel> EventTypes { get; set; }
        public DbSet<ApprovalRequestModel> Approvals { get; set; }
        public DbSet<AccessRequestModel> AccessRequests { get; set; }
        public DbSet<UserPermissionModel> UserPermissions { get; set; }
        public DbSet<PurchaseModel> Purchases { get; set; }
        public DbSet<InvoiceModel> Invoices { get; set; }
        public DbSet<InvoiceItemModel> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var utcNow = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entry.Properties.FirstOrDefault(p =>
                        string.Equals(p.Metadata.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase));
                    if (createdAtProp != null && (createdAtProp.CurrentValue == null || (createdAtProp.CurrentValue is DateTime dt && dt == default)))
                    {
                        createdAtProp.CurrentValue = utcNow;
                    }

                    var updatedAtProp = entry.Properties.FirstOrDefault(p =>
                        string.Equals(p.Metadata.Name, "UpdatedAt", StringComparison.OrdinalIgnoreCase));
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = utcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Never overwrite CreatedAt during an entity update
                    var createdAtProp = entry.Properties.FirstOrDefault(p =>
                        string.Equals(p.Metadata.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase));
                    if (createdAtProp != null)
                    {
                        createdAtProp.IsModified = false;
                    }

                    var updatedAtProp = entry.Properties.FirstOrDefault(p =>
                        string.Equals(p.Metadata.Name, "UpdatedAt", StringComparison.OrdinalIgnoreCase));
                    if (updatedAtProp != null)
                    {
                        updatedAtProp.CurrentValue = utcNow;
                    }
                }

                // Ensure all DateTime properties have DateTimeKind.Utc to satisfy PostgreSQL timestamptz
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.CurrentValue is DateTime dateTime && dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            prop.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }
}