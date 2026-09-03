using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();

        Task<RoleDto?> GetRoleByIdAsync(int id);

        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

        Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request);

        Task<bool> SoftDeleteRoleAsync(int id);

        Task<bool> RestoreRoleAsync(int id);
    }
}
