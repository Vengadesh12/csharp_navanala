namespace MyBackend.Application.Contracts
{
    /// <summary>
    /// Comprehensive aggregation response model for the administrative dashboard.
    /// </summary>
    public class DashboardSummaryResponse
    {
        /// <summary>
        /// Key performance indicator summary metrics.
        /// </summary>
        public DashboardKpiMetrics Kpis { get; set; } = new();

        /// <summary>
        /// Breakdown of user distribution across active roles.
        /// </summary>
        public List<DashboardRoleItem> RoleDistribution { get; set; } = [];

        /// <summary>
        /// Top recent registered users in the workspace.
        /// </summary>
        public List<DashboardRecentUserItem> RecentUsers { get; set; } = [];

        /// <summary>
        /// Chronological audit and system activity items.
        /// </summary>
        public List<DashboardActivityItem> RecentActivities { get; set; } = [];

        /// <summary>
        /// Time-series data points for the user activity and growth area chart.
        /// </summary>
        public List<DashboardChartPoint> ChartData { get; set; } = [];

        /// <summary>
        /// Formatted human-readable date range description for the selected timeframe.
        /// </summary>
        public string DateRangeDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// High-level KPI metric counts and trend statistics.
    /// </summary>
    public class DashboardKpiMetrics
    {
        /// <summary>
        /// Total number of registered users (active + inactive).
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Count of active users (DeletedFlag = 1).
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Count of deactivated / soft-deleted users (DeletedFlag = 0).
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// Total number of configured active authorization roles.
        /// </summary>
        public int TotalRoles { get; set; }

        /// <summary>
        /// Total count of granular permission capabilities in the system.
        /// </summary>
        public int TotalPermissions { get; set; }

        /// <summary>
        /// Estimated or active online user sessions.
        /// </summary>
        public int ActiveSessions { get; set; }

        /// <summary>
        /// Percentage change indicator for total users (e.g. "+12.5%").
        /// </summary>
        public string UsersGrowth { get; set; } = "+12.5%";

        /// <summary>
        /// Percentage change indicator for roles.
        /// </summary>
        public string RolesGrowth { get; set; } = "+5.2%";

        /// <summary>
        /// Percentage change indicator for permissions.
        /// </summary>
        public string PermissionsGrowth { get; set; } = "+8.7%";

        /// <summary>
        /// Percentage change indicator for active sessions.
        /// </summary>
        public string SessionsGrowth { get; set; } = "+3.1%";
    }

    /// <summary>
    /// Role distribution item for donut chart visualization.
    /// </summary>
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

    /// <summary>
    /// User profile snippet displayed in the recent users table.
    /// </summary>
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

    /// <summary>
    /// System and administrative event item for the activity stream.
    /// </summary>
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

    /// <summary>
    /// Daily or interval aggregated points for trend area charts.
    /// </summary>
    public class DashboardChartPoint
    {
        public string Day { get; set; } = string.Empty;
        public int Active { get; set; }
        public int NewUsers { get; set; }
        public int AuditLogs { get; set; }
        public int Total { get; set; }
    }
}

