using Microsoft.EntityFrameworkCore.Storage;
using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    /// <summary>
    /// Coordinates operations across repositories and manages atomic transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// User repository instance.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Role repository instance.
        /// </summary>
        IRoleRepository Roles { get; }

        /// <summary>
        /// Department repository instance.
        /// </summary>
        IDepartmentRepository Departments { get; }

        /// <summary>
        /// Designation repository instance.
        /// </summary>
        IDesignationRepository Designations { get; }

        /// <summary>
        /// Permission repository instance.
        /// </summary>
        IPermissionRepository Permissions { get; }

        /// <summary>
        /// System settings repository instance.
        /// </summary>
        IRepository<SystemSetting> SystemSettings { get; }

        /// <summary>
        /// Navigation menus repository instance.
        /// </summary>
        IRepository<Menu> Menus { get; }

        /// <summary>
        /// User login and logout sessions repository instance.
        /// </summary>
        IUserSessionRepository Sessions { get; }

        /// <summary>
        /// Dynamic repository accessor for any entity type T.
        /// </summary>
        IRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Commits all pending changes to the underlying database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new atomic database transaction.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Commits the active transaction.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the active transaction.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
