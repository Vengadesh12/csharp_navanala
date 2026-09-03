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
    [Route("api/settings")]
    [Tags("Settings")]
    [Produces("application/json")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Retrieve all workspace settings and parameters.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SettingsOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings([FromQuery] string? category, [FromQuery] string? search)
        {
            var response = await _settingService.GetSettingsAsync(category, search);
            return Ok(response);
        }

        /// <summary>
        /// Retrieve all configuration categories registered in database.
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SettingCategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _settingService.GetCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Create and persist a new configuration category.
        /// </summary>
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

        /// <summary>
        /// Update an existing configuration category.
        /// </summary>
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

        /// <summary>
        /// Delete / soft-delete a configuration category.
        /// </summary>
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

        /// <summary>
        /// Update multiple workspace configuration settings in bulk.
        /// </summary>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSettingsBulk([FromBody] UpdateSettingsBulkRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            await _settingService.UpdateSettingsBulkAsync(request, callerName);

            return Ok(new MessageResponse
            {
                Success = true,
                Message = "Workspace settings updated and persisted in database successfully!"
            });
        }

        /// <summary>
        /// Create or register a custom system configuration key.
        /// </summary>
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

        /// <summary>
        /// Update an existing system configuration setting.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SystemSettingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSetting(int id, [FromBody] UpdateSettingRequest request)
        {
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

        /// <summary>
        /// Delete a configuration setting from the database.
        /// </summary>
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
