using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IApprovalService
    {
        Task<PagedApprovalResponse> GetApprovalsAsync(ApprovalQueryParameters query, int currentUserId, bool isManagerOrAdmin);

        Task<ApprovalSummaryDto> GetSummaryAsync(int currentUserId, bool isManagerOrAdmin);

        Task<ApprovalRequestDto?> GetApprovalByIdAsync(int id);

        Task<ApprovalRequestDto> CreateApprovalAsync(CreateApprovalRequest request, int userId, string userName, string userEmail, string? departmentName);

        Task<ApprovalRequestDto?> ProcessActionAsync(int id, ApprovalActionRequest request, int reviewerId, string reviewerName);

        Task<bool> DeleteApprovalAsync(int id, int currentUserId, bool isManagerOrAdmin);
    }
}
