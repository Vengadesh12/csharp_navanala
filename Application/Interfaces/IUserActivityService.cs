using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Contract for managing user login/logout activity monitoring and active online sessions.
    /// </summary>
    public interface IUserActivityService
    {
        /// <summary>
        /// Retrieves high-level dashboard metrics and active sessions.
        /// </summary>
        Task<UserActivitySummaryDto> GetSummaryAsync();

        /// <summary>
        /// Retrieves paginated login/logout activity records with filters.
        /// </summary>
        Task<PagedUserActivityResponse> GetPagedActivitiesAsync(UserActivityQueryParameters query);

        /// <summary>
        /// Retrieves currently active/logged-in users.
        /// </summary>
        Task<List<UserSessionItemDto>> GetActiveUsersAsync();

        /// <summary>
        /// Retrieves an active session by session ID.
        /// </summary>
        Task<UserSessionDto?> GetSessionByIdAsync(int sessionId);

        /// <summary>
        /// Terminates a specific session by ID.
        /// </summary>
        Task<bool> TerminateSessionAsync(int sessionId, int adminUserId);

        /// <summary>
        /// Force-logs out a user across all active sessions.
        /// </summary>
        Task<int> ForceLogoutUserAsync(int targetUserId, int adminUserId);
    }
}
