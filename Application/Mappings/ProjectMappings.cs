using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Mappings
{
    public static class ProjectMappings
    {
        public static ProjectDto ToDto(this ProjectModel project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                Category = project.Category,
                Status = project.Status,
                Priority = project.Priority,
                LeadName = project.LeadName ?? string.Empty,
                ProgressPercentage = project.ProgressPercentage,
                DueDate = project.DueDate ?? string.Empty,
                CreatedAt = project.CreatedAt,
                DeletedFlag = project.DeletedFlag
            };
        }

        public static List<ProjectDto> ToDtoList(this IEnumerable<ProjectModel> projects)
        {
            return projects.Select(p => p.ToDto()).ToList();
        }

        public static ProjectCategoryDto ToDto(this ProjectCategoryModel cat)
        {
            return new ProjectCategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description ?? string.Empty,
                DeletedFlag = cat.DeletedFlag,
                CreatedAt = cat.CreatedAt
            };
        }

        public static List<ProjectCategoryDto> ToDtoList(this IEnumerable<ProjectCategoryModel> categories)
        {
            return categories.Select(c => c.ToDto()).ToList();
        }
    }
}
