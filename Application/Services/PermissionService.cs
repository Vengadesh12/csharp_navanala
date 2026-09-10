using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionHierarchyService _permissionHierarchyService;
        private readonly ICurrentUserService _currentUserService;

        public PermissionService(
            IUnitOfWork unitOfWork,
            IPermissionHierarchyService permissionHierarchyService,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _permissionHierarchyService = permissionHierarchyService;
            _currentUserService = currentUserService;
        }

        public async Task<PermissionsMatrixResponse> GetPermissionsMatrixAsync()
        {
            return await _unitOfWork.Permissions.GetPermissionsMatrixAsync();
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _unitOfWork.Permissions.GetAllActivePermissionsAsync();
        }

        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            return await _unitOfWork.Permissions.GetPermissionKeysByRoleIdAsync(roleId);
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, UpdatePermissionsRequest request)
        {
            var keysToValidate = request.Rules != null && request.Rules.Count > 0
                ? request.Rules.Select(r => r.PermissionKey)
                : request.PermissionKeys;

            // Privilege escalation prevention:
            // Ensure caller cannot grant permissions exceeding their own capabilities
            if (_currentUserService.UserId.HasValue)
            {
                await _permissionHierarchyService.ValidatePermissionAssignmentAsync(
                    _currentUserService.UserId.Value, keysToValidate);
            }

            // Boundary check: ensure child role permissions do not violate parent boundaries
            await _permissionHierarchyService.ValidateChildRolePermissionsAsync(roleId, keysToValidate);

            if (request.Rules != null && request.Rules.Count > 0)
            {
                return await _unitOfWork.Permissions.UpdateRolePermissionsWithRulesAsync(roleId, request.Rules);
            }

            return await _unitOfWork.Permissions.UpdateRolePermissionsAsync(roleId, request.PermissionKeys);
        }

        public async Task<List<string>> GetDepartmentPermissionsAsync(int departmentId)
        {
            return await _unitOfWork.Permissions.GetPermissionKeysByDepartmentIdAsync(departmentId);
        }

        public async Task<bool> UpdateDepartmentPermissionsAsync(int departmentId, UpdatePermissionsRequest request)
        {
            return await _unitOfWork.Permissions.UpdateDepartmentPermissionsAsync(departmentId, request.PermissionKeys);
        }
    }
}
