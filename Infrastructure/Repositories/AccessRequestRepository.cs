using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class AccessRequestRepository : IAccessRequestRepository
    {
        private readonly AppDbContext _context;

        public AccessRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetPendingKeysForUserAsync(int userId)
        {
            return await _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.Status == "Pending" && r.DeletedFlag == 1)
                .Select(r => r.PermissionKey)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetAllActivePermissionsAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<List<AccessRequest>> GetRequestsForUserAsync(int userId)
        {
            return await _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.DeletedFlag == 1)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Permission?> GetPermissionByKeyAsync(string permKey)
        {
            return await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionKey.ToLower() == permKey.ToLower() && p.DeletedFlag == 1);
        }

        public async Task<bool> HasPendingRequestAsync(int userId, string permKey)
        {
            return await _context.AccessRequests
                .AnyAsync(r => r.UserId == userId &&
                               r.PermissionKey.ToLower() == permKey.ToLower() &&
                               r.Status == "Pending" &&
                               r.DeletedFlag == 1);
        }

        public async Task<string?> GetDepartmentNameForDesignationAsync(int designationId)
        {
            return await (from des in _context.Designations
                          join d in _context.Departments on des.DepartmentId equals d.Id
                          where des.Id == designationId
                          select d.Name).FirstOrDefaultAsync();
        }

        public async Task<AccessRequest> AddRequestAsync(AccessRequest request)
        {
            _context.AccessRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<(List<AccessRequest> Items, int TotalCount)> GetPagedRequestsAsync(
            bool onlyMyRequests,
            string? status,
            string? priority,
            string? module,
            string? search,
            int currentUserId,
            bool isSuperAdmin,
            int page,
            int pageSize)
        {
            var queryable = _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.DeletedFlag == 1);

            if (!isSuperAdmin || onlyMyRequests)
            {
                queryable = queryable.Where(r => r.UserId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                queryable = queryable.Where(r => r.Status.ToLower() == status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(priority) && priority != "all")
            {
                queryable = queryable.Where(r => r.Priority.ToLower() == priority.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(module) && module != "all")
            {
                queryable = queryable.Where(r => r.Module != null && r.Module.ToLower() == module.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                queryable = queryable.Where(r =>
                    r.UserName.ToLower().Contains(s) ||
                    r.UserEmail.ToLower().Contains(s) ||
                    r.PermissionName.ToLower().Contains(s) ||
                    r.PermissionKey.ToLower().Contains(s) ||
                    (r.DepartmentName != null && r.DepartmentName.ToLower().Contains(s)) ||
                    (r.RoleName != null && r.RoleName.ToLower().Contains(s)) ||
                    r.Reason.ToLower().Contains(s));
            }

            var totalCount = await queryable.CountAsync();
            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 10;

            var items = await queryable
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNum - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, int MyPendingRequests)> GetSummaryCountsAsync(
            int currentUserId,
            bool isSuperAdmin)
        {
            var queryable = _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.DeletedFlag == 1);

            var all = await queryable.ToListAsync();

            var total = isSuperAdmin ? all.Count : all.Count(r => r.UserId == currentUserId);
            var pending = isSuperAdmin ? all.Count(r => r.Status == "Pending") : all.Count(r => r.UserId == currentUserId && r.Status == "Pending");
            var approved = isSuperAdmin ? all.Count(r => r.Status == "Approved") : all.Count(r => r.UserId == currentUserId && r.Status == "Approved");
            var rejected = isSuperAdmin ? all.Count(r => r.Status == "Rejected") : all.Count(r => r.UserId == currentUserId && r.Status == "Rejected");
            var myPending = all.Count(r => r.UserId == currentUserId && r.Status == "Pending");

            return (total, pending, approved, rejected, myPending);
        }

        public async Task<AccessRequest?> GetRequestByIdAsync(int id)
        {
            return await _context.AccessRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.DeletedFlag == 1);
        }

        public async Task<bool> ApproveRequestAsync(int requestId, int reviewerId, string reviewerName, string? comments)
        {
            var request = await _context.AccessRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);
            if (request == null || request.Status != "Pending") return false;

            request.Status = "Approved";
            request.ReviewerId = reviewerId;
            request.ReviewerName = reviewerName;
            request.ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionKey.ToLower() == request.PermissionKey.ToLower() && p.DeletedFlag == 1);

            if (permission != null)
            {
                var alreadyAssigned = await _context.UserPermissions
                    .AnyAsync(up => up.UserId == request.UserId && up.PermissionId == permission.Id);

                if (!alreadyAssigned)
                {
                    _context.UserPermissions.Add(new UserPermission
                    {
                        UserId = request.UserId,
                        PermissionId = permission.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "AccessRequest.Approve",
                    Module = "Access Requests",
                    PerformedBy = reviewerName,
                    Details = $"Granted permission '{request.PermissionKey}' ({request.PermissionName}) to user #{request.UserId} ({request.UserName}). Notes: {comments ?? "None"}",
                    IpAddress = "127.0.0.1",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectRequestAsync(int requestId, int reviewerId, string reviewerName, string? comments)
        {
            var request = await _context.AccessRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);
            if (request == null || request.Status != "Pending") return false;

            request.Status = "Rejected";
            request.ReviewerId = reviewerId;
            request.ReviewerName = reviewerName;
            request.ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "AccessRequest.Reject",
                    Module = "Access Requests",
                    PerformedBy = reviewerName,
                    Details = $"Rejected permission request for '{request.PermissionKey}' by user #{request.UserId} ({request.UserName}). Reason: {comments ?? "No comments"}",
                    IpAddress = "127.0.0.1",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedFlag = 1
                });
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteRequestAsync(int requestId, int currentUserId, bool isSuperAdmin)
        {
            var request = await _context.AccessRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);
            if (request == null) return false;

            if (!isSuperAdmin)
            {
                if (request.UserId != currentUserId || request.Status != "Pending")
                    return false;
            }

            request.DeletedFlag = 0;
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
