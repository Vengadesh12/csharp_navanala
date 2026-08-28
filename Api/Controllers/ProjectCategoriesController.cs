using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Workspace project categories management and lookup endpoints.
    /// </summary>
    [ApiController]
    [Route("api/project-categories")]
    [Tags("Project Categories")]
    [Produces("application/json")]
    [Authorize]
    public class ProjectCategoriesController : ControllerBase
    {
        private readonly IProjectCategoryService _projectCategoryService;

        public ProjectCategoriesController(IProjectCategoryService projectCategoryService)
        {
            _projectCategoryService = projectCategoryService;
        }

        /// <summary>
        /// Retrieves all active project categories from the database.
        /// </summary>
        /// <response code="200">List of project categories returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProjectCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _projectCategoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Retrieves a specific project category by ID.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <response code="200">Category details returned.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProjectCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _projectCategoryService.GetCategoryByIdAsync(id);
            if (category is null)
            {
                return NotFound(new ErrorResponse { Message = $"Project category with ID {id} not found." });
            }
            return Ok(category);
        }

        /// <summary>
        /// Creates and stores a new project category in the database.
        /// </summary>
        /// <param name="request">New project category payload.</param>
        /// <response code="201">Project category created successfully.</response>
        /// <response code="400">Invalid payload or category name already exists.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProjectCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateProjectCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Category name is required." });
            }

            try
            {
                var created = await _projectCategoryService.CreateCategoryAsync(request);
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = created.Id },
                    new ApiResponse<ProjectCategoryDto>
                    {
                        Success = true,
                        Message = "Project category created and saved in database successfully!",
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

        /// <summary>
        /// Deletes / soft-deletes a project category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _projectCategoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Project category with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Project category removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
