using System;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IDepartmentRepository Departments { get; }
        IDesignationRepository Designations { get; }
        IPermissionRepository Permissions { get; }
        ISettingRepository SystemSettings { get; }
        IMenuRepository Menus { get; }
        IUserSessionRepository Sessions { get; }
        IScheduleRepository Schedules { get; }
        IReportRepository Reports { get; }
        IAuditLogRepository AuditLogs { get; }
        IApprovalRepository Approvals { get; }
        IAccessRequestRepository AccessRequests { get; }
        IPurchaseRepository Purchases { get; }
        IInvoiceRepository Invoices { get; }
        IDashboardRepository Dashboard { get; }
        IProjectRepository Projects { get; }
        IProjectCategoryRepository ProjectCategories { get; }
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
