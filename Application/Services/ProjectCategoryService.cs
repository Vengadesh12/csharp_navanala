using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements project category management and queries.
    /// </summary>
    public class ProjectCategoryService : IProjectCategoryService
    {
        private readonly IApplicationDbContext _context;

        public ProjectCategoryService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectCategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.ProjectCategories
                .Where(c => c.DeletedFlag == 1)
                .OrderBy(c => c.Name)
                .Select(c => new ProjectCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description ?? string.Empty,
                    DeletedFlag = c.DeletedFlag,
                    CreatedAt = c.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProjectCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.ProjectCategories
                .Where(c => c.Id == id && c.DeletedFlag == 1)
                .AsNoTracking()
                .FirstOrDefaultAsync();

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
            var exists = await _context.ProjectCategories
                .AnyAsync(c => c.DeletedFlag == 1 && c.Name.ToLower() == trimmedName.ToLower());

            if (exists)
            {
                throw new BadRequestException($"A project category with the name '{trimmedName}' already exists.");
            }

            var category = ProjectCategory.Create(trimmedName, request.Description);

            _context.ProjectCategories.Add(category);
            await _context.SaveChangesAsync();

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
            var category = await _context.ProjectCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedFlag == 1);

            if (category is null) return false;

            category.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
