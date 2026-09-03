using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public DashboardService(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(string? timeframe = "7d")
        {
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            var roles = await _unitOfWork.Roles.GetActiveRolesAsync();
            var permissionsCount = await _unitOfWork.Permissions.CountAsync(p => p.DeletedFlag == 1);

            var activeUsersList = users.Where(u => u.DeletedFlag == 1).ToList();
            var inactiveUsersList = users.Where(u => u.DeletedFlag == 0).ToList();

            var totalUsers = users.Count;
            var activeUsersCount = activeUsersList.Count;
            var inactiveUsersCount = inactiveUsersList.Count;
            var totalRoles = roles.Count;

            // Query real active sessions count from database
            var activeSessions = await _context.UserSessions
                .AsNoTracking()
                .CountAsync(s => s.DeletedFlag == 1 && s.IsActive && s.LogoutTime == null);

            var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);
            var colorPalette = new[]
            {
                "#2563eb", // blue (Super Admin)
                "#8b5cf6", // purple (Admin)
                "#f59e0b", // amber (Manager)
                "#10b981", // emerald (Editor)
                "#ef4444", // rose (Viewer)
                "#06b6d4", // cyan
                "#ec4899", // pink
                "#84cc16", // lime
                "#64748b"  // slate
            };

            var userRoleCounts = users
                .GroupBy(u => u.RoleId ?? 0)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            const double totalCircumference = 427.26;
            double currentOffset = 0.0;
            var roleDistribution = new List<DashboardRoleItem>();

            for (int i = 0; i < userRoleCounts.Count; i++)
            {
                var item = userRoleCounts[i];
                var roleName = item.RoleId > 0 && roleMap.TryGetValue(item.RoleId, out var rName) ? rName : "Unassigned";
                var percentageValue = totalUsers > 0 ? Math.Round((double)item.Count / totalUsers * 100.0, 1) : 0;
                var dashLength = totalUsers > 0 ? Math.Round((double)item.Count / totalUsers * totalCircumference, 1) : 0;
                var color = colorPalette[i % colorPalette.Length];

                roleDistribution.Add(new DashboardRoleItem
                {
                    RoleId = item.RoleId,
                    Name = roleName,
                    Count = item.Count,
                    PercentageValue = percentageValue,
                    Percentage = $"{percentageValue:0.#}%",
                    Color = color,
                    StrokeDash = $"{dashLength} {totalCircumference - dashLength:0.#}",
                    StrokeOffset = $"{currentOffset:0.#}"
                });

                currentOffset -= dashLength;
            }

            // Query latest sessions for each user to get the true last login time
            var allSessions = await _context.UserSessions
                .AsNoTracking()
                .Where(s => s.DeletedFlag == 1)
                .OrderByDescending(s => s.LoginTime)
                .ToListAsync();

            var latestSessionByUserId = allSessions
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.First());

            var latestSessionByEmail = allSessions
                .Where(s => !string.IsNullOrWhiteSpace(s.Email))
                .GroupBy(s => s.Email.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var recentUsers = activeUsersList
                .Take(5)
                .Select(u =>
                {
                    var roleName = u.RoleId.HasValue && roleMap.TryGetValue(u.RoleId.Value, out var rName) ? rName : "Unassigned";
                    var roleBadge = roleName switch
                    {
                        "Super Admin" => "bg-indigo-50 text-indigo-700 border border-indigo-200",
                        "Admin" => "bg-blue-50 text-blue-700 border border-blue-200",
                        "Manager" => "bg-amber-50 text-amber-700 border border-amber-200",
                        "Editor" => "bg-emerald-50 text-emerald-700 border border-emerald-200",
                        _ => "bg-slate-50 text-slate-700 border border-slate-200"
                    };

                    // Find latest session for this user
                    UserSession? latestSession = null;
                    if (latestSessionByUserId.TryGetValue(u.Id, out var sById))
                    {
                        latestSession = sById;
                    }
                    else if (!string.IsNullOrWhiteSpace(u.Email) && latestSessionByEmail.TryGetValue(u.Email.ToLowerInvariant(), out var sByEmail))
                    {
                        latestSession = sByEmail;
                    }

                    string lastLoginText;
                    if (latestSession != null)
                    {
                        if (latestSession.IsActive && latestSession.LogoutTime == null)
                        {
                            lastLoginText = "Active now";
                        }
                        else
                        {
                            lastLoginText = FormatRelativeTime(latestSession.LoginTime);
                        }
                    }
                    else
                    {
                        lastLoginText = "Never";
                    }

                    return new DashboardRecentUserItem
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        Role = roleName,
                        RoleBadge = roleBadge,
                        Status = u.DeletedFlag == 1 ? "Active" : "Inactive",
                        LastLogin = lastLoginText,
                        Avatar = $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(u.Name)}",
                        Phone = u.Phone ?? string.Empty
                    };
                })
                .ToList();

            // Query real recent audit logs
            var recentAuditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentActivities = recentAuditLogs.Select((log, idx) =>
            {
                var actionLower = (log.Action ?? string.Empty).ToLowerInvariant();
                var iconBg = actionLower switch
                {
                    var a when a.Contains("login") || a.Contains("otp") || a.Contains("2fa") => "bg-emerald-100 text-emerald-600",
                    var a when a.Contains("terminate") || a.Contains("logout") || a.Contains("delete") => "bg-rose-100 text-rose-600",
                    var a when a.Contains("role") || a.Contains("permission") => "bg-purple-100 text-purple-600",
                    _ => "bg-blue-100 text-blue-600"
                };

                var actionType = actionLower switch
                {
                    var a when a.Contains("role") || a.Contains("permission") => "role_change",
                    var a when a.Contains("user") || a.Contains("create") => "user_created",
                    _ => "user_login"
                };

                return new DashboardActivityItem
                {
                    Id = log.Id > 0 ? log.Id : idx + 1,
                    Type = actionType,
                    Title = log.Action ?? "System Event",
                    TargetName = log.Module ?? "Workspace",
                    TargetHighlight = log.Status ?? "Completed",
                    ActionText = log.Details ?? "performed operation on",
                    Author = string.IsNullOrWhiteSpace(log.PerformedBy) ? "System Administrator" : log.PerformedBy,
                    Time = FormatRelativeTime(log.CreatedAt),
                    IconBg = iconBg
                };
            }).ToList();

            // If audit logs are empty, fallback to recent login sessions
            if (recentActivities.Count == 0)
            {
                var recentSess = allSessions.Take(3).ToList();
                foreach (var s in recentSess)
                {
                    var isLive = s.IsActive && s.LogoutTime == null;
                    recentActivities.Add(new DashboardActivityItem
                    {
                        Id = s.Id,
                        Type = "user_login",
                        Title = isLive ? "Active Login Session" : "Completed Session",
                        TargetName = s.UserName,
                        TargetHighlight = isLive ? "Active" : "Logged Out",
                        ActionText = $"authenticated from IP {s.IpAddress}",
                        Author = s.UserName,
                        Time = FormatRelativeTime(s.LoginTime),
                        IconBg = isLive ? "bg-emerald-100 text-emerald-600" : "bg-slate-100 text-slate-600"
                    });
                }
            }

            var daysCount = (timeframe?.ToLowerInvariant()) switch
            {
                "30d" => 30,
                "90d" => 90,
                _ => 7
            };

            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-daysCount);

            var allAuditLogsInPeriod = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1 && a.CreatedAt >= startDate)
                .ToListAsync();

            var chartData = new List<DashboardChartPoint>();

            for (int i = daysCount - 1; i >= 0; i--)
            {
                var dayStart = today.AddDays(-i);
                var dayEnd = dayStart.AddDays(1);
                var dayLabel = dayStart.ToString("MMM dd");

                var activeOnDay = allSessions.Count(s => s.LoginTime >= dayStart && s.LoginTime < dayEnd);
                var uniqueUsersOnDay = allSessions
                    .Where(s => s.LoginTime >= dayStart && s.LoginTime < dayEnd)
                    .Select(s => s.UserId)
                    .Distinct()
                    .Count();

                var auditLogsOnDay = allAuditLogsInPeriod.Count(a => a.CreatedAt >= dayStart && a.CreatedAt < dayEnd);

                chartData.Add(new DashboardChartPoint
                {
                    Day = dayLabel,
                    Active = activeOnDay,
                    NewUsers = uniqueUsersOnDay,
                    AuditLogs = auditLogsOnDay,
                    Total = totalUsers
                });
            }

            var recentLoginsCount = allSessions.Count(s => s.LoginTime >= today.AddDays(-7));
            var prevPeriodLoginsCount = allSessions.Count(s => s.LoginTime >= today.AddDays(-14) && s.LoginTime < today.AddDays(-7));
            var sessionsGrowth = prevPeriodLoginsCount > 0
                ? $"{((double)(recentLoginsCount - prevPeriodLoginsCount) / prevPeriodLoginsCount * 100):+0.#;-0.#;0}%"
                : (recentLoginsCount > 0 ? "+100%" : "0%");

            var userActivePercentage = totalUsers > 0 ? $"{Math.Round((double)activeUsersCount / totalUsers * 100):0.#}%" : "0%";

            return new DashboardSummaryResponse
            {
                Kpis = new DashboardKpiMetrics
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsersCount,
                    InactiveUsers = inactiveUsersCount,
                    TotalRoles = totalRoles,
                    TotalPermissions = permissionsCount,
                    ActiveSessions = activeSessions,
                    UsersGrowth = userActivePercentage,
                    RolesGrowth = $"{totalRoles} Active",
                    PermissionsGrowth = $"{permissionsCount} Matrix",
                    SessionsGrowth = sessionsGrowth
                },
                RoleDistribution = roleDistribution,
                RecentUsers = recentUsers,
                RecentActivities = recentActivities,
                ChartData = chartData,
                DateRangeDescription = $"Last {daysCount} Days ({today.AddDays(-daysCount):MMM dd} - {today:MMM dd, yyyy})"
            };
        }

        private static string FormatRelativeTime(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;
            if (diff.TotalSeconds < 60) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}
