using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Centralized authorization service for dynamic, database-driven RBAC governance.
    /// Evaluates effective permissions, role capabilities, and menu access without static role IDs.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks whether a user possesses a specific permission dynamically from database.
        /// Unrestricted system roles (e.g. Super Admin) or users with universal privileges bypass granular checks.
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, string permission);

        /// <summary>
        /// Checks whether a user has permission to perform a specific action on a menu by menu ID.
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, int menuId, string action);

        /// <summary>
        /// Checks action-level access for a given menu and action string (e.g., menu="users", action="create").
        /// </summary>
        Task<bool> HasActionAccessAsync(int userId, string menu, string action);

        /// <summary>
        /// Checks whether a user has visibility/access to a specific menu key.
        /// </summary>
        Task<bool> HasMenuAccessAsync(int userId, string menuKey);

        /// <summary>
        /// Retrieves the full list of effective permissions for a user including inheritance, department, and direct rules.
        /// </summary>
        Task<List<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId);

        /// <summary>
        /// Retrieves a distinct list of allowed permission key strings for the user session.
        /// </summary>
        Task<List<string>> GetEffectivePermissionKeysAsync(int userId);

        /// <summary>
        /// Dynamically checks whether a user holds a highest-level / Super Admin role from the database.
        /// </summary>
        Task<bool> IsSuperAdminAsync(int userId);

        /// <summary>
        /// Dynamically checks whether a role ID is configured as a Super Admin or system role in the database.
        /// </summary>
        Task<bool> IsSuperAdminRoleAsync(int roleId);

        /// <summary>
        /// Returns all menus that the user is authorized to access based on database permissions.
        /// </summary>
        Task<List<MenuItemDto>> GetAuthorizedMenusAsync(int userId);
    }
}
