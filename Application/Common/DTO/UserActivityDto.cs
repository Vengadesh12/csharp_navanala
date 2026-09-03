using System;
using System.Collections.Generic;

namespace MyBackend.Application.Common.DTO;

public class UserSessionItemDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = "Member";
    public string IpAddress { get; set; } = "127.0.0.1";
    public string? UserAgent { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsActive { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class UserActivitySummaryDto
{
    public int ActiveUsersCount { get; set; }
    public int TotalLoginsToday { get; set; }
    public int TotalLogoutsToday { get; set; }
    public int TotalSessionsRecorded { get; set; }
    public List<UserSessionItemDto> ActiveSessions { get; set; } = [];
    public List<UserSessionItemDto> RecentActivities { get; set; } = [];
}

public class UserActivityQueryParameters
{
    public string? Search { get; set; }
    public string? Status { get; set; } = "all"; // all, active, completed
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedUserActivityResponse
{
    public List<UserSessionItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
