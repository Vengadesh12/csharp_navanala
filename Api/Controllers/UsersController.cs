using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// User account provisioning, profile retrieval, updates, soft deletion, and restoration.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    [Produces("application/json")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieve all users in the system formatted as UserDto.
        /// </summary>
        /// <response code="200">List of user accounts retrieved successfully.</response>
        /// <response code="401">Unauthorized: Missing or invalid authentication token.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieve detailed profile information for a single user by ID.
        /// </summary>
        /// <param name="id">The unique integer identifier of the user.</param>
        /// <response code="200">User profile found and returned.</response>
        /// <response code="404">No user exists with the specified ID.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user is null)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {id} not found." });
            }
            return Ok(user);
        }

        /// <summary>
        /// Provision and register a new user account and dispatch welcome credentials email via Gmail.
        /// </summary>
        /// <param name="request">New user registration payload.</param>
        /// <response code="201">User created successfully; returns UserDto.</response>
        /// <response code="400">Missing required fields.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks user creation permissions.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!await HasPermission("users.create", "users.manage"))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse { Message = "Password is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new ErrorResponse { Message = "Email is required." });
            }

            var createdUser = await _userService.CreateUserAsync(request);
            return CreatedAtAction(
                nameof(GetUser),
                new { id = createdUser.Id },
                new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User saved successfully and credentials email dispatched!",
                    Data = createdUser
                });
        }

        /// <summary>
        /// Update an existing user's profile and credentials.
        /// </summary>
        /// <param name="id">The unique integer identifier of the user to update.</param>
        /// <param name="request">Updated user payload.</param>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid payload provided.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks edit permissions.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            if (!await HasPermission("users.edit", "users.manage"))
            {
                return Forbid();
            }

            var updatedUser = await _userService.UpdateUserAsync(id, request);
            if (updatedUser is null)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {id} not found." });
            }

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User updated successfully!",
                Data = updatedUser
            });
        }

        /// <summary>
        /// Soft-delete (deactivate) a user account.
        /// </summary>
        /// <param name="id">The unique integer identifier of the user to deactivate.</param>
        /// <response code="200">User deactivated successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks delete permissions.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await HasPermission("users.delete", "users.manage"))
            {
                return Forbid();
            }

            var success = await _userService.SoftDeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "User deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        /// <summary>
        /// Restore a previously deactivated user account.
        /// </summary>
        /// <param name="id">The unique integer identifier of the user to restore.</param>
        /// <response code="200">User account restored successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks restoration permissions.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("{id:int}/restore")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreUser(int id)
        {
            if (!await HasPermission("users.edit", "users.manage"))
            {
                return Forbid();
            }

            var success = await _userService.RestoreUserAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "User restored successfully!",
                Id = id,
                DeletedFlag = 1
            });
        }

        private async Task<bool> HasPermission(params string[] requiredPermissions)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return false;
            }
            return await _userService.HasPermissionAsync(userId, requiredPermissions);
        }
    }
}