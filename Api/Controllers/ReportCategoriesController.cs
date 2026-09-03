using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/report-categories")]
    [Tags("Report Categories")]
    [Produces("application/json")]
    [Authorize]
    public class ReportCategoriesController : ControllerBase
    {
        private readonly IReportCategoryService _reportCategoryService;

        public ReportCategoriesController(IReportCategoryService reportCategoryService)
        {
            _reportCategoryService = reportCategoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ReportCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _reportCategoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ReportCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _reportCategoryService.GetCategoryByIdAsync(id);
            if (category is null)
            {
                return NotFound(new ErrorResponse { Message = $"Report category with ID {id} not found." });
            }
            return Ok(category);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReportCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateReportCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Category name is required." });
            }

            try
            {
                var created = await _reportCategoryService.CreateCategoryAsync(request);
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = created.Id },
                    new ApiResponse<ReportCategoryDto>
                    {
                        Success = true,
                        Message = "Report category created and saved in database successfully!",
                        Data = created
                    });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _reportCategoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Report category with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Report category removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
