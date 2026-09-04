using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IAccessRequestRepository
    {
        Task<List<string>> GetPendingKeysForUserAsync(int userId);

        Task<List<Permission>> GetAllActivePermissionsAsync();

        Task<List<AccessRequest>> GetRequestsForUserAsync(int userId);

        Task<Permission?> GetPermissionByKeyAsync(string permKey);

        Task<bool> HasPendingRequestAsync(int userId, string permKey);

        Task<string?> GetDepartmentNameForDesignationAsync(int designationId);

        Task<AccessRequest> AddRequestAsync(AccessRequest request);

        Task<(List<AccessRequest> Items, int TotalCount)> GetPagedRequestsAsync(
            bool onlyMyRequests,
            string? status,
            string? priority,
            string? module,
            string? search,
            int currentUserId,
            bool isSuperAdmin,
            int page,
            int pageSize);

        Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, int MyPendingRequests)> GetSummaryCountsAsync(
            int currentUserId,
            bool isSuperAdmin);

        Task<AccessRequest?> GetRequestByIdAsync(int id);

        Task<bool> ApproveRequestAsync(int requestId, int reviewerId, string reviewerName, string? comments);

        Task<bool> RejectRequestAsync(int requestId, int reviewerId, string reviewerName, string? comments);

        Task<bool> SoftDeleteRequestAsync(int requestId, int currentUserId, bool isSuperAdmin);
    }
}
