using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Services;
using MyBackend.Domain.Models;
using Xunit;

namespace UnitTests.Application
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC)
    // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
    // TOPIC: Permission conflict resolution
    // Unit tests validating:
    //  - Super Admin full capabilities
    //  - Parent-to-child permission inheritance (e.g. Employee inherits from Manager)
    //  - Deterministic 5-tier conflict resolution (Explicit Child Deny > Explicit Child Allow > Inherited Deny > Inherited Allow > Default Deny)
    //  - Cycle detection & privilege escalation guards
    // ==============================================================================
    public class HierarchicalRbacTests
    {
        private class InMemoryRepo<T> : IRepository<T> where T : class
        {
            public List<T> Items { get; } = new();

            public Task<T?> GetByIdAsync(int id)
            {
                var prop = typeof(T).GetProperty("Id");
                var item = Items.FirstOrDefault(x => (int)(prop?.GetValue(x) ?? 0) == id);
                return Task.FromResult(item);
            }

            public Task<List<T>> ListAllAsync() => Task.FromResult(Items.ToList());

            public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
            {
                return Task.FromResult(Items.AsQueryable().Where(predicate).ToList());
            }

            public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            {
                return Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));
            }

            public Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null)
            {
                return Task.FromResult(predicate == null ? Items.Any() : Items.AsQueryable().Any(predicate));
            }

            public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            {
                return Task.FromResult(predicate == null ? Items.Count : Items.AsQueryable().Count(predicate));
            }

            public Task AddAsync(T entity)
            {
                Items.Add(entity);
                return Task.CompletedTask;
            }

            public Task AddRangeAsync(IEnumerable<T> entities)
            {
                Items.AddRange(entities);
                return Task.CompletedTask;
            }

            public void Update(T entity) { }
            public void Delete(T entity) => Items.Remove(entity);
            public void DeleteRange(IEnumerable<T> entities)
            {
                foreach (var e in entities) Items.Remove(e);
            }
        }

        private class FakePermissionRepo : InMemoryRepo<PermissionModel>, IPermissionRepository
        {
            public Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync() => Task.FromResult(new PermissionsMatrixResponse());

            public Task<List<PermissionDto>> GetAllActivePermissionsAsync()
            {
                return Task.FromResult(Items.Where(p => p.DeletedFlag == 1).Select(p => new PermissionDto
                {
                    PermissionKey = p.PermissionKey,
                    Name = p.Name,
                    Description = p.Description,
                    IsAssigned = 1
                }).ToList());
            }

            public Task<List<string>> GetPermissionKeysByRoleIdAsync(int roleId) => Task.FromResult(new List<string>());
            public Task<bool> UpdateRolePermissionsAsync(int roleId, IEnumerable<string> permissionKeys) => Task.FromResult(true);
            public Task<bool> UpdateRolePermissionsWithRulesAsync(int roleId, IEnumerable<UpdateRolePermissionRule> rules) => Task.FromResult(true);
            public Task<List<string>> GetPermissionKeysByDepartmentIdAsync(int departmentId) => Task.FromResult(new List<string>());
            public Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, IEnumerable<string> permissionKeys) => Task.FromResult(true);
        }

        private class FakeUserRepo : InMemoryRepo<UserModel>, IUserRepository
        {
            public Task<UserModel?> GetUserByIdAsync(int id) => GetByIdAsync(id);
            public Task<UserModel?> GetByEmailAsync(string email) => Task.FromResult(Items.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
            public Task<UserLoginDetailsModel?> GetLoginUserDetailsByEmailAsync(string email) => Task.FromResult<UserLoginDetailsModel?>(null);
            public Task<List<UserModel>> GetAllUsersAsync() => ListAllAsync();
            public Task<bool> SetDeletedFlagAsync(int id, int deletedFlag) => Task.FromResult(true);
            public Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys) => Task.FromResult(true);
            public Task<List<string>> GetUserPermissionKeysAsync(int userId, int? roleId = null, int? designationId = null) => Task.FromResult(new List<string>());
            public Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash) => Task.FromResult(true);
            public Task<Dictionary<int, string>> GetActiveRolesLookupAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<Dictionary<int, string>> GetActiveDesignationsLookupAsync() => Task.FromResult(new Dictionary<int, string>());
            public Task<string?> GetRoleNameByIdAsync(int roleId) => Task.FromResult<string?>(null);
            public Task<string?> GetDesignationNameByIdAsync(int designationId) => Task.FromResult<string?>(null);
            public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null) => Task.FromResult(false);
            public Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null) => Task.FromResult(false);
            public Task<int> GetActiveUsersCountAsync() => Task.FromResult(Items.Count);
            public Task<int> GetUsersWithRoleCountAsync() => Task.FromResult(Items.Count);
            public Task<List<string>> GetUserPermissionKeysForProfileAsync(int roleId, int designationId) => Task.FromResult(new List<string>());
            public Task<Dictionary<int, string>> GetUserRoleMapAsync() => Task.FromResult(new Dictionary<int, string>());
        }

        private class FakeRoleRepo : InMemoryRepo<RoleModel>, IRoleRepository
        {
            public Task<List<RoleModel>> GetActiveRolesAsync() => Task.FromResult(Items.Where(r => r.DeletedFlag == 1).ToList());
            public Task<RoleModel?> GetActiveRoleByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(r => r.Id == id && r.DeletedFlag == 1));
            public Task<bool> SetDeletedFlagAsync(int id, int deletedFlag) => throw new NotImplementedException();
            public Task<Dictionary<int, string>> GetRoleNameDictionaryAsync() => Task.FromResult(Items.ToDictionary(r => r.Id, r => r.Name));
        }

        private class TestUnitOfWork : IUnitOfWork
        {
            public FakeUserRepo UsersRepo { get; } = new();
            public FakeRoleRepo RolesRepo { get; } = new();
            public FakePermissionRepo PermsRepo { get; } = new();
            public InMemoryRepo<RolePermissionModel> RolePermsRepo { get; } = new();
            public InMemoryRepo<UserPermissionModel> UserPermsRepo { get; } = new();

            public IUserRepository Users => UsersRepo;
            public IRoleRepository Roles => RolesRepo;
            public IPermissionRepository Permissions => PermsRepo;
            public IDepartmentRepository Departments => throw new NotImplementedException();
            public IDesignationRepository Designations => throw new NotImplementedException();
            public ISettingRepository SystemSettings => throw new NotImplementedException();
            public IMenuRepository Menus => throw new NotImplementedException();
            public IUserSessionRepository Sessions => throw new NotImplementedException();
            public IScheduleRepository Schedules => throw new NotImplementedException();
            public IReportRepository Reports => throw new NotImplementedException();
            public IAuditLogRepository AuditLogs => throw new NotImplementedException();
            public IApprovalRepository Approvals => throw new NotImplementedException();
            public IAccessRequestRepository AccessRequests => throw new NotImplementedException();
            public IPurchaseRepository Purchases => throw new NotImplementedException();
            public IInvoiceRepository Invoices => throw new NotImplementedException();
            public IDashboardRepository Dashboard => throw new NotImplementedException();
            public IProjectRepository Projects => throw new NotImplementedException();
            public IProjectCategoryRepository ProjectCategories => throw new NotImplementedException();

            public IRepository<T> Repository<T>() where T : class
            {
                if (typeof(T) == typeof(RolePermissionModel)) return (IRepository<T>)RolePermsRepo;
                if (typeof(T) == typeof(UserPermissionModel)) return (IRepository<T>)UserPermsRepo;
                if (typeof(T) == typeof(RoleModel)) return (IRepository<T>)RolesRepo;
                if (typeof(T) == typeof(PermissionModel)) return (IRepository<T>)PermsRepo;
                if (typeof(T) == typeof(UserModel)) return (IRepository<T>)UsersRepo;
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
            public Task<IDbTransaction> BeginTransactionAsync() => throw new NotImplementedException();
            public Task CommitTransactionAsync() => Task.CompletedTask;
            public Task RollbackTransactionAsync() => Task.CompletedTask;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }

        private (PermissionHierarchyService Service, TestUnitOfWork Uow) CreateTestSetup()
        {
            var uow = new TestUnitOfWork();
            var logger = NullLogger<PermissionHierarchyService>.Instance;
            var service = new PermissionHierarchyService(uow, logger);
            return (service, uow);
        }

        [Fact]
        public async Task SuperAdmin_Role_HasFullCapabilities_Automatically()
        {
            var (service, uow) = CreateTestSetup();
            uow.RolesRepo.Items.Add(new RoleModel { Id = 2, Name = "Super Admin", DeletedFlag = 1 });
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 1, PermissionKey = "users.view", DeletedFlag = 1 });
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 2, PermissionKey = "users.delete", DeletedFlag = 1 });

            var effective = await service.GetEffectivePermissionsForRoleAsync(2);

            Assert.Equal(2, effective.Count);
            Assert.All(effective, p =>
            {
                Assert.True(p.IsAllowed);
                Assert.Equal("Allow", p.Access);
                Assert.Equal("SuperAdmin", p.Source);
            });
        }

        [Fact]
        public async Task PermissionInheritance_EmployeeInheritsFromManager()
        {
            var (service, uow) = CreateTestSetup();

            // Manager (Id 4) -> Employee (Id 5, ParentRoleId = 4)
            uow.RolesRepo.Items.Add(new RoleModel { Id = 4, Name = "Manager", DeletedFlag = 1 });
            uow.RolesRepo.Items.Add(new RoleModel { Id = 5, Name = "Employee", ParentRoleId = 4, DeletedFlag = 1 });

            var viewPerm = new PermissionModel { Id = 1, PermissionKey = "projects.view", DeletedFlag = 1 };
            uow.PermsRepo.Items.Add(viewPerm);

            // Grant projects.view to Manager
            uow.RolePermsRepo.Items.Add(new RolePermissionModel
            {
                RoleId = 4,
                PermissionId = 1,
                Access = "Allow"
            });

            var employeeEffective = await service.GetEffectivePermissionsForRoleAsync(5);
            var perm = employeeEffective.FirstOrDefault(p => p.PermissionKey == "projects.view");

            Assert.NotNull(perm);
            Assert.True(perm.IsAllowed);
            Assert.Equal("InheritedAllow", perm.Source);
            Assert.Equal("Manager", perm.InheritedFromRole);
        }

        [Fact]
        public async Task DeterministicPrecedence_ExplicitChildDeny_Overrides_AncestorAllow()
        {
            var (service, uow) = CreateTestSetup();

            // Admin (Id 3) -> Manager (Id 4, ParentRoleId = 3)
            uow.RolesRepo.Items.Add(new RoleModel { Id = 3, Name = "Admin", DeletedFlag = 1 });
            uow.RolesRepo.Items.Add(new RoleModel { Id = 4, Name = "Manager", ParentRoleId = 3, DeletedFlag = 1 });

            var deletePerm = new PermissionModel { Id = 1, PermissionKey = "users.delete", DeletedFlag = 1 };
            uow.PermsRepo.Items.Add(deletePerm);

            // Admin allows users.delete
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 3, PermissionId = 1, Access = "Allow" });
            // Child (Manager) explicitly denies users.delete
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 4, PermissionId = 1, Access = "Deny" });

            var managerEffective = await service.GetEffectivePermissionsForRoleAsync(4);
            var perm = managerEffective.FirstOrDefault(p => p.PermissionKey == "users.delete");

            Assert.NotNull(perm);
            Assert.False(perm.IsAllowed);
            Assert.Equal("Deny", perm.Access);
            Assert.Equal("ExplicitChildDeny", perm.Source);
        }

        [Fact]
        public async Task DeterministicPrecedence_ExplicitChildAllow_Overrides_AncestorDeny()
        {
            var (service, uow) = CreateTestSetup();

            uow.RolesRepo.Items.Add(new RoleModel { Id = 3, Name = "Admin", DeletedFlag = 1 });
            uow.RolesRepo.Items.Add(new RoleModel { Id = 4, Name = "Manager", ParentRoleId = 3, DeletedFlag = 1 });

            var exportPerm = new PermissionModel { Id = 1, PermissionKey = "reports.export", DeletedFlag = 1 };
            uow.PermsRepo.Items.Add(exportPerm);

            // Admin denies reports.export
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 3, PermissionId = 1, Access = "Deny" });
            // Manager explicitly allows reports.export
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 4, PermissionId = 1, Access = "Allow" });

            var managerEffective = await service.GetEffectivePermissionsForRoleAsync(4);
            var perm = managerEffective.FirstOrDefault(p => p.PermissionKey == "reports.export");

            Assert.NotNull(perm);
            Assert.True(perm.IsAllowed);
            Assert.Equal("Allow", perm.Access);
            Assert.Equal("ExplicitChildAllow", perm.Source);
        }

        [Fact]
        public async Task DeterministicPrecedence_DefaultDeny_WhenUnassigned()
        {
            var (service, uow) = CreateTestSetup();
            uow.RolesRepo.Items.Add(new RoleModel { Id = 5, Name = "Employee", DeletedFlag = 1 });
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 1, PermissionKey = "settings.edit", DeletedFlag = 1 });

            var effective = await service.GetEffectivePermissionsForRoleAsync(5);
            var perm = effective.FirstOrDefault(p => p.PermissionKey == "settings.edit");

            Assert.NotNull(perm);
            Assert.False(perm.IsAllowed);
            Assert.Equal("Deny", perm.Access);
            Assert.Equal("DefaultDeny", perm.Source);
        }

        [Fact]
        public async Task ManagePermission_Grants_SubActions()
        {
            var (service, uow) = CreateTestSetup();

            uow.RolesRepo.Items.Add(new RoleModel { Id = 4, Name = "Manager", DeletedFlag = 1 });
            uow.UsersRepo.Items.Add(new UserModel { Id = 10, RoleId = 4, DeletedFlag = 1, Email = "mgr@test.com", Name = "Manager" });

            // Only users.manage is granted
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 1, PermissionKey = "users.manage", DeletedFlag = 1 });
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 4, PermissionId = 1, Access = "Allow" });

            // User should have users.view, users.edit, users.create
            Assert.True(await service.HasPermissionAsync(10, "users.view"));
            Assert.True(await service.HasPermissionAsync(10, "users.edit"));
            Assert.True(await service.HasPermissionAsync(10, "users.create"));
            // But should NOT have invoices.view
            Assert.False(await service.HasPermissionAsync(10, "invoices.view"));
        }

        [Fact]
        public async Task PrivilegeEscalation_ThrowsForbiddenException()
        {
            var (service, uow) = CreateTestSetup();

            uow.RolesRepo.Items.Add(new RoleModel { Id = 5, Name = "Employee", DeletedFlag = 1 });
            uow.UsersRepo.Items.Add(new UserModel { Id = 20, RoleId = 5, DeletedFlag = 1, Email = "emp@test.com", Name = "Employee" });

            // Employee only has users.view
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 1, PermissionKey = "users.view", DeletedFlag = 1 });
            uow.PermsRepo.Items.Add(new PermissionModel { Id = 2, PermissionKey = "users.delete", DeletedFlag = 1 });
            uow.RolePermsRepo.Items.Add(new RolePermissionModel { RoleId = 5, PermissionId = 1, Access = "Allow" });

            // Employee tries to grant users.delete (which they do not have)
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
            {
                await service.ValidatePermissionAssignmentAsync(20, new[] { "users.delete" });
            });
        }
    }
}
