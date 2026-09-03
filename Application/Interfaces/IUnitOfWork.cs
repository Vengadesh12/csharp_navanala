using System;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IDepartmentRepository Departments { get; }
        IDesignationRepository Designations { get; }
        IPermissionRepository Permissions { get; }
        IRepository<SystemSetting> SystemSettings { get; }
        IRepository<Menu> Menus { get; }
        IUserSessionRepository Sessions { get; }
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
