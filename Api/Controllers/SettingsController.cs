using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Tags("Settings")]
    [Produces("application/json")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;

        public SettingsController(ISettingService settingService, IUserService userService)
        {
            _settingService = settingService;
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SettingsOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings([FromQuery] string? category, [FromQuery] string? search)
        {
            var response = await _settingService.GetSettingsAsync(category, search);
            return Ok(response);
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SettingCategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _settingService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost("categories")]
        [ProducesResponseType(typeof(ApiResponse<SettingCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateSettingCategoryRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var category = await _settingService.CreateCategoryAsync(request, callerName);

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new ApiResponse<SettingCategoryDto>
            {
                Success = true,
                Message = $"Category '{category.Name}' created and saved in database successfully!",
                Data = category
            });
        }

        [HttpPut("categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SettingCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateSettingCategoryRequest request)
        {
            var category = await _settingService.UpdateCategoryAsync(id, request);
            if (category == null)
            {
                return NotFound(new ErrorResponse { Message = $"Category with ID {id} not found." });
            }

            return Ok(new ApiResponse<SettingCategoryDto>
            {
                Success = true,
                Message = $"Category '{category.Name}' updated successfully!",
                Data = category
            });
        }

        [HttpDelete("categories/{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _settingService.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Category with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Category deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        [HttpPost("bulk")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateSettingsBulk([FromBody] UpdateSettingsBulkRequest request)
        {
            if (request?.Settings != null && request.Settings.ContainsKey("maintenance_mode"))
            {
                if (!await HasMaintenancePermissionAsync())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                    {
                        Message = "Access Denied. Only Super Admins, Administrators, or authorized users with the 'settings.maintenance' permission can toggle Maintenance Mode."
                    });
                }
            }

            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            await _settingService.UpdateSettingsBulkAsync(request, callerName);

            return Ok(new MessageResponse
            {
                Success = true,
                Message = "Workspace settings updated and persisted in database successfully!"
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SystemSettingDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSetting([FromBody] CreateSettingRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var setting = await _settingService.CreateSettingAsync(request, callerName);

            return CreatedAtAction(nameof(GetSettings), new { id = setting.Id }, new ApiResponse<SystemSettingDto>
            {
                Success = true,
                Message = "Setting key registered successfully!",
                Data = setting
            });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SystemSettingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSetting(int id, [FromBody] UpdateSettingRequest request)
        {
            if (string.Equals(request?.SettingKey, "maintenance_mode", StringComparison.OrdinalIgnoreCase))
            {
                if (!await HasMaintenancePermissionAsync())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                    {
                        Message = "Access Denied. Only Super Admins, Administrators, or authorized users with the 'settings.maintenance' permission can toggle Maintenance Mode."
                    });
                }
            }

            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var setting = await _settingService.UpdateSettingAsync(id, request, callerName);
            if (setting == null)
            {
                return NotFound(new ErrorResponse { Message = $"Setting with ID {id} not found." });
            }

            return Ok(new ApiResponse<SystemSettingDto>
            {
                Success = true,
                Message = $"Setting '{setting.SettingKey}' updated and persisted in database!",
                Data = setting
            });
        }

        private async Task<bool> HasMaintenancePermissionAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return false;

            var roleClaim = User.FindFirstValue(ClaimTypes.Role)?.ToLowerInvariant() ?? "";
            if (roleClaim.Contains("super admin") || roleClaim == "admin") return true;

            return await _userService.HasPermissionAsync(userId, "settings.maintenance");
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSetting(int id)
        {
            var success = await _settingService.DeleteSettingAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Setting with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Setting deleted from database successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
