using Microsoft.EntityFrameworkCore.Storage;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    /// <summary>
    /// Implements the Unit of Work pattern, coordinating repositories and database transaction lifecycles.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _currentTransaction;

        private IUserRepository? _users;
        private IRoleRepository? _roles;
        private IDepartmentRepository? _departments;
        private IDesignationRepository? _designations;
        private IPermissionRepository? _permissions;
        private IRepository<SystemSetting>? _systemSettings;
        private IRepository<Menu>? _menus;
        private IUserSessionRepository? _sessions;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
        public IDesignationRepository Designations => _designations ??= new DesignationRepository(_context);
        public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
        public IRepository<SystemSetting> SystemSettings => _systemSettings ??= new Repository<SystemSetting>(_context);
        public IRepository<Menu> Menus => _menus ??= new Repository<Menu>(_context);
        public IUserSessionRepository Sessions => _sessions ??= new UserSessionRepository(_context);

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = new Repository<T>(_context);
                _repositories[type] = repository;
            }
            return (IRepository<T>)repository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
            }
            await _context.DisposeAsync();
        }
    }
}
