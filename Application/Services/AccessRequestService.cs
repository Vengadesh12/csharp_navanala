using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class AccessRequestService : IAccessRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccessRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AvailablePermissionDto>> GetAvailablePermissionsAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1) return [];

            var userPermissionKeys = (await _unitOfWork.Users.GetUserPermissionKeysAsync(userId))
                .Select(k => k.ToLowerInvariant())
                .ToHashSet();

            var pendingKeys = (await _unitOfWork.AccessRequests.GetPendingKeysForUserAsync(userId))
                .Select(k => k.ToLowerInvariant())
                .ToHashSet();

            var allPermissions = await _unitOfWork.AccessRequests.GetAllActivePermissionsAsync();

            return allPermissions.Select(p =>
            {
                var pKeyLower = p.PermissionKey.ToLowerInvariant();
                var alreadyAssigned = userPermissionKeys.Contains(pKeyLower);
                var isPending = pendingKeys.Contains(pKeyLower);

                return new AvailablePermissionDto
                {
                    Id = p.Id,
                    PermissionKey = p.PermissionKey,
                    Name = p.Name,
                    Module = InferModule(p.PermissionKey),
                    Description = p.Description,
                    IsGranted = alreadyAssigned,
                    HasPendingRequest = isPending
                };
            }).ToList();
        }

        public async Task<List<AccessRequestDto>> GetMyRequestsAsync(int userId)
        {
            var requests = await _unitOfWork.AccessRequests.GetRequestsForUserAsync(userId);
            return requests.Select(MapToDto).ToList();
        }

        public async Task<AccessRequestDto> CreateRequestAsync(int userId, CreateAccessRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PermissionKey))
                throw new ArgumentException("Permission key is required.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("A reason for the access request must be provided.");

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
                throw new InvalidOperationException("User not found or deactivated.");

            var permKey = dto.PermissionKey.Trim();
            var permission = await _unitOfWork.AccessRequests.GetPermissionByKeyAsync(permKey);

            if (permission == null)
                throw new ArgumentException($"Permission with key '{permKey}' not found.");

            var userPermissionKeys = (await _unitOfWork.Users.GetUserPermissionKeysAsync(userId))
                .Select(k => k.ToLowerInvariant())
                .ToHashSet();

            if (userPermissionKeys.Contains(permKey.ToLower()))
                throw new InvalidOperationException($"You already have active access to '{permission.Name}'.");

            var existingPending = await _unitOfWork.AccessRequests.HasPendingRequestAsync(userId, permKey);
            if (existingPending)
                throw new InvalidOperationException($"You already have a pending access request for '{permission.Name}'.");

            string? deptName = null;
            if (user.DesignationId.HasValue)
            {
                deptName = await _unitOfWork.AccessRequests.GetDepartmentNameForDesignationAsync(user.DesignationId.Value);
            }

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _unitOfWork.Users.GetRoleNameByIdAsync(user.RoleId.Value);
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

            await _unitOfWork.AccessRequests.AddRequestAsync(entity);

            return MapToDto(entity);
        }

        public async Task<PagedAccessRequestResponse> GetRequestsAsync(AccessRequestQueryParameters query, int currentUserId, bool isSuperAdmin)
        {
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var (items, totalCount) = await _unitOfWork.AccessRequests.GetPagedRequestsAsync(
                query.OnlyMyRequests,
                query.Status,
                query.Priority,
                query.Module,
                query.Search,
                currentUserId,
                isSuperAdmin,
                page,
                pageSize);

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
            var (total, pending, approved, rejected, myPending) =
                await _unitOfWork.AccessRequests.GetSummaryCountsAsync(currentUserId, isSuperAdmin);

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
            var request = await _unitOfWork.AccessRequests.GetRequestByIdAsync(requestId);
            return request == null ? null : MapToDto(request);
        }

        public async Task<bool> ApproveRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto)
        {
            return await _unitOfWork.AccessRequests.ApproveRequestAsync(requestId, reviewerId, reviewerName, dto.Comments);
        }

        public async Task<bool> RejectRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto)
        {
            return await _unitOfWork.AccessRequests.RejectRequestAsync(requestId, reviewerId, reviewerName, dto.Comments);
        }

        public async Task<bool> DeleteRequestAsync(int requestId, int currentUserId, bool isSuperAdmin)
        {
            return await _unitOfWork.AccessRequests.SoftDeleteRequestAsync(requestId, currentUserId, isSuperAdmin);
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
