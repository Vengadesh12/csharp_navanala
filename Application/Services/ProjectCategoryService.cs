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
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM project_categories
                    WHERE deleted_flag = 1
                    ORDER BY name ASC
                """)
                .AsNoTracking()
                .Select(c => new ProjectCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description ?? string.Empty,
                    DeletedFlag = c.DeletedFlag,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ProjectCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.ProjectCategories
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM project_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
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
            var exists = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM project_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """, trimmedName).SingleOrDefaultAsync() > 0;

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
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM project_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .FirstOrDefaultAsync();

            if (category is null) return false;

            category.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
