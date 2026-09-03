using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for role provisioning, retrieval, updates, and soft deletion.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Retrieves all workspace roles formatted as RoleDto.
        /// </summary>
        Task<List<RoleDto>> GetAllRolesAsync();

        /// <summary>
        /// Retrieves a single role by ID.
        /// </summary>
        Task<RoleDto?> GetRoleByIdAsync(int id);

        /// <summary>
        /// Creates and persists a new workspace role.
        /// </summary>
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

        /// <summary>
        /// Updates an existing role's name and description.
        /// </summary>
        Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request);

        /// <summary>
        /// Soft-deletes a role.
        /// </summary>
        Task<bool> SoftDeleteRoleAsync(int id);

        /// <summary>
        /// Restores a soft-deleted role.
        /// </summary>
        Task<bool> RestoreRoleAsync(int id);
    }
}
