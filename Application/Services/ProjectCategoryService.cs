using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class ProjectCategoryService : IProjectCategoryService
    {
        private readonly IProjectCategoryRepository _projectCategoryRepository;

        public ProjectCategoryService(IProjectCategoryRepository projectCategoryRepository)
        {
            _projectCategoryRepository = projectCategoryRepository;
        }

        public async Task<List<ProjectCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _projectCategoryRepository.GetAllCategoriesAsync();

            return categories.Select(c => new ProjectCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? string.Empty,
                DeletedFlag = c.DeletedFlag,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<ProjectCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _projectCategoryRepository.GetCategoryByIdAsync(id);
            if (category is null) return null;

            return new ProjectCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                DeletedFlag = category.DeletedFlag,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<ProjectCategoryDto> CreateCategoryAsync(CreateProjectCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Category name is required.");
            }

            var trimmedName = request.Name.Trim();
            var exists = await _projectCategoryRepository.CategoryExistsByNameAsync(trimmedName);

            if (exists)
            {
                throw new BadRequestException($"A project category with the name '{trimmedName}' already exists.");
            }

            var category = ProjectCategory.Create(trimmedName, request.Description);
            await _projectCategoryRepository.AddCategoryAsync(category);

            return new ProjectCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                DeletedFlag = category.DeletedFlag,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _projectCategoryRepository.SoftDeleteCategoryAsync(id);
        }
    }
}
