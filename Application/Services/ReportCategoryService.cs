using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ReportCategoryService : IReportCategoryService
    {
        private readonly IReportRepository _reportRepository;

        public ReportCategoryService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<ReportCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _reportRepository.GetAllCategoriesAsync();

            return categories.Select(c => new ReportCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description ?? string.Empty,
                DeletedFlag = c.DeletedFlag,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<ReportCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _reportRepository.GetCategoryByIdAsync(id);
            if (category is null) return null;

            return new ReportCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                DeletedFlag = category.DeletedFlag,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<ReportCategoryDto> CreateCategoryAsync(CreateReportCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Category name is required.");
            }

            var trimmedName = request.Name.Trim();
            var exists = await _reportRepository.CategoryExistsByNameAsync(trimmedName);

            if (exists)
            {
                throw new BadRequestException($"A report category with the name '{trimmedName}' already exists.");
            }

            var category = new ReportCategory
            {
                Name = trimmedName,
                Description = request.Description?.Trim() ?? string.Empty,
                DeletedFlag = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _reportRepository.AddCategoryAsync(category);

            return new ReportCategoryDto
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
            return await _reportRepository.SoftDeleteCategoryAsync(id);
        }
    }
}
