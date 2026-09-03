using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Workspace department management and designation hierarchy mapping endpoints.
    /// </summary>
    [ApiController]
    [Route("api/departments")]
    [Tags("Departments")]
    [Produces("application/json")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /// <summary>
        /// Retrieves complete department overview with hierarchy tree statistics and unassigned designations.
        /// </summary>
        /// <response code="200">Department overview and tree hierarchy returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(DepartmentOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _departmentService.GetDepartmentsOverviewAsync();
            return Ok(overview);
        }

        /// <summary>
        /// Retrieves all active departments with mapped designations.
        /// </summary>
        /// <response code="200">List of active departments returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        /// <summary>
        /// Retrieves a specific department by ID.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <response code="200">Department details returned.</response>
        /// <response code="404">Department not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department is null)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }
            return Ok(department);
        }

        /// <summary>
        /// Creates a new department and optionally associates designations.
        /// </summary>
        /// <param name="request">Department creation payload.</param>
        /// <response code="201">Department created successfully.</response>
        /// <response code="400">Invalid payload or department name already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Department name is required." });
            }

            try
            {
                var created = await _departmentService.CreateDepartmentAsync(request);
                return CreatedAtAction(
                    nameof(GetDepartment),
                    new { id = created.Id },
                    new ApiResponse<DepartmentDto>
                    {
                        Success = true,
                        Message = "Department created successfully!",
                        Data = created
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing department and its designation mappings.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <param name="request">Updated department payload.</param>
        /// <response code="200">Department updated successfully.</response>
        /// <response code="400">Invalid payload.</response>
        /// <response code="404">Department not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Department name is required." });
            }

            try
            {
                var updated = await _departmentService.UpdateDepartmentAsync(id, request);
                if (updated is null)
                {
                    return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
                }

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department updated successfully!",
                    Data = updated
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes / soft-deletes a department.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <response code="200">Department deleted successfully.</response>
        /// <response code="404">Department not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var success = await _departmentService.DeleteDepartmentAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Department deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        /// <summary>
        /// Maps or assigns specific designations to this department.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <param name="request">Designation IDs to map.</param>
        /// <response code="200">Designations mapped successfully.</response>
        /// <response code="404">Department not found.</response>
        [HttpPost("{id:int}/map-designations")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MapDesignations(int id, [FromBody] MapDepartmentDesignationsRequest request)
        {
            var updated = await _departmentService.MapDesignationsToDepartmentAsync(id, request);
            if (updated is null)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }

            return Ok(new ApiResponse<DepartmentDto>
            {
                Success = true,
                Message = "Designations mapped to department successfully!",
                Data = updated
            });
        }
    }
}
