using Microsoft.EntityFrameworkCore.Storage;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
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
        private ISettingRepository? _systemSettings;
        private IMenuRepository? _menus;
        private IUserSessionRepository? _sessions;
        private IScheduleRepository? _schedules;
        private IReportRepository? _reports;
        private IAuditLogRepository? _auditLogs;
        private IApprovalRepository? _approvals;
        private IAccessRequestRepository? _accessRequests;
        private IPurchaseRepository? _purchases;
        private IInvoiceRepository? _invoices;
        private IDashboardRepository? _dashboard;
        private IProjectRepository? _projects;
        private IProjectCategoryRepository? _projectCategories;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
        public IDesignationRepository Designations => _designations ??= new DesignationRepository(_context);
        public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
        public ISettingRepository SystemSettings => _systemSettings ??= new SettingRepository(_context);
        public IMenuRepository Menus => _menus ??= new MenuRepository(_context);
        public IUserSessionRepository Sessions => _sessions ??= new UserSessionRepository(_context);
        public IScheduleRepository Schedules => _schedules ??= new ScheduleRepository(_context);
        public IReportRepository Reports => _reports ??= new ReportRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        public IApprovalRepository Approvals => _approvals ??= new ApprovalRepository(_context);
        public IAccessRequestRepository AccessRequests => _accessRequests ??= new AccessRequestRepository(_context);
        public IPurchaseRepository Purchases => _purchases ??= new PurchaseRepository(_context);
        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
        public IDashboardRepository Dashboard => _dashboard ??= new DashboardRepository(_context);
        public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
        public IProjectCategoryRepository ProjectCategories => _projectCategories ??= new ProjectCategoryRepository(_context);

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

        public async Task<IDbTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return new DbTransactionWrapper(_currentTransaction);
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

    public class DbTransactionWrapper : IDbTransaction
    {
        private readonly IDbContextTransaction _transaction;
        public DbTransactionWrapper(IDbContextTransaction transaction) => _transaction = transaction;

        public Task CommitAsync(CancellationToken cancellationToken = default) => _transaction.CommitAsync(cancellationToken);
        public Task RollbackAsync(CancellationToken cancellationToken = default) => _transaction.RollbackAsync(cancellationToken);
        public void Dispose() => _transaction.Dispose();
        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
