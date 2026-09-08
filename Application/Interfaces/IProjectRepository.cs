using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IProjectRepository
    {
        Task<(List<ProjectModel> Projects, int ActiveRollouts, int OnTrackCount, int PendingReviews)> GetProjectsOverviewDataAsync(string? category, string? status, string? search);

        Task<int> CreateProjectAsync(string name, string description, string category, string status, string priority, string leadName, int progressPercentage, string dueDate);

        Task<ProjectModel?> GetProjectByIdAsync(int id);

        Task<bool> UpdateProjectAsync(int id, string name, string description, string category, string status, string priority, string leadName, int progressPercentage, string dueDate);

        Task<bool> SoftDeleteProjectAsync(int id);
    }
}
