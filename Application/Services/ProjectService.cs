using System;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectsOverviewResponse> GetProjectsAsync(string? category, string? status, string? search)
        {
            var (rawProjects, activeRollouts, onTrackCount, pendingReviews) =
                await _projectRepository.GetProjectsOverviewDataAsync(category, status, search);

            return new ProjectsOverviewResponse
            {
                ActiveRollouts = activeRollouts,
                OnTrackCount = onTrackCount,
                PendingReviewsCount = pendingReviews,
                Projects = rawProjects.ToDtoList(),
            };
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, string creatorName)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Project name is required.");
            }

            var name = request.Name.Trim();
            var description = request.Description.Trim();
            var category = string.IsNullOrWhiteSpace(request.Category) ? "RBAC Rollout" : request.Category.Trim();
            var status = string.IsNullOrWhiteSpace(request.Status) ? "In Progress" : request.Status.Trim();
            var priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim();
            var leadName = string.IsNullOrWhiteSpace(request.LeadName) ? creatorName : request.LeadName.Trim();
            var progress = Math.Clamp(request.ProgressPercentage, 0, 100);
            var dueDate = string.IsNullOrWhiteSpace(request.DueDate) ? DateTime.UtcNow.AddMonths(1).ToString("MMM dd, yyyy") : request.DueDate.Trim();

            var newId = await _projectRepository.CreateProjectAsync(name, description, category, status, priority, leadName, progress, dueDate);
            var createdProject = await _projectRepository.GetProjectByIdAsync(newId);

            return createdProject!.ToDto();
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request)
        {
            var updated = await _projectRepository.UpdateProjectAsync(
                id,
                request.Name.Trim(),
                request.Description.Trim(),
                request.Category.Trim(),
                request.Status.Trim(),
                request.Priority.Trim(),
                request.LeadName.Trim(),
                Math.Clamp(request.ProgressPercentage, 0, 100),
                request.DueDate.Trim());

            if (!updated) return null;

            var project = await _projectRepository.GetProjectByIdAsync(id);
            return project?.ToDto();
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            return await _projectRepository.SoftDeleteProjectAsync(id);
        }
    }
}
