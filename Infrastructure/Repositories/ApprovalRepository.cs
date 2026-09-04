using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
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

        public async Task<(List<ApprovalRequest> Items, int TotalCount)> GetApprovalsPagedAsync(
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
            IQueryable<ApprovalRequest> queryable = _context.Approvals
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1);

            if (!isManagerOrAdmin || string.Equals(scope, "my", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(a => a.UserId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetStatus = status.Trim().ToLower();
                queryable = queryable.Where(a => a.Status.ToLower() == targetStatus);
            }

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetCategory = category.Trim().ToLower();
                queryable = queryable.Where(a => a.Category.ToLower() == targetCategory);
            }

            if (!string.IsNullOrWhiteSpace(priority) && !string.Equals(priority, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetPriority = priority.Trim().ToLower();
                queryable = queryable.Where(a => a.Priority.ToLower() == targetPriority);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                queryable = queryable.Where(a =>
                    a.EmployeeName.ToLower().Contains(s) ||
                    a.EmployeeEmail.ToLower().Contains(s) ||
                    a.ItemName.ToLower().Contains(s) ||
                    a.Description.ToLower().Contains(s) ||
                    (a.DepartmentName != null && a.DepartmentName.ToLower().Contains(s)) ||
                    (a.Comments != null && a.Comments.ToLower().Contains(s))
                );
            }

            queryable = queryable.OrderByDescending(a => a.Id);

            var totalCount = await queryable.CountAsync();
            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 50;

            var items = await queryable
                .Skip((pageNum - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(int TotalRequests, int PendingCount, int ApprovedCount, int RejectedCount, int MyRequestsCount)> GetSummaryAsync(
            int currentUserId,
            bool isManagerOrAdmin)
        {
            var baseQuery = _context.Approvals.Where(a => a.DeletedFlag == 1);

            int total, pending, approved, rejected;

            if (isManagerOrAdmin)
            {
                total = await baseQuery.CountAsync();
                pending = await baseQuery.CountAsync(a => a.Status.ToLower() == "pending");
                approved = await baseQuery.CountAsync(a => a.Status.ToLower() == "approved");
                rejected = await baseQuery.CountAsync(a => a.Status.ToLower() == "rejected");
            }
            else
            {
                var userQuery = baseQuery.Where(a => a.UserId == currentUserId);
                total = await userQuery.CountAsync();
                pending = await userQuery.CountAsync(a => a.Status.ToLower() == "pending");
                approved = await userQuery.CountAsync(a => a.Status.ToLower() == "approved");
                rejected = await userQuery.CountAsync(a => a.Status.ToLower() == "rejected");
            }

            var myRequests = await _context.Approvals
                .Where(a => a.DeletedFlag == 1 && a.UserId == currentUserId)
                .CountAsync();

            return (total, pending, approved, rejected, myRequests);
        }

        public async Task<ApprovalRequest?> GetByIdAsync(int id)
        {
            return await _context.Approvals
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedFlag == 1);
        }

        public async Task<ApprovalRequest> AddApprovalAsync(ApprovalRequest approval)
        {
            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
            return approval;
        }

        public async Task UpdateApprovalAsync(ApprovalRequest approval)
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

        public async Task<List<ApprovalRequest>> GetApprovedApprovalsAsync()
        {
            return await _context.Approvals
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1 && a.Status.ToLower() == "approved")
                .OrderByDescending(a => a.ReviewedAt ?? a.CreatedAt)
                .ToListAsync();
        }
    }
}
