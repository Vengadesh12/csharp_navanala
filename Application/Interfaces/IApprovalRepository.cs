using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IApprovalRepository
    {
        Task<(List<ApprovalRequest> Items, int TotalCount)> GetApprovalsPagedAsync(
            int currentUserId,
            bool isManagerOrAdmin,
            string? scope,
            string? status,
            string? category,
            string? priority,
            string? search,
            int page,
            int pageSize);

        Task<(int TotalRequests, int PendingCount, int ApprovedCount, int RejectedCount, int MyRequestsCount)> GetSummaryAsync(
            int currentUserId,
            bool isManagerOrAdmin);

        Task<ApprovalRequest?> GetByIdAsync(int id);

        Task<ApprovalRequest> AddApprovalAsync(ApprovalRequest approval);

        Task UpdateApprovalAsync(ApprovalRequest approval);

        Task<bool> SoftDeleteApprovalAsync(int id);

        Task<List<ApprovalRequest>> GetApprovedApprovalsAsync();
    }
}
