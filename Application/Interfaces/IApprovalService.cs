using System.Threading.Tasks;
using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Contract for managing employee approval requests, manager reviews, and status lifecycle.
    /// </summary>
    public interface IApprovalService
    {
        /// <summary>
        /// Retrieves paginated approval requests based on user role, filters, and search query.
        /// </summary>
        Task<PagedApprovalResponse> GetApprovalsAsync(ApprovalQueryParameters query, int currentUserId, bool isManagerOrAdmin);

        /// <summary>
        /// Retrieves high-level KPI summary metrics for approvals.
        /// </summary>
        Task<ApprovalSummaryDto> GetSummaryAsync(int currentUserId, bool isManagerOrAdmin);

        /// <summary>
        /// Retrieves a single approval request by its unique identifier.
        /// </summary>
        Task<ApprovalRequestDto?> GetApprovalByIdAsync(int id);

        /// <summary>
        /// Creates/raises a new approval request for an employee.
        /// </summary>
        Task<ApprovalRequestDto> CreateApprovalAsync(CreateApprovalRequest request, int userId, string userName, string userEmail, string? departmentName);

        /// <summary>
        /// Processes a manager's decision (Approve / Reject) on an employee's request with optional comments.
        /// </summary>
        Task<ApprovalRequestDto?> ProcessActionAsync(int id, ApprovalActionRequest request, int reviewerId, string reviewerName);

        /// <summary>
        /// Cancels or soft-deletes a pending approval request.
        /// </summary>
        Task<bool> DeleteApprovalAsync(int id, int currentUserId, bool isManagerOrAdmin);
    }
}
