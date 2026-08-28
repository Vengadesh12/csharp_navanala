using System.Text;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IApplicationDbContext _context;

        public ProjectService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectsOverviewResponse> GetProjectsAsync(string? category, string? status, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag
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

            var projects = await _context.Projects
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var activeRollouts = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND status = 'In Progress'
            """).SingleOrDefaultAsync();

            var onTrackCount = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND progress_percentage >= 50
            """).SingleOrDefaultAsync();

            var pendingReviews = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM projects
                WHERE deleted_flag = 1 AND status = 'Review'
            """).SingleOrDefaultAsync();

            return new ProjectsOverviewResponse
            {
                ActiveRollouts = activeRollouts,
                OnTrackCount = onTrackCount,
                PendingReviewsCount = pendingReviews,
                Projects = projects
            };
        }

        public async Task<Project> CreateProjectAsync(CreateProjectRequest request, string creatorName)
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
            var now = DateTime.UtcNow;

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO projects (name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, 1)
                RETURNING id AS "Value"
            """, name, description, category, status, priority, leadName, progress, dueDate, now).SingleAsync();

            var createdProject = await _context.Projects
                .FromSqlRaw("""
                    SELECT id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag
                    FROM projects
                    WHERE id = {0} AND deleted_flag = 1
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return createdProject!;
        }

        public async Task<Project?> UpdateProjectAsync(int id, UpdateProjectRequest request)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE projects
                SET name = {0}, description = {1}, category = {2}, status = {3}, priority = {4}, lead_name = {5}, progress_percentage = {6}, due_date = {7}
                WHERE id = {8} AND deleted_flag = 1
            """, request.Name.Trim(), request.Description.Trim(), request.Category.Trim(), request.Status.Trim(), request.Priority.Trim(), request.LeadName.Trim(), Math.Clamp(request.ProgressPercentage, 0, 100), request.DueDate.Trim(), id);

            if (rowsAffected == 0) return null;

            return await _context.Projects
                .FromSqlRaw("""
                    SELECT id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag
                    FROM projects
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE projects
                SET deleted_flag = 0
                WHERE id = {0} AND deleted_flag = 1
            """, id);

            return rowsAffected > 0;
        }
    }
}
