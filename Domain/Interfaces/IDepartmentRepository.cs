using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    /// <summary>
    /// Specialized repository contract for Department entity queries and operations.
    /// </summary>
    public interface IDepartmentRepository : IRepository<Department>
    {
        /// <summary>
        /// Retrieves all active workspace departments including their mapped active designations.
        /// </summary>
        Task<List<Department>> GetActiveDepartmentsWithDesignationsAsync();

        /// <summary>
        /// Retrieves an active department by ID with its mapped active designations.
        /// </summary>
        Task<Department?> GetActiveDepartmentByIdAsync(int id);

        /// <summary>
        /// Retrieves a lookup dictionary of Department ID to Department Name for active departments.
        /// </summary>
        Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync();

        /// <summary>
        /// Checks whether a department with the given name already exists.
        /// </summary>
        Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null);
    }
}
