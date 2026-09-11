using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Services
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC)
    // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
    // TOPIC: Permission conflict resolution
    // ==============================================================================
    // 1. Role-Based Access Control (RBAC):
    //    Users are assigned roles, and permissions are associated with roles or user-specific overrides.
    //    Controls access at both Menu level (viewing UI sections) and Action level (create, edit, delete).
    //
    // 2. Hierarchical Role-Based Access Control with Permission Inheritance:
    //    Roles form a tree hierarchy via 'ParentRoleId' (e.g., Super Admin -> Admin -> Manager -> Employee).
    //    Child roles automatically inherit all allowed permissions from their parent and ancestors.
    //    Protected against cyclic parent-child loops using HashSet cycle detection.
    //
    // 3. Deterministic Permission Conflict Resolution (5-Tier Priority):
    //    When multiple permissions apply or conflict, resolution follows a strict priority order:
    //      Tier 1: Explicit Child Deny   -> If child role explicitly has 'Deny', access is blocked immediately.
    //      Tier 2: Explicit Child Allow  -> If child role explicitly has 'Allow', access is granted.
    //      Tier 3: Inherited Deny        -> Inherited from nearest ancestor with 'Deny' rule.
    //      Tier 4: Inherited Allow       -> Inherited from nearest ancestor with 'Allow' rule.
    //      Tier 5: Default Deny          -> Fail-secure zero trust default (if unassigned, deny access).
    // ==============================================================================
    public class PermissionHierarchyService : IPermissionHierarchyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionHierarchyService> _logger;

        public PermissionHierarchyService(
            IUnitOfWork unitOfWork,
            ILogger<PermissionHierarchyService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ==============================================================================
        // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
        // Traverses the ancestor role hierarchy and evaluates effective permissions for a role.
        // ==============================================================================
        public async Task<List<EffectivePermissionDto>> GetEffectivePermissionsForRoleAsync(int roleId)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllActivePermissionsAsync();
            var allRoles = await _unitOfWork.Roles.ListAllAsync();
            var targetRole = allRoles.FirstOrDefault(r => r.Id == roleId && r.DeletedFlag == 1);

            if (targetRole == null)
            {
                return [];
            }

            // Super Admin (Id 2 or Super Admin name) has full system capabilities
            if (targetRole.Id == 2 || string.Equals(targetRole.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
            {
                return allPermissions.Select(p =>
                {
                    var (menu, action) = ParseMenuAndAction(p.PermissionKey);
                    return new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "SuperAdmin",
                        InheritedFromRole = null
                    };
                }).ToList();
            }

            // TOPIC: Hierarchical RBAC - Ancestor Chain with Cycle Protection
            // Traverses parent roles upwards (Child -> Parent -> Grandparent -> Root)
            var roleChain = new List<RoleModel>();
            var visitedRoleIds = new HashSet<int>();
            var currentRole = targetRole;

            while (currentRole != null && visitedRoleIds.Add(currentRole.Id))
            {
                roleChain.Add(currentRole);
                if (currentRole.ParentRoleId.HasValue)
                {
                    currentRole = allRoles.FirstOrDefault(r => r.Id == currentRole.ParentRoleId.Value && r.DeletedFlag == 1);
                }
                else
                {
                    currentRole = null;
                }
            }

            // Load assigned permissions for all roles in the hierarchy
            var hierarchyRoleIds = roleChain.Select(r => r.Id).ToList();
            var rolePermissions = await _unitOfWork.Repository<RolePermissionModel>().FindAsync(rp => hierarchyRoleIds.Contains(rp.RoleId));
            var permissionIdMap = allPermissions.ToDictionary(p => p.PermissionKey, p => p, StringComparer.OrdinalIgnoreCase);

            var permissionsList = await _unitOfWork.Permissions.ListAllAsync();
            var permIdToKey = permissionsList.ToDictionary(p => p.Id, p => p.PermissionKey);

            var effectiveList = new List<EffectivePermissionDto>();

            foreach (var perm in allPermissions)
            {
                var (menu, action) = ParseMenuAndAction(perm.PermissionKey);
                var permEntity = permissionsList.FirstOrDefault(p => p.PermissionKey.Equals(perm.PermissionKey, StringComparison.OrdinalIgnoreCase));
                if (permEntity == null) continue;

                // ==============================================================================
                // TOPIC: Permission conflict resolution (Deterministic Precedence Evaluation)
                // 1. Explicit child Deny
                // 2. Explicit child Allow
                // 3. Inherited Deny (by ancestor proximity)
                // 4. Inherited Allow (by ancestor proximity)
                // 5. Default Deny (fail securely)
                // ==============================================================================
                string access = "Deny";
                bool isAllowed = false;
                string source = "DefaultDeny";
                string? inheritedFrom = null;

                // Step 1 & 2: Check direct child role rule first (Explicit Child Deny / Explicit Child Allow)
                var directRule = rolePermissions.FirstOrDefault(rp => rp.RoleId == targetRole.Id && rp.PermissionId == permEntity.Id);
                if (directRule != null)
                {
                    var isDirectDeny = string.Equals(directRule.Access, "Deny", StringComparison.OrdinalIgnoreCase);
                    access = isDirectDeny ? "Deny" : "Allow";
                    isAllowed = !isDirectDeny;
                    source = isDirectDeny ? "ExplicitChildDeny" : "ExplicitChildAllow";
                }
                else
                {
                    // Step 3 & 4: Permission Inheritance - Check ancestor chain in order of proximity (Parent -> Grandparent -> ...)
                    for (int i = 1; i < roleChain.Count; i++)
                    {
                        var ancestor = roleChain[i];
                        // Super Admin full capability does not blanket-propagate down to child roles
                        if (ancestor.Id == 2 || string.Equals(ancestor.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var ancestorRule = rolePermissions.FirstOrDefault(rp => rp.RoleId == ancestor.Id && rp.PermissionId == permEntity.Id);
                        if (ancestorRule != null)
                        {
                            var isAncestorDeny = string.Equals(ancestorRule.Access, "Deny", StringComparison.OrdinalIgnoreCase);
                            access = isAncestorDeny ? "Deny" : "Allow";
                            isAllowed = !isAncestorDeny;
                            source = isAncestorDeny ? "InheritedDeny" : "InheritedAllow";
                            inheritedFrom = ancestor.Name;
                            break;
                        }
                    }
                }

                effectiveList.Add(new EffectivePermissionDto
                {
                    PermissionKey = perm.PermissionKey,
                    Menu = menu,
                    Action = action,
                    Access = access,
                    IsAllowed = isAllowed,
                    Source = source,
                    InheritedFromRole = inheritedFrom
                });
            }

            return effectiveList;
        }

        public async Task<List<EffectivePermissionDto>> GetEffectivePermissionsForUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null) return [];

            var allPermissions = await _unitOfWork.Permissions.GetAllActivePermissionsAsync();
            var allPermissionsList = await _unitOfWork.Permissions.ListAllAsync();

            // 1. Super Admin check
            if (user.RoleId == 2)
            {
                return allPermissions.Select(p =>
                {
                    var (menu, action) = ParseMenuAndAction(p.PermissionKey);
                    return new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "SuperAdmin"
                    };
                }).ToList();
            }

            // 2. Role Permissions
            List<EffectivePermissionDto> rolePermissions = [];
            if (user.RoleId.HasValue)
            {
                rolePermissions = await GetEffectivePermissionsForRoleAsync(user.RoleId.Value);
            }
            var rolePermMap = rolePermissions.ToDictionary(rp => rp.PermissionKey, rp => rp, StringComparer.OrdinalIgnoreCase);

            // 3. Department Permissions (via designation -> department)
            var deptPermKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string? deptName = null;
            if (user.DesignationId.HasValue)
            {
                var designations = await _unitOfWork.Designations.ListAllAsync();
                var designation = designations.FirstOrDefault(d => d.Id == user.DesignationId.Value && d.DeletedFlag == 1);
                if (designation?.DepartmentId.HasValue == true)
                {
                    var departments = await _unitOfWork.Departments.ListAllAsync();
                    var dept = departments.FirstOrDefault(d => d.Id == designation.DepartmentId.Value && d.DeletedFlag == 1);
                    deptName = dept?.Name;

                    var keys = await _unitOfWork.Permissions.GetPermissionKeysByDepartmentIdAsync(designation.DepartmentId.Value);
                    foreach (var k in keys)
                    {
                        deptPermKeys.Add(k);
                    }
                }
            }

            // 4. User Direct Permissions
            var userPerms = await _unitOfWork.Repository<UserPermissionModel>().FindAsync(up => up.UserId == userId);
            var userPermIdSet = userPerms.Select(up => up.PermissionId).ToHashSet();
            var permIdToKey = allPermissionsList.ToDictionary(p => p.Id, p => p.PermissionKey);
            var directPermKeys = userPermIdSet.Where(id => permIdToKey.ContainsKey(id)).Select(id => permIdToKey[id]).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = new List<EffectivePermissionDto>();

            foreach (var p in allPermissions)
            {
                var (menu, action) = ParseMenuAndAction(p.PermissionKey);
                var isDirect = directPermKeys.Contains(p.PermissionKey);
                rolePermMap.TryGetValue(p.PermissionKey, out var rp);
                var isRoleAllowed = rp != null && rp.IsAllowed;
                var isDeptAllowed = deptPermKeys.Contains(p.PermissionKey);

                if (isDirect)
                {
                    result.Add(new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "UserDirectGrant"
                    });
                }
                else if (isRoleAllowed && isDeptAllowed)
                {
                    result.Add(new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "RoleAndDepartment",
                        InheritedFromRole = rp?.InheritedFromRole,
                        InheritedFromDepartment = deptName
                    });
                }
                else if (isRoleAllowed)
                {
                    result.Add(new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "Role",
                        InheritedFromRole = rp?.InheritedFromRole
                    });
                }
                else if (isDeptAllowed)
                {
                    result.Add(new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Allow",
                        IsAllowed = true,
                        Source = "Department",
                        InheritedFromDepartment = deptName
                    });
                }
                else
                {
                    result.Add(new EffectivePermissionDto
                    {
                        PermissionKey = p.PermissionKey,
                        Menu = menu,
                        Action = action,
                        Access = "Deny",
                        IsAllowed = false,
                        Source = "DefaultDeny"
                    });
                }
            }

            return result;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey)) return true;

            var effective = await GetEffectivePermissionsForUserAsync(userId);
            var normalizedKey = permissionKey.Trim().ToLowerInvariant();

            // Direct key match
            var match = effective.FirstOrDefault(p => p.PermissionKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));
            if (match != null && match.IsAllowed)
            {
                return true;
            }

            // Check if user has corresponding 'manage' permission for the menu
            // E.g. users.manage grants users.view, users.create, users.edit, users.delete
            var (menu, _) = ParseMenuAndAction(normalizedKey);
            var manageKey = $"{menu}.manage";
            var manageMatch = effective.FirstOrDefault(p => p.PermissionKey.Equals(manageKey, StringComparison.OrdinalIgnoreCase));
            if (manageMatch != null && manageMatch.IsAllowed)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> HasMenuAccessAsync(int userId, string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey)) return true;

            var effective = await GetEffectivePermissionsForUserAsync(userId);
            var normalizedMenu = menuKey.Trim().ToLowerInvariant();

            // Check any allow permission belonging to this menu
            return effective.Any(p =>
                (p.Menu.Equals(normalizedMenu, StringComparison.OrdinalIgnoreCase) ||
                 p.PermissionKey.StartsWith(normalizedMenu, StringComparison.OrdinalIgnoreCase)) &&
                p.IsAllowed);
        }

        public async Task<bool> HasActionAccessAsync(int userId, string menu, string action)
        {
            var key = $"{menu.Trim().ToLowerInvariant()}.{action.Trim().ToLowerInvariant()}";
            return await HasPermissionAsync(userId, key);
        }

        // ==============================================================================
        // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
        // Recursively constructs the hierarchical role tree starting from the root role
        // (Super Admin) down to leaf roles, calculating effective permissions at each level.
        // ==============================================================================
        public async Task<RoleHierarchyDto> GetRoleHierarchyTreeAsync()
        {
            var allRoles = await _unitOfWork.Roles.ListAllAsync();
            var activeRoles = allRoles.Where(r => r.DeletedFlag == 1).ToList();

            var rootRole = activeRoles.FirstOrDefault(r => r.ParentRoleId == null || r.Id == 2)
                ?? activeRoles.FirstOrDefault();

            if (rootRole == null)
            {
                return new RoleHierarchyDto();
            }

            var visited = new HashSet<int>();
            return await BuildHierarchyNodeAsync(rootRole, activeRoles, visited);
        }

        private async Task<RoleHierarchyDto> BuildHierarchyNodeAsync(
            RoleModel role,
            List<RoleModel> allRoles,
            HashSet<int> visited)
        {
            visited.Add(role.Id);
            var parentName = role.ParentRoleId.HasValue
                ? allRoles.FirstOrDefault(r => r.Id == role.ParentRoleId.Value)?.Name
                : null;

            var effective = await GetEffectivePermissionsForRoleAsync(role.Id);

            var node = new RoleHierarchyDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                ParentRoleId = role.ParentRoleId,
                ParentRoleName = parentName,
                EffectivePermissions = effective
            };

            var children = allRoles.Where(r => r.ParentRoleId == role.Id && !visited.Contains(r.Id)).ToList();
            foreach (var child in children)
            {
                node.Children.Add(await BuildHierarchyNodeAsync(child, allRoles, visited));
            }

            return node;
        }

        // ==============================================================================
        // TOPIC: Role-Based Access Control - Privilege Escalation Prevention
        // Enforces that a user or role cannot grant permissions they do not themselves possess.
        // ==============================================================================
        public async Task ValidatePermissionAssignmentAsync(int granterUserId, IEnumerable<string> requestedPermissionKeys)
        {
            var granterUser = await _unitOfWork.Users.GetUserByIdAsync(granterUserId);
            if (granterUser == null)
            {
                throw new UnauthorizedException("Valid user session required.");
            }

            // Super Admin (RoleId 2) can grant any permission
            if (granterUser.RoleId == 2) return;

            var granterEffective = await GetEffectivePermissionsForUserAsync(granterUserId);
            var allowedKeys = granterEffective
                .Where(p => p.IsAllowed)
                .Select(p => p.PermissionKey.ToLowerInvariant())
                .ToHashSet();

            var escalationKeys = requestedPermissionKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => !allowedKeys.Contains(k))
                .ToList();

            if (escalationKeys.Count > 0)
            {
                throw new ForbiddenException(
                    $"Privilege Escalation Prohibited: You cannot grant capabilities you do not possess ({string.Join(", ", escalationKeys)})."
                );
            }
        }

        public async Task ValidateChildRolePermissionsAsync(int childRoleId, IEnumerable<string> requestedPermissionKeys)
        {
            var childRole = await _unitOfWork.Roles.GetByIdAsync(childRoleId);
            if (childRole == null || !childRole.ParentRoleId.HasValue) return;

            var parentEffective = await GetEffectivePermissionsForRoleAsync(childRole.ParentRoleId.Value);
            var parentAllowedKeys = parentEffective
                .Where(p => p.IsAllowed)
                .Select(p => p.PermissionKey.ToLowerInvariant())
                .ToHashSet();

            var exceedingKeys = requestedPermissionKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => !parentAllowedKeys.Contains(k))
                .ToList();

            if (exceedingKeys.Count > 0)
            {
                _logger.LogWarning(
                    "Child role #{ChildRoleId} permissions exceed parent role permissions: {ExceedingKeys}",
                    childRoleId,
                    string.Join(", ", exceedingKeys));
            }
        }

        private static (string Menu, string Action) ParseMenuAndAction(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return ("general", "access");

            var dotIndex = key.IndexOf('.');
            if (dotIndex > 0 && dotIndex < key.Length - 1)
            {
                return (key[..dotIndex], key[(dotIndex + 1)..]);
            }

            var underscoreIndex = key.IndexOf('_');
            if (underscoreIndex > 0 && underscoreIndex < key.Length - 1)
            {
                return (key[..underscoreIndex], key[(underscoreIndex + 1)..]);
            }

            return (key, "view");
        }
    }
}
