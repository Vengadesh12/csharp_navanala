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
    [Route("api/projects")]
    [Tags("Projects")]
    [Produces("application/json")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Retrieve all RBAC and deployment projects with summary metrics.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProjectsOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjects([FromQuery] string? category, [FromQuery] string? status, [FromQuery] string? search)
        {
            var response = await _projectService.GetProjectsAsync(category, status, search);
            return Ok(response);
        }

        /// <summary>
        /// Create a new project initiative in the database.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Lead";
            var project = await _projectService.CreateProjectAsync(request, callerName);

            return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, new ApiResponse<ProjectDto>
            {
                Success = true,
                Message = "Project created and stored in database successfully!",
                Data = project
            });
        }

        /// <summary>
        /// Update an existing project's status, progress, or parameters.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            var project = await _projectService.UpdateProjectAsync(id, request);
            if (project == null)
            {
                return NotFound(new ErrorResponse { Message = $"Project with ID {id} not found." });
            }

            return Ok(new ApiResponse<ProjectDto>
            {
                Success = true,
                Message = "Project updated successfully!",
                Data = project
            });
        }

        /// <summary>
        /// Delete / soft-delete a project.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Project with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Project removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
