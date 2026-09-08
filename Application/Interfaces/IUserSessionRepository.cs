using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IUserSessionRepository : IRepository<UserSessionModel>
    {
        Task<UserSessionModel> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null);

        Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null);

        Task<List<UserSessionModel>> GetUserSessionsAsync(int userId, int limit = 50);

        Task<List<UserSessionModel>> GetAllRecentSessionsAsync(int limit = 100);

        Task<List<UserSessionModel>> GetActiveSessionsAsync();

        Task<(List<UserSessionModel> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize);

        Task<bool> TerminateSessionAsync(int sessionId);

        Task<int> TerminateAllUserSessionsAsync(int userId);

        Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync();

        Task<UserSessionModel?> GetSessionByIdAsync(int sessionId);

        Task<List<UserSessionModel>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null);

        Task<List<UserSessionModel>> GetActiveSessionsForEmailAsync(string email);

        Task<UserSessionModel?> FindActiveSessionByTokenAsync(int userId, string token);

        Task TouchSessionAsync(int sessionId, string clientIp);

        Task<int> GetActiveSessionsCountAsync();

        Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId);

        Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId);

        Task AddSessionAsync(UserSessionModel session);
    }
}
