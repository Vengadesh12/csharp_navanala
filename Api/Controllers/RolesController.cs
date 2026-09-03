using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Role management endpoints for creating, editing, viewing, and deleting workspace roles.
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Tags("Roles")]
    [Produces("application/json")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Retrieve all active workspace roles.
        /// </summary>
        /// <response code="200">List of roles retrieved successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Retrieve detailed information for a single role by ID.
        /// </summary>
        /// <param name="id">The unique integer identifier of the role.</param>
        /// <response code="200">Role retrieved successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="404">Role not found or is deactivated.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role is null)
            {
                return NotFound(new ErrorResponse { Message = $"Role with ID {id} not found." });
            }
            return Ok(role);
        }

        /// <summary>
        /// Create a new workspace role.
        /// </summary>
        /// <param name="request">Payload containing the name and description of the role.</param>
        /// <response code="201">Role created successfully; returns the created role entity.</response>
        /// <response code="400">Role name is required.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Role name is required." });
            }

            var createdRole = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role saved successfully!",
                Data = createdRole
            });
        }

        /// <summary>
        /// Update an existing role's title and description.
        /// </summary>
        /// <param name="id">The unique integer identifier of the role to update.</param>
        /// <param name="request">Updated role details.</param>
        /// <response code="200">Role updated successfully.</response>
        /// <response code="400">Invalid payload provided.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="404">Role not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Role name is required." });
            }

            var updatedRole = await _roleService.UpdateRoleAsync(id, request);
            if (updatedRole is null)
            {
                return NotFound(new ErrorResponse { Message = $"Role with ID {id} not found." });
            }

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role updated successfully!",
                Data = updatedRole
            });
        }

        /// <summary>
        /// Soft-delete (deactivate) a workspace role.
        /// </summary>
        /// <param name="id">The unique integer identifier of the role to deactivate.</param>
        /// <response code="200">Role deleted successfully.</response>
        /// <response code="400">Cannot delete Super Admin protected role.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="404">Role not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            if (id == 2)
            {
                return BadRequest(new ErrorResponse { Message = "Super Admin system role cannot be deleted." });
            }

            var success = await _roleService.SoftDeleteRoleAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Role with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Role deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        /// <summary>
        /// Restore a previously soft-deleted role.
        /// </summary>
        /// <param name="id">The unique integer identifier of the role to restore.</param>
        /// <response code="200">Role restored successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="404">Role not found.</response>
        [HttpPost("{id:int}/restore")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreRole(int id)
        {
            var success = await _roleService.RestoreRoleAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Role with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Role restored successfully!",
                Id = id,
                DeletedFlag = 1
            });
        }
    }
}
