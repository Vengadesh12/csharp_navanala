using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC)
    // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
    // TOPIC: Permission conflict resolution
    // Controller exposing role hierarchy trees, effective permissions calculated through
    // deterministic inheritance and conflict resolution, and permission matrix administration.
    // ==============================================================================
    [ApiController]
    [Route("api/permissions")]
    [Tags("Permissions")]
    [Produces("application/json")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IPermissionHierarchyService _permissionHierarchyService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public PermissionsController(
            IPermissionService permissionService,
            IPermissionHierarchyService permissionHierarchyService,
            IUserService userService,
            ICurrentUserService currentUserService)
        {
            _permissionService = permissionService;
            _permissionHierarchyService = permissionHierarchyService;
            _userService = userService;
            _currentUserService = currentUserService;
        }

        // ==============================================================================
        // TOPIC: Implement Hierarchical Role-Based Access Control with Permission Inheritance
        // Exposes the complete role hierarchy tree structure with inherited capabilities.
        // ==============================================================================
        [HttpGet("hierarchy")]
        [ProducesResponseType(typeof(RoleHierarchyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRoleHierarchyTree()
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var tree = await _permissionHierarchyService.GetRoleHierarchyTreeAsync();
            return Ok(tree);
        }

        // ==============================================================================
        // TOPIC: Permission conflict resolution & Role-Based Access Control
        // Returns effective permissions for a role using the 5-tier conflict resolution precedence:
        // Explicit Child Deny > Explicit Child Allow > Inherited Deny > Inherited Allow > Default Deny
        // ==============================================================================
        [HttpGet("effective/roles/{roleId:int}")]
        [ProducesResponseType(typeof(List<EffectivePermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetEffectiveRolePermissions(int roleId)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var permissions = await _permissionHierarchyService.GetEffectivePermissionsForRoleAsync(roleId);
            return Ok(permissions);
        }

        // ==============================================================================
        // TOPIC: Permission conflict resolution & Role-Based Access Control (User-Level)
        // Combines inherited role permissions with user-specific direct grants/overrides.
        // ==============================================================================
        [HttpGet("effective/users/{userId:int}")]
        [ProducesResponseType(typeof(List<EffectivePermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetEffectiveUserPermissions(int userId)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var permissions = await _permissionHierarchyService.GetEffectivePermissionsForUserAsync(userId);
            return Ok(permissions);
        }

        // ==============================================================================
        // TOPIC: Role-Based Access Control (RBAC Permissions Matrix)
        // ==============================================================================
        [HttpGet]
        [ProducesResponseType(typeof(PermissionsMatrixResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPermissions()
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var matrix = await _permissionService.GetPermissionsMatrixAsync();
            return Ok(matrix);
        }

        [HttpPut("{roleId:int}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdatePermissionsRequest request)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            try
            {
                var success = await _permissionService.UpdateRolePermissionsAsync(roleId, request);
                if (!success)
                {
                    return NotFound(new ErrorResponse { Message = $"Role with ID {roleId} not found." });
                }

                return Ok(new MessageResponse { Success = true, Message = "Permissions updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpGet("departments/{departmentId:int}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDepartmentPermissions(int departmentId)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var keys = await _permissionService.GetDepartmentPermissionsAsync(departmentId);
            return Ok(keys);
        }

        [HttpPut("departments/{departmentId:int}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDepartmentPermissions(int departmentId, [FromBody] UpdatePermissionsRequest request)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            try
            {
                var success = await _permissionService.UpdateDepartmentPermissionsAsync(departmentId, request);
                if (!success)
                {
                    return NotFound(new ErrorResponse { Message = $"Department with ID {departmentId} not found." });
                }

                return Ok(new MessageResponse { Success = true, Message = "Department permissions updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        // ==============================================================================
        // TOPIC: User Permissions Management
        // Exposes user directory overview with direct and effective permission counts,
        // user permissions detail, assignment, and revocation.
        // ==============================================================================
        [HttpGet("users-overview")]
        [ProducesResponseType(typeof(List<UserPermissionOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersPermissionOverview()
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var overview = await _permissionService.GetUsersPermissionOverviewAsync();
            return Ok(overview);
        }

        [HttpGet("users/{userId:int}/details")]
        [ProducesResponseType(typeof(UserPermissionProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserPermissionsDetail(int userId)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            var detail = await _permissionService.GetUserPermissionsDetailAsync(userId);
            if (detail == null)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {userId} not found." });
            }

            return Ok(detail);
        }

        [HttpPost("users/{userId:int}/assign")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignUserPermission(int userId, [FromBody] AssignUserPermissionRequest request)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request?.PermissionKey))
            {
                return BadRequest(new ErrorResponse { Message = "Permission key is required." });
            }

            var currentUserId = _currentUserService.UserId ?? 0;
            var success = await _permissionService.AssignUserPermissionAsync(userId, request.PermissionKey, currentUserId);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"User or permission '{request.PermissionKey}' not found." });
            }

            return Ok(new MessageResponse { Success = true, Message = $"Permission '{request.PermissionKey}' assigned directly to user successfully." });
        }

        [HttpDelete("users/{userId:int}/revoke/{permissionKey}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeUserPermission(int userId, string permissionKey)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(permissionKey))
            {
                return BadRequest(new ErrorResponse { Message = "Permission key is required." });
            }

            var currentUserId = _currentUserService.UserId ?? 0;
            var success = await _permissionService.RevokeUserPermissionAsync(userId, permissionKey, currentUserId);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"User or permission '{permissionKey}' not found." });
            }

            return Ok(new MessageResponse { Success = true, Message = $"Permission '{permissionKey}' revoked successfully." });
        }

        [HttpPost("users/{userId:int}/revoke")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RevokeUserPermissionPost(int userId, [FromBody] AssignUserPermissionRequest request)
        {
            if (!await CanManagePermissions())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request?.PermissionKey))
            {
                return BadRequest(new ErrorResponse { Message = "Permission key is required." });
            }

            var currentUserId = _currentUserService.UserId ?? 0;
            var success = await _permissionService.RevokeUserPermissionAsync(userId, request.PermissionKey, currentUserId);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"User or permission '{request.PermissionKey}' not found." });
            }

            return Ok(new MessageResponse { Success = true, Message = $"Permission '{request.PermissionKey}' revoked successfully." });
        }

        private async Task<bool> CanManagePermissions()
        {
            var userId = _currentUserService.UserId ?? 0;
            if (userId <= 0)
            {
                return false;
            }
            return await _userService.HasPermissionAsync(userId, "permissions.manage");
        }
    }
}
