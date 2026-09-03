using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for permissions matrix retrieval and role capability assignments.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Retrieves the comprehensive matrix of permissions mapped across all workspace roles.
        /// </summary>
        Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync();

        /// <summary>
        /// Retrieves all registered system permissions formatted as PermissionDto.
        /// </summary>
        Task<List<PermissionDto>> GetAllPermissionsAsync();

        /// <summary>
        /// Retrieves all permission keys assigned to a specific role.
        /// </summary>
        Task<List<string>> GetRolePermissionsAsync(int roleId);

        /// <summary>
        /// Updates the set of permission keys assigned to a specific role.
        /// </summary>
        Task<bool> UpdateRolePermissionsAsync(int roleId, UpdatePermissionsRequest request);

        /// <summary>
        /// Retrieves all permission keys assigned directly to a specific department.
        /// </summary>
        Task<List<string>> GetDepartmentPermissionsAsync(int departmentId);

        /// <summary>
        /// Updates the set of permission keys assigned to a specific department.
        /// </summary>
        Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, UpdatePermissionsRequest request);
    }
}
