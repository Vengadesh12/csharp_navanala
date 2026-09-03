using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserSession> RecordLoginAsync(int userId, string email, string userName, string ipAddress, string? userAgent = null, string? sessionToken = null)
        {
            var session = new UserSession
            {
                UserId = userId,
                Email = email,
                UserName = userName,
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim(),
                UserAgent = userAgent,
                LoginTime = DateTime.UtcNow,
                LogoutTime = null,
                SessionToken = sessionToken,
                IsActive = true,
                DeletedFlag = 1
            };

            await _context.UserSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<bool> RecordLogoutAsync(int userId, string? ipAddress = null, string? sessionToken = null, string? email = null)
        {
            var now = DateTime.UtcNow;
            var clientIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim();

            // 1. If specific session token is provided, prioritize terminating that exact session
            List<UserSession> activeSessions = new();
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                activeSessions = await _context.UserSessions
                    .Where(s => s.DeletedFlag == 1 && s.SessionToken == sessionToken && (s.LogoutTime == null || s.IsActive))
                    .ToListAsync();
            }

            // 2. If no session was found by token, look up active sessions by userId or email
            if (activeSessions.Count == 0)
            {
                var query = _context.UserSessions
                    .Where(s => s.DeletedFlag == 1 && (s.LogoutTime == null || s.IsActive));

                if (userId > 0)
                {
                    query = query.Where(s => s.UserId == userId);
                }
                else if (!string.IsNullOrWhiteSpace(email))
                {
                    var normalizedEmail = email.Trim().ToLower();
                    query = query.Where(s => s.Email.ToLower() == normalizedEmail);
                }

                activeSessions = await query.ToListAsync();
            }

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.LogoutTime = now;
                    session.IsActive = false;
                    if (!string.IsNullOrWhiteSpace(ipAddress))
                    {
                        session.IpAddress = clientIp;
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }

            // Fallback: If no open session row existed, create a completed session row so history is preserved
            User? user = null;
            if (userId > 0)
            {
                user = await _context.Users.FindAsync(userId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }

            if (user is not null)
            {
                var auditSession = new UserSession
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.Name,
                    IpAddress = clientIp,
                    LoginTime = now.AddMinutes(-5),
                    LogoutTime = now,
                    SessionToken = sessionToken,
                    IsActive = false,
                    DeletedFlag = 1
                };
                await _context.UserSessions.AddAsync(auditSession);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<UserSession>> GetUserSessionsAsync(int userId, int limit = 50)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.DeletedFlag == 1)
                .OrderByDescending(s => s.LoginTime)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UserSession>> GetAllRecentSessionsAsync(int limit = 100)
        {
            return await _context.UserSessions
                .Where(s => s.DeletedFlag == 1)
                .OrderByDescending(s => s.LoginTime)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        private const int DefaultSessionExpiryMinutes = 120;

        public async Task<List<UserSession>> GetActiveSessionsAsync()
        {
            var expiryCutoff = DateTime.UtcNow.AddMinutes(-DefaultSessionExpiryMinutes);
            return await _context.UserSessions
                .Where(s => s.DeletedFlag == 1 && s.IsActive && s.LogoutTime == null && (s.UpdatedAt ?? s.LoginTime) >= expiryCutoff)
                .OrderByDescending(s => s.LoginTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<UserSession> Items, int TotalCount)> GetPagedSessionsAsync(string? search, string? status, int page, int pageSize)
        {
            var query = _context.UserSessions.Where(s => s.DeletedFlag == 1);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.UserName.ToLower().Contains(term)
                                      || s.Email.ToLower().Contains(term)
                                      || s.IpAddress.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLower = status.Trim().ToLower();
                if (statusLower == "active")
                {
                    query = query.Where(s => s.IsActive && s.LogoutTime == null);
                }
                else if (statusLower == "completed" || statusLower == "inactive" || statusLower == "loggedout")
                {
                    query = query.Where(s => !s.IsActive || s.LogoutTime != null);
                }
            }

            var totalCount = await query.CountAsync();
            var skip = Math.Max(0, (page - 1) * pageSize);

            var items = await query
                .OrderByDescending(s => s.LoginTime)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> TerminateSessionAsync(int sessionId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.DeletedFlag == 1);
            if (session == null) return false;

            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> TerminateAllUserSessionsAsync(int userId)
        {
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.DeletedFlag == 1 && (s.IsActive || s.LogoutTime == null))
                .ToListAsync();

            if (activeSessions.Count == 0) return 0;

            var now = DateTime.UtcNow;
            foreach (var session in activeSessions)
            {
                session.IsActive = false;
                session.LogoutTime = now;
            }

            await _context.SaveChangesAsync();
            return activeSessions.Count;
        }

        public async Task<(int ActiveCount, int TodayLogins, int TodayLogouts, int TotalSessions)> GetActivityStatsAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var expiryCutoff = DateTime.UtcNow.AddMinutes(-DefaultSessionExpiryMinutes);

            var activeCount = await _context.UserSessions
                .CountAsync(s => s.DeletedFlag == 1 && s.IsActive && s.LogoutTime == null && (s.UpdatedAt ?? s.LoginTime) >= expiryCutoff);

            var todayLogins = await _context.UserSessions
                .CountAsync(s => s.DeletedFlag == 1 && s.LoginTime >= todayUtc);

            var todayLogouts = await _context.UserSessions
                .CountAsync(s => s.DeletedFlag == 1 && s.LogoutTime != null && s.LogoutTime >= todayUtc);

            var totalSessions = await _context.UserSessions
                .CountAsync(s => s.DeletedFlag == 1);

            return (activeCount, todayLogins, todayLogouts, totalSessions);
        }

        public async Task<UserSession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.UserSessions
                .FromSqlRaw("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE id = {0} AND deleted_flag = 1
                """, sessionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSession>> GetActiveSessionsForUserAsync(int userId, int? excludeSessionId = null)
        {
            if (excludeSessionId.HasValue)
            {
                return await _context.UserSessions
                    .FromSqlRaw("""
                        SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                        FROM user_sessions
                        WHERE user_id = {0} AND id != {1} AND (is_active = true OR logout_time IS NULL)
                    """, userId, excludeSessionId.Value)
                    .ToListAsync();
            }

            return await _context.UserSessions
                .FromSqlRaw("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE user_id = {0} AND (is_active = true OR logout_time IS NULL)
                """, userId)
                .ToListAsync();
        }

        public async Task<List<UserSession>> GetActiveSessionsForEmailAsync(string email)
        {
            return await _context.UserSessions
                .FromSqlRaw("""
                    SELECT id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag, created_at, updated_at
                    FROM user_sessions
                    WHERE LOWER(email) = LOWER({0}) AND (is_active = true OR logout_time IS NULL)
                """, email)
                .ToListAsync();
        }

        public async Task<UserSession?> FindActiveSessionByTokenAsync(int userId, string token)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.SessionToken == token && s.DeletedFlag == 1)
                .OrderByDescending(s => s.LoginTime)
                .FirstOrDefaultAsync();
        }

        public async Task TouchSessionAsync(int sessionId, string clientIp)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session != null)
            {
                session.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(clientIp) && session.IpAddress != clientIp)
                {
                    session.IpAddress = clientIp;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM user_sessions
                WHERE deleted_flag = 1 AND is_active = true AND logout_time IS NULL
            """).SingleOrDefaultAsync();
        }

        public async Task<bool> TerminateSessionWithAuditAsync(int sessionId, int adminUserId)
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null) return false;

            var now = DateTime.UtcNow;
            session.EndSession(now);

            if (session.UserId > 0)
            {
                var otherSessions = await GetActiveSessionsForUserAsync(session.UserId, session.Id);
                foreach (var other in otherSessions)
                {
                    other.EndSession(now);
                }
            }

            try
            {
                var adminUser = await _context.Users
                    .FromSqlRaw("""
                        SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                        FROM users
                        WHERE "Id" = {0} AND "DeletedFlag" = 1
                    """, adminUserId)
                    .FirstOrDefaultAsync();

                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(AuditLog.CreateLog(
                    action: "Force Terminate Session",
                    module: "Auth",
                    performedBy: adminName,
                    details: $"Terminated active session #{sessionId} for user {session.UserName} ({session.Email})",
                    ipAddress: session.IpAddress,
                    status: "Success"
                ));
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ForceLogoutUserWithAuditAsync(int targetUserId, int adminUserId)
        {
            var user = await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, targetUserId)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;
            var activeSessions = await GetActiveSessionsForUserAsync(targetUserId);

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.EndSession(now);
                }
            }
            else if (user != null)
            {
                var emailSessions = await GetActiveSessionsForEmailAsync(user.Email);
                foreach (var s in emailSessions)
                {
                    s.EndSession(now);
                }
            }

            try
            {
                var adminUser = await _context.Users
                    .FromSqlRaw("""
                        SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                        FROM users
                        WHERE "Id" = {0} AND "DeletedFlag" = 1
                    """, adminUserId)
                    .FirstOrDefaultAsync();

                var adminName = adminUser?.Name ?? $"Admin #{adminUserId}";

                _context.AuditLogs.Add(AuditLog.CreateLog(
                    action: "Force User Logout",
                    module: "Auth",
                    performedBy: adminName,
                    details: $"Terminated all active sessions for {user?.Name ?? $"User #{targetUserId}"}",
                    ipAddress: "127.0.0.1",
                    status: "Success"
                ));
            }
            catch
            {
                // Ignore audit log error
            }

            await _context.SaveChangesAsync();
            return Math.Max(1, activeSessions.Count);
        }

        public async Task AddSessionAsync(UserSession session)
        {
            _context.UserSessions.Add(session);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Ignore concurrency collision if another request created it simultaneously
            }
        }
    }
}
