using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/menus")]
    [Tags("Menus")]
    [Produces("application/json")]
    [Authorize]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Retrieves the list of accessible menus for the current authenticated user based on JWT token and role permissions.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MenuItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserMenus()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "A valid user token is required." });
            }

            var menus = await _menuService.GetUserMenusAsync(userId);
            return Ok(menus);
        }

        /// <summary>
        /// Retrieves all configured menus in the system (for administrative purposes).
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<MenuItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenus()
        {
            var menus = await _menuService.GetAllMenusAsync();
            return Ok(menus);
        }
    }
}
