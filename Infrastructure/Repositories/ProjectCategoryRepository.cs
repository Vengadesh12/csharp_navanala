using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class ProjectCategoryRepository : IProjectCategoryRepository
    {
        private readonly AppDbContext _context;

        public ProjectCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectCategory>> GetAllCategoriesAsync()
        {
            return await _context.ProjectCategories
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM project_categories
                    WHERE deleted_flag = 1
                    ORDER BY name ASC
                """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProjectCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.ProjectCategories
                .FromSqlRaw("""
                    SELECT id, name, description, deleted_flag, created_at, updated_at
                    FROM project_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CategoryExistsByNameAsync(string name)
        {
            var count = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM project_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """, name.Trim()).SingleOrDefaultAsync();

            return count > 0;
        }

        public async Task<ProjectCategory> AddCategoryAsync(ProjectCategory category)
        {
            _context.ProjectCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.ProjectCategories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedFlag == 1);
            if (category == null) return false;

            category.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
