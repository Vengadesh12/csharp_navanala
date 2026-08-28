using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Workspace job designations management and catalog endpoints.
    /// </summary>
    [ApiController]
    [Route("api/designations")]
    [Tags("Designations")]
    [Produces("application/json")]
    [Authorize]
    public class DesignationsController : ControllerBase
    {
        private readonly IDesignationService _designationService;

        public DesignationsController(IDesignationService designationService)
        {
            _designationService = designationService;
        }

        /// <summary>
        /// Retrieves all active designations.
        /// </summary>
        /// <response code="200">List of designations returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<DesignationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDesignations()
        {
            var designations = await _designationService.GetAllDesignationsAsync();
            return Ok(designations);
        }

        /// <summary>
        /// Retrieves a specific designation by ID.
        /// </summary>
        /// <param name="id">Designation ID.</param>
        /// <response code="200">Designation details returned.</response>
        /// <response code="404">Designation not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DesignationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDesignation(int id)
        {
            var designation = await _designationService.GetDesignationByIdAsync(id);
            if (designation is null)
            {
                return NotFound(new ErrorResponse { Message = $"Designation with ID {id} not found." });
            }
            return Ok(designation);
        }

        /// <summary>
        /// Creates and saves a new job designation in the system.
        /// </summary>
        /// <param name="request">New designation creation payload.</param>
        /// <response code="201">Designation created successfully.</response>
        /// <response code="400">Invalid payload or designation name already exists.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateDesignation([FromBody] CreateDesignationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Designation name is required." });
            }

            try
            {
                var created = await _designationService.CreateDesignationAsync(request);
                return CreatedAtAction(
                    nameof(GetDesignation),
                    new { id = created.Id },
                    new ApiResponse<DesignationDto>
                    {
                        Success = true,
                        Message = "Designation created successfully!",
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
        /// Updates an existing job designation.
        /// </summary>
        /// <param name="id">Designation ID.</param>
        /// <param name="request">Updated designation payload.</param>
        /// <response code="200">Designation updated successfully.</response>
        /// <response code="400">Invalid payload.</response>
        /// <response code="404">Designation not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DesignationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDesignation(int id, [FromBody] UpdateDesignationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Designation name is required." });
            }

            try
            {
                var updated = await _designationService.UpdateDesignationAsync(id, request);
                if (updated is null)
                {
                    return NotFound(new ErrorResponse { Message = $"Designation with ID {id} not found." });
                }

                return Ok(new ApiResponse<DesignationDto>
                {
                    Success = true,
                    Message = "Designation updated successfully!",
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
        /// Deletes / soft-deletes a job designation.
        /// </summary>
        /// <param name="id">Designation ID.</param>
        /// <response code="200">Designation deleted successfully.</response>
        /// <response code="404">Designation not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            var success = await _designationService.DeleteDesignationAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Designation with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Designation deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
