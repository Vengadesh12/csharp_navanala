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
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly AppDbContext _context;

        public ApprovalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ApprovalRequestModel> Items, int TotalCount)> GetApprovalsPagedAsync(
            int currentUserId,
            bool isManagerOrAdmin,
            string? scope,
            string? status,
            string? category,
            string? priority,
            string? search,
            int page,
            int pageSize)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, employee_name, employee_email, department_name, item_name, category, description, quantity, priority, estimated_amount, status, comments, reviewed_by_id, reviewed_by_name, reviewed_at, created_at, updated_at, deleted_flag
                FROM approval_requests
                WHERE deleted_flag = 1
            """);

            var countSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM approval_requests
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!isManagerOrAdmin || string.Equals(scope, "my", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND user_id = {{{paramIndex++}}}";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND LOWER(status) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND LOWER(category) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(priority) && !string.Equals(priority, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND LOWER(priority) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(priority.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                var clause = $" AND (LOWER(employee_name) LIKE {{{paramIndex}}} OR LOWER(employee_email) LIKE {{{paramIndex}}} OR LOWER(item_name) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR (department_name IS NOT NULL AND LOWER(department_name) LIKE {{{paramIndex}}}) OR (comments IS NOT NULL AND LOWER(comments) LIKE {{{paramIndex++}}}))";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(pattern);
            }

            var totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql.ToString(), parameters.ToArray())
                .SingleOrDefaultAsync();

            sql.Append(" ORDER BY id DESC");

            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 50;
            var offset = (pageNum - 1) * size;

            sql.Append($" LIMIT {{{paramIndex++}}} OFFSET {{{paramIndex++}}}");
            parameters.Add(size);
            parameters.Add(offset);

            var items = await _context.Approvals
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(int TotalRequests, int PendingCount, int ApprovedCount, int RejectedCount, int MyRequestsCount)> GetSummaryAsync(
            int currentUserId,
            bool isManagerOrAdmin)
        {
            var totalSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM approval_requests WHERE deleted_flag = 1");
            var pendingSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM approval_requests WHERE deleted_flag = 1 AND LOWER(status) = 'pending'");
            var approvedSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM approval_requests WHERE deleted_flag = 1 AND LOWER(status) = 'approved'");
            var rejectedSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM approval_requests WHERE deleted_flag = 1 AND LOWER(status) = 'rejected'");
            var myRequestsSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM approval_requests WHERE deleted_flag = 1 AND user_id = {0}");

            int total, pending, approved, rejected;

            if (isManagerOrAdmin)
            {
                total = await _context.Database.SqlQueryRaw<int>(totalSql.ToString()).SingleOrDefaultAsync();
                pending = await _context.Database.SqlQueryRaw<int>(pendingSql.ToString()).SingleOrDefaultAsync();
                approved = await _context.Database.SqlQueryRaw<int>(approvedSql.ToString()).SingleOrDefaultAsync();
                rejected = await _context.Database.SqlQueryRaw<int>(rejectedSql.ToString()).SingleOrDefaultAsync();
            }
            else
            {
                totalSql.Append(" AND user_id = {0}");
                pendingSql.Append(" AND user_id = {0}");
                approvedSql.Append(" AND user_id = {0}");
                rejectedSql.Append(" AND user_id = {0}");

                total = await _context.Database.SqlQueryRaw<int>(totalSql.ToString(), currentUserId).SingleOrDefaultAsync();
                pending = await _context.Database.SqlQueryRaw<int>(pendingSql.ToString(), currentUserId).SingleOrDefaultAsync();
                approved = await _context.Database.SqlQueryRaw<int>(approvedSql.ToString(), currentUserId).SingleOrDefaultAsync();
                rejected = await _context.Database.SqlQueryRaw<int>(rejectedSql.ToString(), currentUserId).SingleOrDefaultAsync();
            }

            var myRequests = await _context.Database.SqlQueryRaw<int>(myRequestsSql.ToString(), currentUserId).SingleOrDefaultAsync();

            return (total, pending, approved, rejected, myRequests);
        }

        public async Task<ApprovalRequestModel?> GetByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, employee_name, employee_email, department_name, item_name, category, description, quantity, priority, estimated_amount, status, comments, reviewed_by_id, reviewed_by_name, reviewed_at, created_at, updated_at, deleted_flag
                FROM approval_requests
                WHERE id = {0} AND deleted_flag = 1
                LIMIT 1
            """);

            return await _context.Approvals
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ApprovalRequestModel> AddApprovalAsync(ApprovalRequestModel approval)
        {
            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
            return approval;
        }

        public async Task UpdateApprovalAsync(ApprovalRequestModel approval)
        {
            _context.Approvals.Update(approval);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteApprovalAsync(int id)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a => a.Id == id && a.DeletedFlag == 1);
            if (approval == null) return false;

            approval.DeletedFlag = 0;
            approval.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApprovalRequestModel>> GetApprovedApprovalsAsync()
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, employee_name, employee_email, department_name, item_name, category, description, quantity, priority, estimated_amount, status, comments, reviewed_by_id, reviewed_by_name, reviewed_at, created_at, updated_at, deleted_flag
                FROM approval_requests
                WHERE deleted_flag = 1 AND LOWER(status) = 'approved'
                ORDER BY COALESCE(reviewed_at, created_at) DESC
            """);

            return await _context.Approvals
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
