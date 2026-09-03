using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        Task<UserSession> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null);

        Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null);

        Task<List<UserSession>> GetUserSessionsAsync(int userId, int limit = 50);

        Task<List<UserSession>> GetAllRecentSessionsAsync(int limit = 100);

        Task<List<UserSession>> GetActiveSessionsAsync();

        Task<(List<UserSession> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize);

        Task<bool> TerminateSessionAsync(int sessionId);

        Task<int> TerminateAllUserSessionsAsync(int userId);

        Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync();

        Task<UserSession?> GetSessionByIdAsync(int sessionId);

        Task<List<UserSession>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null);

        Task<List<UserSession>> GetActiveSessionsForEmailAsync(string email);

        Task<UserSession?> FindActiveSessionByTokenAsync(int userId, string token);

        Task TouchSessionAsync(int sessionId, string clientIp);

        Task<int> GetActiveSessionsCountAsync();

        Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId);

        Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId);

        Task AddSessionAsync(UserSession session);
    }
}
