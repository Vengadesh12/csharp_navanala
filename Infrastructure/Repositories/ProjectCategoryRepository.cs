using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
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

        public async Task<List<ProjectCategoryModel>> GetAllCategoriesAsync()
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM project_categories
                WHERE deleted_flag = 1
                ORDER BY name ASC
            """);

            return await _context.ProjectCategories
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProjectCategoryModel?> GetCategoryByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, deleted_flag, created_at, updated_at
                FROM project_categories
                WHERE id = {0} AND deleted_flag = 1
                LIMIT 1
            """);

            return await _context.ProjectCategories
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CategoryExistsByNameAsync(string name)
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM project_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """);

            var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), name.Trim()).SingleOrDefaultAsync();

            return count > 0;
        }

        public async Task<ProjectCategoryModel> AddCategoryAsync(ProjectCategoryModel category)
        {
            _context.ProjectCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.ProjectCategories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedFlag == 1);
            if (category == null) return false;

            category.DeletedFlag = 0;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
