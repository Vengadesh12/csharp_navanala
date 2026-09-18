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
    public class AccessRequestRepository : IAccessRequestRepository
    {
        private readonly AppDbContext _context;

        public AccessRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetPendingKeysForUserAsync(int userId)
        {
            var sql = new StringBuilder("""
                SELECT permission_key AS "Value"
                FROM access_requests
                WHERE user_id = {0} AND status = 'Pending' AND deleted_flag = 1
            """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), userId).ToListAsync();
        }

        public async Task<List<PermissionModel>> GetAllActivePermissionsAsync()
        {
            var sql = new StringBuilder("""
                SELECT "Id", "PermissionKey", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM permissions
                WHERE "DeletedFlag" = 1
                ORDER BY "Id"
            """);

            return await _context.Permissions
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AccessRequestModel>> GetRequestsForUserAsync(int userId)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, user_name, user_email, department_name, role_name, permission_key, permission_name, module, reason, priority, status, reviewer_id, reviewer_name, reviewer_comments, reviewed_at, deleted_flag, created_at, updated_at
                FROM access_requests
                WHERE user_id = {0} AND deleted_flag = 1
                ORDER BY created_at DESC
            """);

            return await _context.AccessRequests
                .FromSqlRaw(sql.ToString(), userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PermissionModel?> GetPermissionByKeyAsync(string permKey)
        {
            var sql = new StringBuilder("""
                SELECT "Id", "PermissionKey", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                FROM permissions
                WHERE LOWER("PermissionKey") = LOWER({0}) AND "DeletedFlag" = 1
                LIMIT 1
            """);

            return await _context.Permissions
                .FromSqlRaw(sql.ToString(), permKey.Trim())
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingRequestAsync(int userId, string permKey)
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM access_requests
                WHERE user_id = {0} AND LOWER(permission_key) = LOWER({1}) AND status = 'Pending' AND deleted_flag = 1
            """);

            var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), userId, permKey.Trim()).SingleOrDefaultAsync();
            return count > 0;
        }

        public async Task<string?> GetDepartmentNameForDesignationAsync(int designationId)
        {
            var sql = new StringBuilder("""
                SELECT d."Name" AS "Value"
                FROM designations des
                INNER JOIN departments d ON des."DepartmentId" = d."Id"
                WHERE des."Id" = {0} AND des."DeletedFlag" = 1 AND d."DeletedFlag" = 1
                LIMIT 1
            """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), designationId).FirstOrDefaultAsync();
        }

        public async Task<AccessRequestModel> AddRequestAsync(AccessRequestModel request)
        {
            _context.AccessRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<(List<AccessRequestModel> Items, int TotalCount)> GetPagedRequestsAsync(
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
            var sql = new StringBuilder("""
                SELECT id, user_id, user_name, user_email, department_name, role_name, permission_key, permission_name, module, reason, priority, status, reviewer_id, reviewer_name, reviewer_comments, reviewed_at, deleted_flag, created_at, updated_at
                FROM access_requests
                WHERE deleted_flag = 1
            """);

            var countSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM access_requests
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!isSuperAdmin || onlyMyRequests)
            {
                var clause = $" AND user_id = {{{paramIndex++}}}";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                var clause = $" AND LOWER(status) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(priority) && priority != "all")
            {
                var clause = $" AND LOWER(priority) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(priority.Trim());
            }

            if (!string.IsNullOrWhiteSpace(module) && module != "all")
            {
                var clause = $" AND module IS NOT NULL AND LOWER(module) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(module.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                var clause = $" AND (LOWER(user_name) LIKE {{{paramIndex}}} OR LOWER(user_email) LIKE {{{paramIndex}}} OR LOWER(permission_name) LIKE {{{paramIndex}}} OR LOWER(permission_key) LIKE {{{paramIndex}}} OR (department_name IS NOT NULL AND LOWER(department_name) LIKE {{{paramIndex}}}) OR (role_name IS NOT NULL AND LOWER(role_name) LIKE {{{paramIndex}}}) OR LOWER(reason) LIKE {{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(pattern);
            }

            var totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql.ToString(), parameters.ToArray())
                .SingleOrDefaultAsync();

            sql.Append(" ORDER BY created_at DESC");

            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 10;
            var offset = (pageNum - 1) * size;

            sql.Append($" LIMIT {{{paramIndex++}}} OFFSET {{{paramIndex++}}}");
            parameters.Add(size);
            parameters.Add(offset);

            var items = await _context.AccessRequests
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, int MyPendingRequests)> GetSummaryCountsAsync(
            int currentUserId,
            bool isSuperAdmin)
        {
            var totalSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM access_requests WHERE deleted_flag = 1");
            var pendingSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM access_requests WHERE deleted_flag = 1 AND status = 'Pending'");
            var approvedSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM access_requests WHERE deleted_flag = 1 AND status = 'Approved'");
            var rejectedSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM access_requests WHERE deleted_flag = 1 AND status = 'Rejected'");
            var myPendingSql = new StringBuilder("SELECT CAST(COUNT(*) AS INTEGER) AS \"Value\" FROM access_requests WHERE deleted_flag = 1 AND user_id = {0} AND status = 'Pending'");

            if (!isSuperAdmin)
            {
                totalSql.Append(" AND user_id = {0}");
                pendingSql.Append(" AND user_id = {0}");
                approvedSql.Append(" AND user_id = {0}");
                rejectedSql.Append(" AND user_id = {0}");
            }

            int total, pending, approved, rejected;
            if (!isSuperAdmin)
            {
                total = await _context.Database.SqlQueryRaw<int>(totalSql.ToString(), currentUserId).SingleOrDefaultAsync();
                pending = await _context.Database.SqlQueryRaw<int>(pendingSql.ToString(), currentUserId).SingleOrDefaultAsync();
                approved = await _context.Database.SqlQueryRaw<int>(approvedSql.ToString(), currentUserId).SingleOrDefaultAsync();
                rejected = await _context.Database.SqlQueryRaw<int>(rejectedSql.ToString(), currentUserId).SingleOrDefaultAsync();
            }
            else
            {
                total = await _context.Database.SqlQueryRaw<int>(totalSql.ToString()).SingleOrDefaultAsync();
                pending = await _context.Database.SqlQueryRaw<int>(pendingSql.ToString()).SingleOrDefaultAsync();
                approved = await _context.Database.SqlQueryRaw<int>(approvedSql.ToString()).SingleOrDefaultAsync();
                rejected = await _context.Database.SqlQueryRaw<int>(rejectedSql.ToString()).SingleOrDefaultAsync();
            }

            var myPending = await _context.Database.SqlQueryRaw<int>(myPendingSql.ToString(), currentUserId).SingleOrDefaultAsync();

            return (total, pending, approved, rejected, myPending);
        }

        public async Task<AccessRequestModel?> GetRequestByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, user_id, user_name, user_email, department_name, role_name, permission_key, permission_name, module, reason, priority, status, reviewer_id, reviewer_name, reviewer_comments, reviewed_at, deleted_flag, created_at, updated_at
                FROM access_requests
                WHERE id = {0} AND deleted_flag = 1
                LIMIT 1
            """);

            return await _context.AccessRequests
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
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
                    _context.UserPermissions.Add(new UserPermissionModel
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
                _context.AuditLogs.Add(new AuditLogModel
                {
                    Action = "AccessRequestModel.Approve",
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
                _context.AuditLogs.Add(new AuditLogModel
                {
                    Action = "AccessRequestModel.Reject",
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
