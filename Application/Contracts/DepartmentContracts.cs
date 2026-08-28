using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Contracts
{
    /// <summary>
    /// Representation of an organizational department returned in API responses.
    /// </summary>
    public sealed class DepartmentDto
    {
        /// <summary>
        /// Unique department identifier.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of the department.
        /// </summary>
        /// <example>Software Development</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the department scope and responsibilities.
        /// </summary>
        /// <example>Core engineering, application architecture, and development teams.</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Status flag (1 = Active, 0 = Deactivated).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Timestamp when the department was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Count of active designations currently assigned to this department.
        /// </summary>
        /// <example>5</example>
        public int DesignationCount { get; set; }

        /// <summary>
        /// Count of workspace users holding designations under this department.
        /// </summary>
        /// <example>18</example>
        public int UserCount { get; set; }

        /// <summary>
        /// List of active designations mapped to this department.
        /// </summary>
        public List<DesignationDto> Designations { get; set; } = new();
    }

    /// <summary>
    /// Payload required to create a new department.
    /// </summary>
    public sealed class CreateDepartmentRequest
    {
        /// <summary>
        /// Name of the department.
        /// </summary>
        /// <example>Software Development</example>
        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the department.
        /// </summary>
        /// <example>Core engineering, application architecture, and development teams.</example>
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional list of designation IDs to map to this department during creation.
        /// </summary>
        public List<int>? DesignationIds { get; set; }
    }

    /// <summary>
    /// Payload required to update an existing department.
    /// </summary>
    public sealed class UpdateDepartmentRequest
    {
        /// <summary>
        /// Updated department name.
        /// </summary>
        /// <example>Software Development</example>
        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Updated description of the department.
        /// </summary>
        /// <example>Core engineering, application architecture, and development teams.</example>
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional updated list of designation IDs mapped to this department.
        /// If specified, designations not in this list will have their DepartmentId cleared (or unassigned).
        /// </summary>
        public List<int>? DesignationIds { get; set; }
    }

    /// <summary>
    /// Payload to map or reassign designation IDs to a specific department.
    /// </summary>
    public sealed class MapDepartmentDesignationsRequest
    {
        /// <summary>
        /// List of designation IDs to associate with this department.
        /// </summary>
        [Required(ErrorMessage = "At least one designation ID is required.")]
        public List<int> DesignationIds { get; set; } = new();
    }

    /// <summary>
    /// Department overview summary response with statistics and tree hierarchy.
    /// </summary>
    public sealed class DepartmentOverviewResponse
    {
        /// <summary>
        /// Total number of active departments.
        /// </summary>
        public int TotalDepartments { get; set; }

        /// <summary>
        /// Total number of active designations.
        /// </summary>
        public int TotalDesignations { get; set; }

        /// <summary>
        /// Number of designations mapped to at least one department.
        /// </summary>
        public int MappedDesignations { get; set; }

        /// <summary>
        /// Number of designations currently unassigned to any department.
        /// </summary>
        public int UnassignedDesignations { get; set; }

        /// <summary>
        /// List of departments with their mapped designations.
        /// </summary>
        public List<DepartmentDto> Departments { get; set; } = new();

        /// <summary>
        /// Designations that do not belong to any department.
        /// </summary>
        public List<DesignationDto> UnassignedList { get; set; } = new();
    }
}
