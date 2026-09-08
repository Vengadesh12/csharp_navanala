using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IApprovalRepository
    {
        Task<(List<ApprovalRequestModel> Items, int TotalCount)> GetApprovalsPagedAsync(
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

        Task<ApprovalRequestModel?> GetByIdAsync(int id);

        Task<ApprovalRequestModel> AddApprovalAsync(ApprovalRequestModel approval);

        Task UpdateApprovalAsync(ApprovalRequestModel approval);

        Task<bool> SoftDeleteApprovalAsync(int id);

        Task<List<ApprovalRequestModel>> GetApprovedApprovalsAsync();
    }
}
