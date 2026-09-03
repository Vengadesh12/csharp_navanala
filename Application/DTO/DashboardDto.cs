using System;
using System.Collections.Generic;

namespace MyBackend.Application.DTO;

public class DashboardSummaryResponse
{
    public DashboardKpiMetrics Kpis { get; set; } = new();

    public List<DashboardRoleItem> RoleDistribution { get; set; } = [];

    public List<DashboardRecentUserItem> RecentUsers { get; set; } = [];

    public List<DashboardActivityItem> RecentActivities { get; set; } = [];

    public List<DashboardChartPoint> ChartData { get; set; } = [];

    public string DateRangeDescription { get; set; } = string.Empty;
}

public class DashboardKpiMetrics
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int InactiveUsers { get; set; }

    public int TotalRoles { get; set; }

    public int TotalPermissions { get; set; }

    public int ActiveSessions { get; set; }

    public string UsersGrowth { get; set; } = "+12.5%";

    public string RolesGrowth { get; set; } = "+5.2%";

    public string PermissionsGrowth { get; set; } = "+8.7%";

    public string SessionsGrowth { get; set; } = "+3.1%";
}

public class DashboardRoleItem
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public double PercentageValue { get; set; }
    public string Percentage { get; set; } = "0%";
    public string Color { get; set; } = "#2563eb";
    public string StrokeDash { get; set; } = "0 427";
    public string StrokeOffset { get; set; } = "0";
}

public class DashboardRecentUserItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public string Role { get; set; } = "Unassigned";
    public string RoleBadge { get; set; } = "bg-slate-100 text-slate-700";
    public string Status { get; set; } = "Active";
    public string LastLogin { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class DashboardActivityItem
{
    public int Id { get; set; }
    public string Type { get; set; } = "user";
    public string Title { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public string TargetHighlight { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string IconBg { get; set; } = "bg-blue-100 text-blue-600";
}

public class DashboardChartPoint
{
    public string Day { get; set; } = string.Empty;
    public int Active { get; set; }
    public int NewUsers { get; set; }
    public int AuditLogs { get; set; }
    public int Total { get; set; }
}
