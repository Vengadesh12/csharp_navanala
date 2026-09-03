using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Permission matrix configuration and role-capability assignments.
    /// </summary>
    [ApiController]
    [Route("api/permissions")]
    [Tags("Permissions")]
    [Produces("application/json")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IUserService _userService;

        public PermissionsController(IPermissionService permissionService, IUserService userService)
        {
            _permissionService = permissionService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieve the full RBAC permission matrix and role assignments.
        /// </summary>
        /// <response code="200">Permission matrix returned successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks permission management privileges.</response>
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

        /// <summary>
        /// Update granted permission keys for a specific role.
        /// </summary>
        /// <param name="roleId">The unique integer identifier of the target role.</param>
        /// <param name="request">Array of valid permission keys to assign to this role.</param>
        /// <response code="200">Role permissions updated successfully.</response>
        /// <response code="400">One or more supplied permission keys are invalid.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks permission management privileges.</response>
        /// <response code="404">Role not found.</response>
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

        /// <summary>
        /// Retrieve granted permission keys for a specific department.
        /// </summary>
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

        /// <summary>
        /// Update granted permission keys for a specific department.
        /// </summary>
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
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return false;
            }
            return await _userService.HasPermissionAsync(userId, "permissions.manage");
        }
    }
}