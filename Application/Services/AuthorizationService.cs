using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Centralized authorization implementation providing dynamic, database-driven RBAC evaluations.
    /// Removes all hardcoded role dependencies and enforces permissions from PostgreSQL.
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionHierarchyService _permissionHierarchyService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IUnitOfWork unitOfWork,
            IPermissionHierarchyService permissionHierarchyService,
            ILogger<AuthorizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _permissionHierarchyService = permissionHierarchyService;
            _logger = logger;
        }

        public async Task<bool> IsSuperAdminRoleAsync(int roleId)
        {
            if (roleId <= 0) return false;

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null || role.DeletedFlag != 1) return false;

            // Database-driven check: IsSuperAdmin flag or system Super Admin role name
            if (role.IsSuperAdmin || string.Equals(role.Name, "Super Admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Universal governance permission check in database
            var manageAllPerm = await _unitOfWork.Permissions.FirstOrDefaultAsync(p =>
                p.DeletedFlag == 1 &&
                (p.PermissionKey == "manage_all_permissions" || p.PermissionKey == "*"));

            if (manageAllPerm != null)
            {
                var hasUniversalRule = await _unitOfWork.Repository<RolePermissionModel>().AnyAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionId == manageAllPerm.Id &&
                    (rp.Access == null || rp.Access == "Allow"));

                if (hasUniversalRule) return true;
            }

            return false;
        }

        public async Task<bool> IsSuperAdminAsync(int userId)
        {
            if (userId <= 0) return false;

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return false;

            // Check assigned role dynamically
            if (user.RoleId.HasValue && await IsSuperAdminRoleAsync(user.RoleId.Value))
            {
                return true;
            }

            // Check user direct grant for manage_all_permissions
            var manageAllPerm = await _unitOfWork.Permissions.FirstOrDefaultAsync(p =>
                p.DeletedFlag == 1 &&
                (p.PermissionKey == "manage_all_permissions" || p.PermissionKey == "*"));

            if (manageAllPerm != null)
            {
                var hasDirectUniversal = await _unitOfWork.Repository<UserPermissionModel>().AnyAsync(up =>
                    up.UserId == userId && up.PermissionId == manageAllPerm.Id);

                if (hasDirectUniversal) return true;
            }

            return false;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            if (string.IsNullOrWhiteSpace(permission)) return true;

            // Unrestricted Super Admin bypass resolved dynamically from DB
            if (await IsSuperAdminAsync(userId))
            {
                return true;
            }

            return await _permissionHierarchyService.HasPermissionAsync(userId, permission);
        }

        public async Task<bool> HasPermissionAsync(int userId, int menuId, string action)
        {
            if (menuId <= 0) return false;

            var allMenus = await _unitOfWork.Menus.GetAllActiveMenusAsync();
            var menu = allMenus.FirstOrDefault(m => m.Id == menuId);
            if (menu == null || menu.DeletedFlag != 1) return false;

            var key = !string.IsNullOrWhiteSpace(menu.PermissionKey)
                ? menu.PermissionKey
                : menu.MenuKey;

            var dotIndex = key.IndexOf('.');
            var modulePrefix = dotIndex > 0 ? key[..dotIndex] : key;
            var targetPermissionKey = $"{modulePrefix.ToLowerInvariant()}.{action.Trim().ToLowerInvariant()}";

            return await HasPermissionAsync(userId, targetPermissionKey);
        }

        public async Task<bool> HasActionAccessAsync(int userId, string menu, string action)
        {
            if (await IsSuperAdminAsync(userId)) return true;
            return await _permissionHierarchyService.HasActionAccessAsync(userId, menu, action);
        }

        public async Task<bool> HasMenuAccessAsync(int userId, string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey)) return true;
            if (await IsSuperAdminAsync(userId)) return true;
            return await _permissionHierarchyService.HasMenuAccessAsync(userId, menuKey);
        }

        public async Task<List<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId)
        {
            return await _permissionHierarchyService.GetEffectivePermissionsForUserAsync(userId);
        }

        public async Task<List<string>> GetEffectivePermissionKeysAsync(int userId)
        {
            var effective = await GetEffectivePermissionsAsync(userId);
            var keys = effective
                .Where(p => p.IsAllowed)
                .Select(p => p.PermissionKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k)
                .ToList();

            if (await IsSuperAdminAsync(userId) && !keys.Contains("manage_all_permissions"))
            {
                keys.Add("manage_all_permissions");
            }

            return keys;
        }

        public async Task<List<MenuItemDto>> GetAuthorizedMenusAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return [];

            var allMenus = await _unitOfWork.Menus.GetAllActiveMenusAsync();

            // Super Admin dynamically receives all active menus
            if (await IsSuperAdminAsync(userId))
            {
                return allMenus.ToDtoList();
            }

            var allowedMenus = new List<MenuModel>();
            foreach (var menu in allMenus)
            {
                // Public / open menu items without restricted permission keys
                if (string.IsNullOrWhiteSpace(menu.PermissionKey))
                {
                    allowedMenus.Add(menu);
                    continue;
                }

                if (await HasPermissionAsync(userId, menu.PermissionKey))
                {
                    allowedMenus.Add(menu);
                }
            }

            return allowedMenus.ToDtoList();
        }
    }
}
