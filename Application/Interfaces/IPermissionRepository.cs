using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IPermissionRepository : IRepository<PermissionModel>
    {
        Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync();
        Task<List<PermissionDto>> GetAllActivePermissionsAsync();
        Task<List<string>> GetPermissionKeysByRoleIdAsync(int roleId);
        Task<bool> UpdateRolePermissionsAsync(int roleId, IEnumerable<string> permissionKeys);
        Task<List<string>> GetPermissionKeysByDepartmentIdAsync(int departmentId);
        Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, IEnumerable<string> permissionKeys);
    }
}
