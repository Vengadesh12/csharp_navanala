using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
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
