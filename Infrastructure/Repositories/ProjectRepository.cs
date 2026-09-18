using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ProjectModel> Projects, int ActiveRollouts, int OnTrackCount, int PendingReviews)> GetProjectsOverviewDataAsync(string? category, string? status, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, updated_at, deleted_flag
                FROM projects
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            {
                sql.Append($" AND LOWER(category) = LOWER({{{paramIndex++}}})");
                parameters.Add(category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
            {
                sql.Append($" AND LOWER(status) = LOWER({{{paramIndex++}}})");
                parameters.Add(status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(name) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR LOWER(lead_name) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY id DESC");

            var rawProjects = await _context.Projects
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var activeRolloutsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND status = 'In Progress'
            """);
            var activeRollouts = await _context.Database.SqlQueryRaw<int>(activeRolloutsSql.ToString()).SingleOrDefaultAsync();

            var onTrackCountSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND progress_percentage >= 50
            """);
            var onTrackCount = await _context.Database.SqlQueryRaw<int>(onTrackCountSql.ToString()).SingleOrDefaultAsync();

            var pendingReviewsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND status = 'Review'
            """);
            var pendingReviews = await _context.Database.SqlQueryRaw<int>(pendingReviewsSql.ToString()).SingleOrDefaultAsync();

            return (rawProjects, activeRollouts, onTrackCount, pendingReviews);
        }

        public Task<int> CreateProjectAsync(string name, string description, string category, string status, string priority, string leadName, int progressPercentage, string dueDate)
        {
            var now = DateTime.UtcNow;
            var insertSql = new StringBuilder("""
                INSERT INTO projects (name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, updated_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {8}, 1)
                RETURNING id AS "Value"
            """);

            var id = _context.Database.SqlQueryRaw<int>(
                insertSql.ToString(),
                name, description, category, status, priority, leadName, progressPercentage, dueDate, now)
            .AsEnumerable()
            .Single();

            return Task.FromResult(id);
        }

        public async Task<ProjectModel?> GetProjectByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, updated_at, deleted_flag
                FROM projects
                WHERE id = {0} AND deleted_flag = 1
            """);

            return await _context.Projects
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateProjectAsync(int id, string name, string description, string category, string status, string priority, string leadName, int progressPercentage, string dueDate)
        {
            var now = DateTime.UtcNow;
            var updateSql = new StringBuilder("""
                UPDATE projects
                SET name = {0}, description = {1}, category = {2}, status = {3}, priority = {4}, lead_name = {5}, progress_percentage = {6}, due_date = {7}, updated_at = {8}
                WHERE id = {9} AND deleted_flag = 1
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(
                updateSql.ToString(),
                name, description, category, status, priority, leadName, progressPercentage, dueDate, now, id);

            return rows > 0;
        }

        public async Task<bool> SoftDeleteProjectAsync(int id)
        {
            var now = DateTime.UtcNow;
            var deleteSql = new StringBuilder("""
                UPDATE projects
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(deleteSql.ToString(), now, id);

            return rows > 0;
        }
    }
}
