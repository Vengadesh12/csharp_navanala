using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionHierarchyService _permissionHierarchyService;
        private readonly ICurrentUserService _currentUserService;

        public PermissionService(
            IUnitOfWork unitOfWork,
            IPermissionHierarchyService permissionHierarchyService,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _permissionHierarchyService = permissionHierarchyService;
            _currentUserService = currentUserService;
        }

        public async Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync()
        {
            return await _unitOfWork.Permissions.GetPermissionsMatrixAsync();
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _unitOfWork.Permissions.GetAllActivePermissionsAsync();
        }

        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            return await _unitOfWork.Permissions.GetPermissionKeysByRoleIdAsync(roleId);
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, UpdatePermissionsRequest request)
        {
            var keysToValidate = request.Rules != null && request.Rules.Count > 0
                ? request.Rules.Select(r => r.PermissionKey)
                : request.PermissionKeys;

            // Privilege escalation prevention:
            // Ensure caller cannot grant permissions exceeding their own capabilities
            if (_currentUserService.UserId.HasValue)
            {
                await _permissionHierarchyService.ValidatePermissionAssignmentAsync(
                    _currentUserService.UserId.Value, keysToValidate);
            }

            // Boundary check: ensure child role permissions do not violate parent boundaries
            await _permissionHierarchyService.ValidateChildRolePermissionsAsync(roleId, keysToValidate);

            if (request.Rules != null && request.Rules.Count > 0)
            {
                return await _unitOfWork.Permissions.UpdateRolePermissionsWithRulesAsync(roleId, request.Rules);
            }

            return await _unitOfWork.Permissions.UpdateRolePermissionsAsync(roleId, request.PermissionKeys);
        }

        public async Task<List<string>> GetDepartmentPermissionsAsync(int departmentId)
        {
            return await _unitOfWork.Permissions.GetPermissionKeysByDepartmentIdAsync(departmentId);
        }

        public async Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, UpdatePermissionsRequest request)
        {
            return await _unitOfWork.Permissions.UpdateDepartmentPermissionsAsync(departmentId, request.PermissionKeys);
        }

        public async Task<List<UserPermissionOverviewDto>> GetUsersPermissionOverviewAsync()
        {
            var users = await _unitOfWork.Users.ListAllAsync();
            var activeUsers = users.Where(u => u.DeletedFlag == 1).ToList();

            var roles = await _unitOfWork.Roles.ListAllAsync();
            var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);

            var departments = await _unitOfWork.Departments.ListAllAsync();
            var deptMap = departments.ToDictionary(d => d.Id, d => d.Name);

            var designations = await _unitOfWork.Designations.ListAllAsync();
            var desMap = designations.ToDictionary(d => d.Id, d => d);

            var allUserPerms = await _unitOfWork.Repository<MyBackend.Domain.Models.UserPermissionModel>().ListAllAsync();
            var directPermCountByUser = allUserPerms
                .GroupBy(up => up.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<UserPermissionOverviewDto>();

            foreach (var u in activeUsers)
            {
                var roleName = u.RoleId.HasValue && roleMap.TryGetValue(u.RoleId.Value, out var rName) ? rName : "Unassigned";
                int? deptId = null;
                string deptName = "";
                string desName = "";

                if (u.DesignationId.HasValue && desMap.TryGetValue(u.DesignationId.Value, out var des))
                {
                    desName = des.Name;
                    if (des.DepartmentId.HasValue && deptMap.TryGetValue(des.DepartmentId.Value, out var dn))
                    {
                        deptId = des.DepartmentId.Value;
                        deptName = dn;
                    }
                }

                var directCount = directPermCountByUser.TryGetValue(u.Id, out var dc) ? dc : 0;
                var effective = await _permissionHierarchyService.GetEffectivePermissionsForUserAsync(u.Id);
                var effectiveCount = effective.Count(p => p.IsAllowed);
                var roleCount = effective.Count(p => p.IsAllowed && (p.Source == "Role" || p.Source == "RoleAndDepartment" || p.Source == "ExplicitChildAllow" || p.Source == "InheritedAllow"));
                var deptCount = effective.Count(p => p.IsAllowed && (p.Source == "Department" || p.Source == "RoleAndDepartment"));

                result.Add(new UserPermissionOverviewDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = roleName,
                    DepartmentId = deptId,
                    DepartmentName = deptName,
                    DesignationId = u.DesignationId,
                    DesignationName = desName,
                    DirectPermissionsCount = directCount,
                    RolePermissionsCount = roleCount,
                    DepartmentPermissionsCount = deptCount,
                    TotalEffectivePermissionsCount = effectiveCount
                });
            }

            return result.OrderBy(u => u.Name).ToList();
        }

        public async Task<UserPermissionProfileDto?> GetUserPermissionsDetailAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return null;

            var roles = await _unitOfWork.Roles.ListAllAsync();
            var role = user.RoleId.HasValue ? roles.FirstOrDefault(r => r.Id == user.RoleId.Value) : null;
            var roleName = role?.Name ?? "Unassigned";

            var designations = await _unitOfWork.Designations.ListAllAsync();
            var designation = user.DesignationId.HasValue ? designations.FirstOrDefault(d => d.Id == user.DesignationId.Value) : null;
            var desName = designation?.Name ?? "";

            var departments = await _unitOfWork.Departments.ListAllAsync();
            var dept = designation?.DepartmentId.HasValue == true ? departments.FirstOrDefault(d => d.Id == designation.DepartmentId.Value) : null;
            var deptName = dept?.Name ?? "";
            var deptId = dept?.Id;

            // Department permission keys
            var deptPermKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (deptId.HasValue)
            {
                var keys = await _unitOfWork.Permissions.GetPermissionKeysByDepartmentIdAsync(deptId.Value);
                foreach (var k in keys) deptPermKeys.Add(k);
            }

            var allPermissions = await _unitOfWork.Permissions.GetAllActivePermissionsAsync();
            var effectivePerms = await _permissionHierarchyService.GetEffectivePermissionsForUserAsync(userId);
            var effectiveMap = effectivePerms.ToDictionary(ep => ep.PermissionKey.ToLower(), ep => ep);

            var directPerms = await _unitOfWork.Repository<MyBackend.Domain.Models.UserPermissionModel>().FindAsync(up => up.UserId == userId);
            var directPermMap = directPerms.ToDictionary(dp => dp.PermissionId, dp => dp);

            var allPermissionsEntities = await _unitOfWork.Permissions.ListAllAsync();
            var permIdMap = allPermissionsEntities.ToDictionary(p => p.PermissionKey.ToLower(), p => p.Id);

            var detailList = new List<UserPermissionDetailDto>();

            foreach (var perm in allPermissions)
            {
                var permKeyLower = perm.PermissionKey.ToLower();
                var permId = permIdMap.TryGetValue(permKeyLower, out var pId) ? pId : 0;

                effectiveMap.TryGetValue(permKeyLower, out var effective);
                var isDirect = directPermMap.TryGetValue(permId, out var directRecord);

                var isAllowed = effective?.IsAllowed ?? false;
                var source = effective?.Source ?? (isAllowed ? "Inherited" : "DefaultDeny");
                if (isDirect)
                {
                    source = "UserDirectGrant";
                }

                var isDept = deptPermKeys.Contains(perm.PermissionKey);
                var isRole = source == "Role" || source == "RoleAndDepartment" || source == "ExplicitChildAllow" || source == "InheritedAllow";

                var category = perm.PermissionKey.Contains('.')
                    ? perm.PermissionKey.Split('.')[0]
                    : "general";

                detailList.Add(new UserPermissionDetailDto
                {
                    PermissionId = permId,
                    PermissionKey = perm.PermissionKey,
                    Name = perm.Name,
                    Description = perm.Description,
                    Category = category,
                    Access = isAllowed ? "Allow" : "Deny",
                    IsAllowed = isAllowed,
                    IsDirect = isDirect,
                    IsFromRole = isRole,
                    IsFromDepartment = isDept,
                    Source = source,
                    DepartmentName = isDept ? deptName : null,
                    RoleName = isRole ? roleName : null,
                    UserPermissionId = directRecord?.Id,
                    GrantedAt = directRecord?.CreatedAt
                });
            }

            var directCount = detailList.Count(p => p.IsDirect);
            var roleCount = detailList.Count(p => p.IsFromRole && !p.IsDirect);
            var deptCount = detailList.Count(p => p.IsFromDepartment && !p.IsDirect);
            var totalCount = detailList.Count(p => p.IsAllowed);

            return new UserPermissionProfileDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = roleName,
                DepartmentId = deptId,
                DepartmentName = deptName,
                DesignationId = user.DesignationId,
                DesignationName = desName,
                DirectCount = directCount,
                RoleCount = roleCount,
                DepartmentCount = deptCount,
                TotalCount = totalCount,
                Permissions = detailList.OrderByDescending(p => p.IsDirect)
                                        .ThenByDescending(p => p.IsFromDepartment)
                                        .ThenByDescending(p => p.IsFromRole)
                                        .ThenBy(p => p.Category)
                                        .ThenBy(p => p.Name)
                                        .ToList()
            };
        }

        public async Task<bool> AssignUserPermissionAsync(int userId, string permissionKey, int granterUserId)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
            {
                return false;
            }

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return false;

            var allPerms = await _unitOfWork.Permissions.ListAllAsync();
            var perm = allPerms.FirstOrDefault(p => p.PermissionKey.Equals(permissionKey.Trim(), StringComparison.OrdinalIgnoreCase) && p.DeletedFlag == 1);
            if (perm == null) return false;

            var userPermRepo = _unitOfWork.Repository<MyBackend.Domain.Models.UserPermissionModel>();
            var existing = await userPermRepo.FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == perm.Id);
            if (existing == null)
            {
                await userPermRepo.AddAsync(new MyBackend.Domain.Models.UserPermissionModel
                {
                    UserId = userId,
                    PermissionId = perm.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            var granter = await _unitOfWork.Users.GetUserByIdAsync(granterUserId);
            var granterName = granter?.Name ?? "Administrator";

            await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                "Direct Permission Assigned",
                "User Permissions",
                granterName,
                $"Assigned permission '{perm.PermissionKey}' directly to {user.Name} ({user.Email})",
                "127.0.0.1",
                "Success"
            );

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeUserPermissionAsync(int userId, string permissionKey, int granterUserId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return false;

            var allPerms = await _unitOfWork.Permissions.ListAllAsync();
            var perm = allPerms.FirstOrDefault(p => p.PermissionKey.Equals(permissionKey.Trim(), StringComparison.OrdinalIgnoreCase) && p.DeletedFlag == 1);
            if (perm == null) return false;

            var userPermRepo = _unitOfWork.Repository<MyBackend.Domain.Models.UserPermissionModel>();
            var directPerms = await userPermRepo.FindAsync(up => up.UserId == userId && up.PermissionId == perm.Id);
            if (directPerms.Count > 0)
            {
                userPermRepo.DeleteRange(directPerms);
            }

            var granter = await _unitOfWork.Users.GetUserByIdAsync(granterUserId);
            var granterName = granter?.Name ?? "Administrator";

            await _unitOfWork.AuditLogs.CreateAuditLogAsync(
                "Direct Permission Revoked",
                "User Permissions",
                granterName,
                $"Revoked direct permission '{perm.PermissionKey}' from {user.Name} ({user.Email})",
                "127.0.0.1",
                "Success"
            );

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
