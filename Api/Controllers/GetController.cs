using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBackend.Api.Controllers
{
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
