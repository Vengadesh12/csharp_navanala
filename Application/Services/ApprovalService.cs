using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly IApplicationDbContext _context;

        public ApprovalService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedApprovalResponse> GetApprovalsAsync(ApprovalQueryParameters query, int currentUserId, bool isManagerOrAdmin)
        {
            IQueryable<ApprovalRequest> queryable = _context.Approvals
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1);

            // Scope filter: Employees only see their own; Managers see all (or their own if requested)
            if (!isManagerOrAdmin || string.Equals(query.Scope, "my", StringComparison.OrdinalIgnoreCase))
            {
                queryable = queryable.Where(a => a.UserId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetStatus = query.Status.Trim();
                queryable = queryable.Where(a => a.Status.ToLower() == targetStatus.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Category) && !string.Equals(query.Category, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetCategory = query.Category.Trim();
                queryable = queryable.Where(a => a.Category.ToLower() == targetCategory.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Priority) && !string.Equals(query.Priority, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                var targetPriority = query.Priority.Trim();
                queryable = queryable.Where(a => a.Priority.ToLower() == targetPriority.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                queryable = queryable.Where(a =>
                    a.EmployeeName.ToLower().Contains(search) ||
                    a.EmployeeEmail.ToLower().Contains(search) ||
                    a.ItemName.ToLower().Contains(search) ||
                    a.Description.ToLower().Contains(search) ||
                    (a.DepartmentName != null && a.DepartmentName.ToLower().Contains(search)) ||
                    (a.Comments != null && a.Comments.ToLower().Contains(search))
                );
            }

            queryable = queryable.OrderByDescending(a => a.Id);

            var totalCount = await queryable.CountAsync();
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var items = await queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var summary = await GetSummaryAsync(currentUserId, isManagerOrAdmin);

            return new PagedApprovalResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Summary = summary
            };
        }

        public async Task<ApprovalSummaryDto> GetSummaryAsync(int currentUserId, bool isManagerOrAdmin)
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

            return new ApprovalSummaryDto
            {
                TotalRequests = total,
                PendingCount = pending,
                ApprovedCount = approved,
                RejectedCount = rejected,
                MyRequestsCount = myRequests
            };
        }

        public async Task<ApprovalRequestDto?> GetApprovalByIdAsync(int id)
        {
            var entity = await _context.Approvals
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedFlag == 1);

            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<ApprovalRequestDto> CreateApprovalAsync(
            CreateApprovalRequest request,
            int userId,
            string userName,
            string userEmail,
            string? departmentName)
        {
            if (string.IsNullOrWhiteSpace(request.ItemName))
            {
                throw new BadRequestException("Product/Item name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new BadRequestException("Reason / justification is required.");
            }

            var entity = ApprovalRequest.Create(
                userId: userId,
                userName: userName,
                userEmail: userEmail,
                departmentName: departmentName,
                itemName: request.ItemName,
                category: request.Category,
                description: request.Description,
                quantity: request.Quantity,
                priority: request.Priority,
                estimatedAmount: request.EstimatedAmount
            );

            _context.Approvals.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<ApprovalRequestDto?> ProcessActionAsync(
            int id,
            ApprovalActionRequest request,
            int reviewerId,
            string reviewerName)
        {
            var actionLower = (request.Action ?? "").Trim().ToLowerInvariant();
            if (actionLower != "approve" && actionLower != "reject")
            {
                throw new BadRequestException("Action must be either 'Approve' or 'Reject'.");
            }

            var entity = await _context.Approvals
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedFlag == 1);

            if (entity == null) return null;

            if (actionLower == "approve")
            {
                entity.Approve(reviewerId, reviewerName, request.Comments);
            }
            else
            {
                entity.Reject(reviewerId, reviewerName, request.Comments);
            }

            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> DeleteApprovalAsync(int id, int currentUserId, bool isManagerOrAdmin)
        {
            var entity = await _context.Approvals
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedFlag == 1);

            if (entity == null) return false;

            // Employees can only cancel their own pending requests
            if (!isManagerOrAdmin && (entity.UserId != currentUserId || !entity.IsPending()))
            {
                return false;
            }

            entity.SoftDelete();

            await _context.SaveChangesAsync();
            return true;
        }

        private static ApprovalRequestDto MapToDto(ApprovalRequest e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            EmployeeName = e.EmployeeName,
            EmployeeEmail = e.EmployeeEmail,
            DepartmentName = e.DepartmentName,
            ItemName = e.ItemName,
            Category = e.Category,
            Description = e.Description,
            Quantity = e.Quantity,
            Priority = e.Priority,
            EstimatedAmount = e.EstimatedAmount,
            Status = e.Status,
            Comments = e.Comments,
            ReviewedById = e.ReviewedById,
            ReviewedByName = e.ReviewedByName,
            ReviewedAt = e.ReviewedAt,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            DeletedFlag = e.DeletedFlag
        };
    }
}
