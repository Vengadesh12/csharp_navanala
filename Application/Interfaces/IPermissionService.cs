using MyBackend.Application.Common.DTO;

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

        Task<List<UserPermissionOverviewDto>> GetUsersPermissionOverviewAsync();

        Task<UserPermissionProfileDto?> GetUserPermissionsDetailAsync(int userId);

        Task<bool> AssignUserPermissionAsync(int userId, string permissionKey, int granterUserId);

        Task<bool> RevokeUserPermissionAsync(int userId, string permissionKey, int granterUserId);
    }
}
