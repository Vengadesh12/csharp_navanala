using MyBackend.Application.Contracts;
using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    /// <summary>
    /// Specialized repository contract for Permissions catalog and RolePermission assignments.
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Retrieves the comprehensive matrix of permissions mapped across all workspace roles.
        /// </summary>
        Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync();

        /// <summary>
        /// Retrieves all active permissions formatted as PermissionDto.
        /// </summary>
        Task<List<PermissionDto>> GetAllActivePermissionsAsync();

        /// <summary>
        /// Retrieves permission keys assigned to a specific role ID.
        /// </summary>
        Task<List<string>> GetPermissionKeysByRoleIdAsync(int roleId);

        /// <summary>
        /// Updates the full set of permission keys assigned to a role ID.
        /// </summary>
        Task<bool> UpdateRolePermissionsAsync(int roleId, IEnumerable<string> permissionKeys);

        /// <summary>
        /// Retrieves permission keys assigned directly to a department ID.
        /// </summary>
        Task<List<string>> GetPermissionKeysByDepartmentIdAsync(int departmentId);

        /// <summary>
        /// Updates the full set of permission keys assigned to a department ID.
        /// </summary>
        Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, IEnumerable<string> permissionKeys);
    }
}
