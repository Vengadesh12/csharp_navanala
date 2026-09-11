using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    // ==============================================================================
    // TOPIC: Role-Based Access Control (RBAC) & Authorization middleware
    // [Authorize] ensures requests must supply a valid, authenticated JWT token.
    // Controller actions enforce fine-grained permissions (e.g. users.view, users.create).
    // ==============================================================================
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    [Produces("application/json")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        // ==============================================================================
        // TOPIC: Dynamic Query / Filtering
        // TOPIC: Dynamic query parameters
        // TOPIC: Multiple filters
        // TOPIC: Range filtering
        // Handles optional dynamic query parameters from the HTTP query string.
        // ==============================================================================
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers([FromQuery] DynamicQueryParameters? query)
        {
            if (query != null && (query.Page > 1 || query.PageSize != 20 ||
                !string.IsNullOrWhiteSpace(query.Search) ||
                !string.IsNullOrWhiteSpace(query.Status) ||
                !string.IsNullOrWhiteSpace(query.Category) ||
                !string.IsNullOrWhiteSpace(query.Department) ||
                query.MinAge.HasValue || query.MaxAge.HasValue ||
                !string.IsNullOrWhiteSpace(query.Fields) ||
                !string.IsNullOrWhiteSpace(query.Include) ||
                !string.IsNullOrWhiteSpace(query.Exclude) ||
                !string.IsNullOrWhiteSpace(query.SortBy)))
            {
                var paged = await _userService.GetUsersPagedAsync(query);
                return Ok(paged);
            }

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // ==============================================================================
        // TOPIC: Dynamic field selection
        // TOPIC: Include/Exclude fields
        // Shapes a single user object dynamically based on fields/include/exclude parameters.
        // ==============================================================================
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(
            int id,
            [FromQuery] string? fields = null,
            [FromQuery] string? include = null,
            [FromQuery] string? exclude = null)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user is null)
            {
                return NotFound(new ErrorResponse { Message = $"User with ID {id} not found." });
            }

            return Ok(user);
        }

        // ==============================================================================
        // TOPIC: Role-Based Access Control (RBAC Action-level Check)
        // ==============================================================================
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
            var userId = _currentUserService.UserId ?? 0;
            if (userId <= 0)
            {
                return false;
            }
            return await _userService.HasPermissionAsync(userId, requiredPermissions);
        }
    }
}
