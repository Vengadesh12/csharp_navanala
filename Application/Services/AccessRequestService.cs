using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class AccessRequestService : IAccessRequestService
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public AccessRequestService(IApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AvailablePermissionDto>> GetAvailablePermissionsAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.DeletedFlag == 1);
            if (user == null) return [];

            var userPerms = await _unitOfWork.Users.GetUserPermissionKeysAsync(userId);
            var userPermSet = new HashSet<string>(userPerms, StringComparer.OrdinalIgnoreCase);

            var pendingKeys = await _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.Status == "Pending" && r.DeletedFlag == 1)
                .Select(r => r.PermissionKey)
                .ToListAsync();
            var pendingKeySet = new HashSet<string>(pendingKeys, StringComparer.OrdinalIgnoreCase);

            var allPermissions = await _context.Permissions
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return allPermissions.Select(p => new AvailablePermissionDto
            {
                Id = p.Id,
                PermissionKey = p.PermissionKey,
                Name = p.Name,
                Description = p.Description,
                Module = InferModule(p.PermissionKey),
                IsGranted = user.RoleId == 2 || userPermSet.Contains(p.PermissionKey),
                HasPendingRequest = pendingKeySet.Contains(p.PermissionKey)
            }).ToList();
        }

        public async Task<List<AccessRequestDto>> GetMyRequestsAsync(int userId)
        {
            var requests = await _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.DeletedFlag == 1)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto).ToList();
        }

        public async Task<AccessRequestDto> CreateRequestAsync(int userId, CreateAccessRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PermissionKey))
                throw new ArgumentException("Permission key is required.");
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Please provide a business justification / reason for your request.");

            var permKey = dto.PermissionKey.Trim();

            // Verify permission exists
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionKey.ToLower() == permKey.ToLower() && p.DeletedFlag == 1);

            if (permission == null)
                throw new ArgumentException($"System permission '{permKey}' was not found.");

            // Verify user exists and doesn't already have the permission
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedFlag == 1);

            if (user == null)
                throw new ArgumentException("User not found.");

            if (user.RoleId == 2)
                throw new InvalidOperationException("Super Admin already possesses all system permissions.");

            var currentPerms = await _unitOfWork.Users.GetUserPermissionKeysAsync(userId);
            if (currentPerms.Contains(permKey, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"You already have the '{permission.Name}' permission active.");

            // Check if active pending request already exists
            var existingPending = await _context.AccessRequests
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.PermissionKey.ToLower() == permKey.ToLower() && r.Status == "Pending" && r.DeletedFlag == 1);

            if (existingPending)
                throw new InvalidOperationException($"You already have a pending access request for '{permission.Name}'.");

            // Look up Department & Role names
            string? deptName = null;
            if (user.DesignationId.HasValue)
            {
                deptName = await (from des in _context.Designations
                                  join d in _context.Departments on des.DepartmentId equals d.Id
                                  where des.Id == user.DesignationId.Value
                                  select d.Name).FirstOrDefaultAsync();
            }

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _context.Roles
                    .Where(r => r.Id == user.RoleId.Value)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
            }

            var module = InferModule(permKey);
            var entity = AccessRequest.Create(
                userId,
                user.Name,
                user.Email,
                deptName,
                roleName,
                permKey,
                permission.Name,
                module,
                dto.Reason,
                dto.Priority);

            _context.AccessRequests.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<PagedAccessRequestResponse> GetRequestsAsync(AccessRequestQueryParameters query, int currentUserId, bool isSuperAdmin)
        {
            var queryable = _context.AccessRequests
                .AsNoTracking()
                .Where(r => r.DeletedFlag == 1);

            if (!isSuperAdmin || query.OnlyMyRequests)
            {
                queryable = queryable.Where(r => r.UserId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "all")
            {
                queryable = queryable.Where(r => r.Status.ToLower() == query.Status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Priority) && query.Priority != "all")
            {
                queryable = queryable.Where(r => r.Priority.ToLower() == query.Priority.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Module) && query.Module != "all")
            {
                queryable = queryable.Where(r => r.Module != null && r.Module.ToLower() == query.Module.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                queryable = queryable.Where(r =>
                    r.UserName.ToLower().Contains(search) ||
                    r.UserEmail.ToLower().Contains(search) ||
                    r.PermissionName.ToLower().Contains(search) ||
                    r.PermissionKey.ToLower().Contains(search) ||
                    (r.DepartmentName != null && r.DepartmentName.ToLower().Contains(search)) ||
                    (r.RoleName != null && r.RoleName.ToLower().Contains(search)) ||
                    r.Reason.ToLower().Contains(search));
            }

            var totalCount = await queryable.CountAsync();
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await queryable
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedAccessRequestResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AccessRequestSummaryDto> GetSummaryAsync(int currentUserId, bool isSuperAdmin)
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

            return new AccessRequestSummaryDto
            {
                TotalRequests = total,
                PendingRequests = pending,
                ApprovedRequests = approved,
                RejectedRequests = rejected,
                MyPendingRequests = myPending
            };
        }

        public async Task<AccessRequestDto?> GetRequestByIdAsync(int requestId)
        {
            var request = await _context.AccessRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);

            return request == null ? null : MapToDto(request);
        }

        public async Task<bool> ApproveRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto)
        {
            var request = await _context.AccessRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);

            if (request == null || request.Status != "Pending") return false;

            // Mark request approved
            request.Approve(reviewerId, reviewerName, dto.Comments);

            // Find matching permission to assign
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionKey.ToLower() == request.PermissionKey.ToLower() && p.DeletedFlag == 1);

            if (permission != null)
            {
                var alreadyAssigned = await _context.UserPermissions
                    .AnyAsync(up => up.UserId == request.UserId && up.PermissionId == permission.Id);

                if (!alreadyAssigned)
                {
                    _context.UserPermissions.Add(UserPermission.Create(request.UserId, permission.Id));
                }
            }

            // Create Audit Log
            _context.AuditLogs.Add(AuditLog.CreateLog(
                action: "AccessRequest.Approve",
                module: "Access Requests",
                performedBy: reviewerName,
                details: $"Granted permission '{request.PermissionKey}' ({request.PermissionName}) to user #{request.UserId} ({request.UserName}). Notes: {dto.Comments ?? "None"}",
                ipAddress: "127.0.0.1",
                status: "Success"
            ));

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto)
        {
            var request = await _context.AccessRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);

            if (request == null || request.Status != "Pending") return false;

            request.Reject(reviewerId, reviewerName, dto.Comments);

            // Create Audit Log
            _context.AuditLogs.Add(AuditLog.CreateLog(
                action: "AccessRequest.Reject",
                module: "Access Requests",
                performedBy: reviewerName,
                details: $"Rejected permission request for '{request.PermissionKey}' by user #{request.UserId} ({request.UserName}). Reason: {dto.Comments ?? "No comments"}",
                ipAddress: "127.0.0.1",
                status: "Success"
            ));

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRequestAsync(int requestId, int currentUserId, bool isSuperAdmin)
        {
            var request = await _context.AccessRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.DeletedFlag == 1);

            if (request == null) return false;

            // Non-admin can only delete/cancel their own request while still pending
            if (!isSuperAdmin)
            {
                if (request.UserId != currentUserId || request.Status != "Pending")
                    return false;
            }

            request.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }

        private static AccessRequestDto MapToDto(AccessRequest r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.UserName,
            UserEmail = r.UserEmail,
            DepartmentName = r.DepartmentName,
            RoleName = r.RoleName,
            PermissionKey = r.PermissionKey,
            PermissionName = r.PermissionName,
            Module = r.Module,
            Reason = r.Reason,
            Priority = r.Priority,
            Status = r.Status,
            ReviewerId = r.ReviewerId,
            ReviewerName = r.ReviewerName,
            ReviewerComments = r.ReviewerComments,
            ReviewedAt = r.ReviewedAt,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            DeletedFlag = r.DeletedFlag
        };

        private static string InferModule(string permissionKey)
        {
            var key = permissionKey.ToLowerInvariant();
            if (key.StartsWith("users.")) return "User Directory";
            if (key.StartsWith("roles.")) return "Roles Management";
            if (key.StartsWith("departments.")) return "Departments";
            if (key.StartsWith("permissions.")) return "Permissions Matrix";
            if (key.StartsWith("invoices.")) return "Invoice & Billing";
            if (key.StartsWith("purchases.")) return "Purchases & Procurement";
            if (key.StartsWith("approvals.")) return "Product Approvals";
            if (key.StartsWith("user_activity.")) return "User Activity & Sessions";
            if (key.StartsWith("audit.")) return "Audit Logs";
            if (key.StartsWith("reports.")) return "Reports & Exports";
            if (key.StartsWith("projects.")) return "Projects Management";
            if (key.StartsWith("calendar.")) return "Schedule & Calendar";
            if (key.StartsWith("settings.")) return "System Settings";
            if (key.StartsWith("dashboard.")) return "Dashboard & Core";
            return "General System";
        }
    }
}
