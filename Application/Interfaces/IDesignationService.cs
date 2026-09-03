using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IDesignationService
    {
        Task<List<DesignationDto>> GetAllDesignationsAsync();

        Task<DesignationDto?> GetDesignationByIdAsync(int id);

        Task<DesignationDto> CreateDesignationAsync(CreateDesignationRequest request);

        Task<DesignationDto?> UpdateDesignationAsync(int id, UpdateDesignationRequest request);

        Task<bool> DeleteDesignationAsync(int id);
    }
}
