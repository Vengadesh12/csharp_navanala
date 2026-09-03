using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IAccessRequestService
    {
        Task<List<AvailablePermissionDto>> GetAvailablePermissionsAsync(int userId);
        Task<List<AccessRequestDto>> GetMyRequestsAsync(int userId);
        Task<AccessRequestDto> CreateRequestAsync(int userId, CreateAccessRequestDto dto);
        Task<PagedAccessRequestResponse> GetRequestsAsync(AccessRequestQueryParameters query, int currentUserId, bool isSuperAdmin);
        Task<AccessRequestSummaryDto> GetSummaryAsync(int currentUserId, bool isSuperAdmin);
        Task<AccessRequestDto?> GetRequestByIdAsync(int requestId);
        Task<bool> ApproveRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto);
        Task<bool> RejectRequestAsync(int requestId, int reviewerId, string reviewerName, ReviewAccessRequestDto dto);
        Task<bool> DeleteRequestAsync(int requestId, int currentUserId, bool isSuperAdmin);
    }
}
