using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for managing organizational departments and designation mappings.
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Retrieves complete department overview with stats, mapped designations, and unassigned items.
        /// </summary>
        Task<DepartmentOverviewResponse> GetDepartmentsOverviewAsync();

        /// <summary>
        /// Retrieves all active departments along with their mapped designations and member counts.
        /// </summary>
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();

        /// <summary>
        /// Retrieves a specific department by ID.
        /// </summary>
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);

        /// <summary>
        /// Creates a new department and optionally associates designations.
        /// </summary>
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);

        /// <summary>
        /// Updates an existing department and its designation mappings.
        /// </summary>
        Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);

        /// <summary>
        /// Deletes / soft-deletes a department and unassigns its designations.
        /// </summary>
        Task<bool> DeleteDepartmentAsync(int id);

        /// <summary>
        /// Maps or reassigns designated IDs to a specific department.
        /// </summary>
        Task<DepartmentDto?> MapDesignationsToDepartmentAsync(int departmentId, MapDepartmentDesignationsRequest request);
    }
}
