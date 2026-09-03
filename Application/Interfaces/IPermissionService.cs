using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync();

        Task<List<PermissionDto>> GetAllPermissionsAsync();

        Task<List<string>> GetRolePermissionsAsync(int roleId);

        Task<bool> UpdateRolePermissionsAsync(int roleId, UpdatePermissionsRequest request);

        Task<List<string>> GetDepartmentPermissionsAsync(int departmentId);

        Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, UpdatePermissionsRequest request);
    }
}
