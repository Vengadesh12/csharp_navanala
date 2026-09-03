using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// User account directory and profile management endpoints (Legacy route support).
    /// </summary>
    [ApiController]
    [Route("get")]
    [Tags("Users")]
    [Produces("application/json")]
    [Authorize]
    public class GetController : ControllerBase
    {
        private readonly IUserService _userService;

        public GetController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieve all users (Legacy route).
        /// </summary>
        /// <remarks>
        /// Legacy endpoint returning all registered users in the workspace. Recommended standard route is <c>GET /api/users</c>.
        /// </remarks>
        /// <response code="200">List of users retrieved successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetData()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
