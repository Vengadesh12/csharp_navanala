using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public UserActivityService(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserActivitySummaryDto> GetSummaryAsync()
        {
            var stats = await _unitOfWork.Sessions.GetActivityStatsAsync();
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var recentSessions = await _unitOfWork.Sessions.GetAllRecentSessionsAsync(15);

            // Fetch user role map
            var roleMap = await GetUserRoleMapAsync();

            var activeDtos = activeSessions.Select(s => MapToDto(s, roleMap)).ToList();
            var recentDtos = recentSessions.Select(s => MapToDto(s, roleMap)).ToList();

            return new UserActivitySummaryDto
            {
                ActiveUsersCount = stats.ActiveCount,
                TotalLoginsToday = stats.TodayLogins,
                TotalLogoutsToday = stats.TodayLogouts,
                TotalSessionsRecorded = stats.TotalSessions,
                ActiveSessions = activeDtos,
                RecentActivities = recentDtos
            };
        }

        public async Task<PagedUserActivityResponse> GetPagedActivitiesAsync(UserActivityQueryParameters query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var (items, totalCount) = await _unitOfWork.Sessions.GetPagedSessionsAsync(query.Search, query.Status, page, pageSize);
            var roleMap = await GetUserRoleMapAsync();

            var dtos = items.Select(s => MapToDto(s, roleMap)).ToList();

            return new PagedUserActivityResponse
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<UserSessionItemDto>> GetActiveUsersAsync()
        {
            var activeSessions = await _unitOfWork.Sessions.GetActiveSessionsAsync();
            var roleMap = await GetUserRoleMapAsync();

            return activeSessions.Select(s => MapToDto(s, roleMap)).ToList();
        }

        public async Task<UserSession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<bool> TerminateSessionAsync(int sessionId, int adminUserId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return false;

            var now = DateTime.UtcNow;
            session.EndSession(now);

            // Also mark any other open/active sessions for the same user as terminated
            if (session.UserId > 0)
            {
                var otherSessions = await _context.UserSessions
                    .Where(s => s.UserId == session.UserId && s.Id != session.Id && (s.IsActive || s.LogoutTime == null))
                    .ToListAsync();

                foreach (var other in otherSessions)
                {
                    other.EndSession(now);
                }
            }

            // Write audit log using Business Object factory
            try
            {
                var adminUser = await _context.Users.FindAsync(adminUserId);
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
                // Ignore audit log error if write fails
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ForceLogoutUserAsync(int targetUserId, int adminUserId)
        {
            var user = await _context.Users.FindAsync(targetUserId);
            var now = DateTime.UtcNow;

            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == targetUserId && (s.IsActive || s.LogoutTime == null))
                .ToListAsync();

            if (activeSessions.Count > 0)
            {
                foreach (var session in activeSessions)
                {
                    session.EndSession(now);
                }
            }
            else if (user != null)
            {
                var emailSessions = await _context.UserSessions
                    .Where(s => s.Email.ToLower() == user.Email.ToLower() && (s.IsActive || s.LogoutTime == null))
                    .ToListAsync();

                foreach (var s in emailSessions)
                {
                    s.EndSession(now);
                }
            }

            // Record audit log using Business Object factory
            try
            {
                var adminUser = await _context.Users.FindAsync(adminUserId);
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

        private async Task<Dictionary<int, string>> GetUserRoleMapAsync()
        {
            try
            {
                var roles = await _context.Roles.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name);
                var userRoles = await _context.Users.AsNoTracking()
                    .Where(u => u.DeletedFlag == 1 && u.RoleId != null)
                    .Select(u => new { u.Id, RoleId = u.RoleId!.Value })
                    .ToListAsync();

                var map = new Dictionary<int, string>();
                foreach (var ur in userRoles)
                {
                    if (roles.TryGetValue(ur.RoleId, out var roleName))
                    {
                        map[ur.Id] = roleName;
                    }
                }
                return map;
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        private static UserSessionItemDto MapToDto(UserSession s, Dictionary<int, string> roleMap)
        {
            var isCurrentlyActive = s.IsActive && s.LogoutTime == null;
            var roleName = roleMap.TryGetValue(s.UserId, out var r) ? r : "Member";

            var (browser, os) = ParseUserAgent(s.UserAgent);
            var durationFormatted = FormatDuration(s.LoginTime, s.LogoutTime, isCurrentlyActive);

            return new UserSessionItemDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Email = s.Email,
                UserName = s.UserName,
                RoleName = roleName,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                Browser = browser,
                Os = os,
                LoginTime = s.LoginTime,
                LogoutTime = s.LogoutTime,
                IsActive = isCurrentlyActive,
                DurationFormatted = durationFormatted,
                Status = isCurrentlyActive ? "Active" : "Completed"
            };
        }

        private static string FormatDuration(DateTime loginTime, DateTime? logoutTime, bool isActive)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime endUtc = logoutTime ?? nowUtc;
            TimeSpan span = endUtc - loginTime;

            if (span.TotalSeconds < 0)
            {
                DateTime nowLocal = DateTime.Now;
                DateTime endLocal = logoutTime ?? nowLocal;
                var altSpan = endLocal - loginTime;
                if (altSpan.TotalSeconds >= 0)
                {
                    span = altSpan;
                }
                else
                {
                    span = TimeSpan.Zero;
                }
            }

            string timeStr;
            if (span.TotalHours >= 24)
            {
                timeStr = $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m {span.Seconds}s";
            }
            else if (span.TotalHours >= 1)
            {
                timeStr = $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            }
            else if (span.TotalMinutes >= 1)
            {
                timeStr = $"{(int)span.TotalMinutes}m {span.Seconds}s";
            }
            else
            {
                timeStr = $"{(int)span.TotalSeconds}s";
            }

            return isActive ? $"Active ({timeStr})" : timeStr;
        }

        private static (string Browser, string Os) ParseUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return ("Web Browser", "Desktop");
            }

            var ua = userAgent;
            string browser = "Chrome";
            string os = "Windows";

            // Determine OS
            if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase)) os = "Windows";
            else if (ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) os = "macOS";
            else if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase)) os = "Linux";
            else if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase)) os = "Android";
            else if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)) os = "iOS";

            // Determine Browser
            if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) browser = "Microsoft Edge";
            else if (ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) browser = "Chrome";
            else if (ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) browser = "Firefox";
            else if (ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) browser = "Safari";
            else if (ua.Contains("Postman", StringComparison.OrdinalIgnoreCase)) browser = "Postman API Client";

            return (browser, os);
        }
    }
}
