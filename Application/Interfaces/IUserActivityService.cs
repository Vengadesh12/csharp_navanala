using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IUserActivityService
    {
        Task<UserActivitySummaryDto> GetSummaryAsync();

        Task<PagedUserActivityResponse> GetPagedActivitiesAsync(UserActivityQueryParameters query);

        Task<List<UserSessionItemDto>> GetActiveUsersAsync();

        Task<UserSessionDto?> GetSessionByIdAsync(int sessionId);

        Task<bool> TerminateSessionAsync(int sessionId, int adminUserId);

        Task<int> ForceLogoutUserAsync(int targetUserId, int adminUserId);
    }
}
