using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
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

        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

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
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return false;
            }
            return await _userService.HasPermissionAsync(userId, requiredPermissions);
        }
    }
}
