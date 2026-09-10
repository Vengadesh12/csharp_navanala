using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IPermissionHierarchyService
    {
        Task<List<EffectivePermissionDto>> GetEffectivePermissionsForRoleAsync(int roleId);
        Task<List<EffectivePermissionDto>> GetEffectivePermissionsForUserAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionKey);
        Task<bool> HasMenuAccessAsync(int userId, string menuKey);
        Task<bool> HasActionAccessAsync(int userId, string menu, string action);
        Task<RoleHierarchyDto> GetRoleHierarchyTreeAsync();
        Task ValidatePermissionAssignmentAsync(int granterUserId, IEnumerable<string> requestedPermissionKeys);
        Task ValidateChildRolePermissionsAsync(int childRoleId, IEnumerable<string> requestedPermissionKeys);
    }
}
