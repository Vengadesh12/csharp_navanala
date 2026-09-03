using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectsOverviewResponse> GetProjectsAsync(string? category, string? status, string? search);
        Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, string creatorName);
        Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request);
        Task<bool> DeleteProjectAsync(int id);
    }
}
