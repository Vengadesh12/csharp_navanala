using System;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly IApprovalRepository _approvalRepository;

        public ApprovalService(IApprovalRepository approvalRepository)
        {
            _approvalRepository = approvalRepository;
        }

        public async Task<PagedApprovalResponse> GetApprovalsAsync(ApprovalQueryParameters query, int currentUserId, bool isManagerOrAdmin)
        {
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var (items, totalCount) = await _approvalRepository.GetApprovalsPagedAsync(
                currentUserId,
                isManagerOrAdmin,
                query.Scope,
                query.Status,
                query.Category,
                query.Priority,
                query.Search,
                page,
                pageSize);

            var summary = await GetSummaryAsync(currentUserId, isManagerOrAdmin);

            return new PagedApprovalResponse
            {
                Items = items.Select(a => a.ToDto()).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Summary = summary
            };
        }

        public async Task<ApprovalSummaryDto> GetSummaryAsync(int currentUserId, bool isManagerOrAdmin)
        {
            var (total, pending, approved, rejected, myRequests) =
                await _approvalRepository.GetSummaryAsync(currentUserId, isManagerOrAdmin);

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
            var entity = await _approvalRepository.GetByIdAsync(id);
            return entity?.ToDto();
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

            await _approvalRepository.AddApprovalAsync(entity);
            return entity.ToDto();
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

            var entity = await _approvalRepository.GetByIdAsync(id);
            if (entity == null) return null;

            if (actionLower == "approve")
            {
                entity.Approve(reviewerId, reviewerName, request.Comments);
            }
            else
            {
                entity.Reject(reviewerId, reviewerName, request.Comments);
            }

            await _approvalRepository.UpdateApprovalAsync(entity);
            return entity.ToDto();
        }

        public async Task<bool> DeleteApprovalAsync(int id, int currentUserId, bool isManagerOrAdmin)
        {
            var entity = await _approvalRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (!isManagerOrAdmin && (entity.UserId != currentUserId || !entity.IsPending()))
            {
                return false;
            }

            return await _approvalRepository.SoftDeleteApprovalAsync(id);
        }
    }
}
