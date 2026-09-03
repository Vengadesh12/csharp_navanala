using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentOverviewResponse> GetDepartmentsOverviewAsync();

        Task<List<DepartmentDto>> GetAllDepartmentsAsync();

        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);

        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);

        Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);

        Task<bool> DeleteDepartmentAsync(int id);

        Task<DepartmentDto?> MapDesignationsToDepartmentAsync(int departmentId, MapDepartmentDesignationsRequest request);
    }
}
