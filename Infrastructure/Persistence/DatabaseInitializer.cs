using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;

namespace MyBackend.Infrastructure.Persistence
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Ensure PostgreSQL database and tables exist
                await context.Database.EnsureCreatedAsync();

                // Seed / verify all standard system permissions
                var standardPermissions = new (string Key, string Name, string Desc)[]
                {
                    ("users.view", "View Users", "View member records and user directory."),
                    ("users.create", "Create Users", "Provision and onboard new member accounts."),
                    ("users.edit", "Edit Users", "Modify member profile data and roles."),
                    ("users.delete", "Delete Users", "Deactivate, soft-delete or restore users."),
                    ("users.manage", "Manage Users", "Full administrative control over user accounts."),
                    ("roles.view", "View Roles", "View workspace role definitions and summaries."),
                    ("roles.create", "Create Roles", "Create new access tiers in the workspace."),
                    ("roles.edit", "Edit Roles", "Modify role parameters and descriptions."),
                    ("roles.delete", "Delete Roles", "Deactivate or remove roles."),
                    ("roles.manage", "Manage Roles", "Full administrative control over role configurations."),
                    ("permissions.manage", "Permission Matrix Governance", "Assign capabilities and authorize operations."),
                    ("dashboard.view", "View Dashboard", "Access and view the workspace operational dashboard."),
                    ("reports.view", "View Reports", "Access and download security and audit reports."),
                    ("projects.view", "View Projects", "See active workspace projects and track progress."),
                    ("calendar.view", "View Calendar", "Access schedule events and calendar sessions."),
                    ("settings.view", "View Settings", "Access system configurations and workspace parameters."),
                    ("audit.view", "View Audit Log", "Inspect immutable workspace audit logs and system events."),
                    ("user_activity.view", "View User Activity", "Inspect user login, logout activity history and view currently active logged-in users."),
                    ("user_activity.force_logout", "Force Logout Sessions", "Immediately terminate active user sessions and force logout members."),
                    ("user_activity.manage", "Manage User Activity", "Terminate active user sessions and manage login sessions.")
                };

                foreach (var perm in standardPermissions)
                {
                    var exists = await context.Permissions.AnyAsync(p => p.PermissionKey.ToLower() == perm.Key.ToLower());
                    if (!exists)
                    {
                        context.Permissions.Add(new Permission
                        {
                            PermissionKey = perm.Key,
                            Name = perm.Name,
                            Description = perm.Desc,
                            DeletedFlag = 1
                        });
                    }
                }
                await context.SaveChangesAsync();

                // Seed standard workspace designations
                var standardDesignations = new (string Name, string Desc)[]
                {
                    ("Software Engineer", "Develops and maintains core applications and services."),
                    ("Senior Software Engineer", "Leads feature development and system architecture."),
                    ("Frontend Developer", "Builds responsive and interactive user interfaces."),
                    ("Backend Developer", "Designs robust server APIs, microservices, and databases."),
                    ("Full Stack Developer", "Works across client and server application stack."),
                    ("DevOps Engineer", "Manages CI/CD pipelines, cloud infrastructure, and releases."),
                    ("QA Engineer", "Executes test automation and quality assurance workflows."),
                    ("UI/UX Designer", "Creates user experience designs, wireframes, and design systems."),
                    ("Product Manager", "Defines product roadmap and oversees feature delivery."),
                    ("Project Manager", "Coordinates team deliverables, sprint milestones, and timelines."),
                    ("System Administrator", "Monitors IT infrastructure, networks, and server health."),
                    ("HR Manager", "Oversees talent acquisition, onboarding, and people operations.")
                };

                foreach (var des in standardDesignations)
                {
                    var exists = await context.Designations.AnyAsync(d => d.Name.ToLower() == des.Name.ToLower());
                    if (!exists)
                    {
                        context.Designations.Add(new Designation
                        {
                            Name = des.Name,
                            Description = des.Desc,
                            DeletedFlag = 1,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                await context.SaveChangesAsync();

                // Ensure all roles have their baseline permissions assigned
                var allPermissions = await context.Permissions.ToListAsync();
                var rolePermissionsMap = new Dictionary<int, string[]>
                {
                    // Role 1 (Employee): Standard workspace view capabilities
                    [1] = new[] { "dashboard.view", "users.view", "projects.view", "calendar.view", "reports.view", "user_activity.view" },
                    // Role 2 (Super Admin): All system permissions
                    [2] = standardPermissions.Select(p => p.Key).ToArray(),
                    // Role 3 (Manager): Workspace management capabilities
                    [3] = new[] { "dashboard.view", "users.view", "roles.view", "reports.view", "projects.view", "calendar.view", "settings.view", "audit.view", "user_activity.view", "user_activity.force_logout", "user_activity.manage" }
                };

                foreach (var (roleId, permKeys) in rolePermissionsMap)
                {
                    foreach (var permKey in permKeys)
                    {
                        var permissionEntity = allPermissions.FirstOrDefault(p => p.PermissionKey.Equals(permKey, StringComparison.OrdinalIgnoreCase));
                        if (permissionEntity != null)
                        {
                            var hasRolePerm = await context.RolePermissions
                                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionEntity.Id);
                            if (!hasRolePerm)
                            {
                                context.RolePermissions.Add(new RolePermission
                                {
                                    RoleId = roleId,
                                    PermissionId = permissionEntity.Id
                                });
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Seed dynamic Menus if empty or missing User Activity
                var defaultMenus = new List<Menu>
                {
                    new() { MenuKey = "dashboard.view", Label = "Dashboard", Icon = "◫", Route = "/dashboard", GroupName = "Core Access", Description = "System metrics & access summary", OrderIndex = 1, PermissionKey = "dashboard.view", DeletedFlag = 1 },
                    new() { MenuKey = "users.view", Label = "User Directory", Icon = "▦", Route = "/add-user", GroupName = "Core Access", Description = "Manage members & assign roles", OrderIndex = 2, PermissionKey = "users.view", DeletedFlag = 1 },
                    new() { MenuKey = "roles.view", Label = "Roles", Icon = "♙", Route = "/roles", GroupName = "Core Access", Description = "Configure workspace roles", OrderIndex = 3, PermissionKey = "roles.view", DeletedFlag = 1 },
                    new() { MenuKey = "permissions.manage", Label = "Permission Matrix", Icon = "⚿", Route = "/permissions", GroupName = "Core Access", Description = "Role permission assignments", OrderIndex = 4, PermissionKey = "permissions.manage", DeletedFlag = 1 },
                    new() { MenuKey = "user_activity.view", Label = "User Activity", Icon = "⏱", Route = "/user-activity", GroupName = "Operations & Audit", Description = "Live active sessions & login/logout tracking", OrderIndex = 5, PermissionKey = "user_activity.view", DeletedFlag = 1 },
                    new() { MenuKey = "audit.view", Label = "Audit Logs", Icon = "◌", Route = "/audit", GroupName = "Operations & Audit", Description = "Activity & security events", OrderIndex = 6, PermissionKey = "audit.view", DeletedFlag = 1 },
                    new() { MenuKey = "reports.view", Label = "Reports", Icon = "▤", Route = "/reports", GroupName = "Operations & Audit", Description = "Insights & exports", OrderIndex = 7, PermissionKey = "reports.view", DeletedFlag = 1 },
                    new() { MenuKey = "projects.view", Label = "Projects", Icon = "◇", Route = "/projects", GroupName = "Operations & Audit", Description = "Project initiatives", OrderIndex = 8, PermissionKey = "projects.view", DeletedFlag = 1 },
                    new() { MenuKey = "calendar.view", Label = "Schedule", Icon = "□", Route = "/calendar", GroupName = "Operations & Audit", Description = "Team rhythm & reviews", OrderIndex = 9, PermissionKey = "calendar.view", DeletedFlag = 1 },
                    new() { MenuKey = "settings.view", Label = "Settings", Icon = "⚙", Route = "/settings", GroupName = "Preferences", Description = "Workspace configuration", OrderIndex = 10, PermissionKey = "settings.view", DeletedFlag = 1 }
                };

                foreach (var menu in defaultMenus)
                {
                    if (!await context.Menus.AnyAsync(m => m.MenuKey.ToLower() == menu.MenuKey.ToLower()))
                    {
                        context.Menus.Add(menu);
                    }
                }

                // Seed initial Reports if table is empty
                if (!await context.Reports.AnyAsync())
                {
                    context.Reports.AddRange(
                        new Report
                        {
                            Title = "User Directory & Role Mapping",
                            Description = "Complete breakdown of active workspace members and assigned RBAC role tiers.",
                            Category = "Role Mapping",
                            Format = "PDF",
                            CreatedBy = "System Administrator",
                            Status = "Ready",
                            FileSize = "1.8 MB",
                            DeletedFlag = 1
                        },
                        new Report
                        {
                            Title = "Permission Matrix Audit",
                            Description = "Historical log of granular capability grants, assignments, and revocations.",
                            Category = "Security",
                            Format = "JSON",
                            CreatedBy = "Security Officer",
                            Status = "Ready",
                            FileSize = "640 KB",
                            DeletedFlag = 1
                        },
                        new Report
                        {
                            Title = "Privileged Access Compliance",
                            Description = "Super Admin activity tracking, elevation events, and compliance logs.",
                            Category = "Compliance",
                            Format = "CSV",
                            CreatedBy = "Audit Daemon",
                            Status = "Generated",
                            FileSize = "2.4 MB",
                            DeletedFlag = 1
                        },
                        new Report
                        {
                            Title = "Access Certification Summary",
                            Description = "Quarterly access review certification for engineering and management units.",
                            Category = "Access Audit",
                            Format = "Excel",
                            CreatedBy = "Compliance Lead",
                            Status = "Ready",
                            FileSize = "920 KB",
                            DeletedFlag = 1
                        }
                    );
                }

                // Seed initial Projects if table is empty
                if (!await context.Projects.AnyAsync())
                {
                    context.Projects.AddRange(
                        new Project
                        {
                            Name = "Engineering Role Segmentation",
                            Description = "Configuring least-privilege matrix roles for lead developers and DevOps engineers.",
                            Category = "DevOps",
                            Status = "In Progress",
                            Priority = "High",
                            LeadName = "Arun Kumar",
                            ProgressPercentage = 75,
                            DueDate = "Dec 15, 2026",
                            DeletedFlag = 1
                        },
                        new Project
                        {
                            Name = "Finance Access Scope Cleanup",
                            Description = "Revoking legacy administrative credentials and configuring view-only audit scopes.",
                            Category = "Finance",
                            Status = "Review",
                            Priority = "Medium",
                            LeadName = "Kaviya R",
                            ProgressPercentage = 90,
                            DueDate = "Nov 30, 2026",
                            DeletedFlag = 1
                        },
                        new Project
                        {
                            Name = "Quarterly Access Certification",
                            Description = "Auditing all active directory member permissions and multi-factor compliance.",
                            Category = "Security",
                            Status = "Planning",
                            Priority = "Critical",
                            LeadName = "Vengadesh M",
                            ProgressPercentage = 30,
                            DueDate = "Jan 20, 2027",
                            DeletedFlag = 1
                        },
                        new Project
                        {
                            Name = "SSO & SAML Enterprise Integration",
                            Description = "Connecting workspace authentication with enterprise identity provider.",
                            Category = "RBAC Rollout",
                            Status = "In Progress",
                            Priority = "High",
                            LeadName = "Divya S",
                            ProgressPercentage = 50,
                            DueDate = "Feb 10, 2027",
                            DeletedFlag = 1
                        }
                    );
                }

                // Seed initial Schedules if table is empty
                if (!await context.Schedules.AnyAsync())
                {
                    var today = DateTime.UtcNow;
                    context.Schedules.AddRange(
                        new ScheduleEvent
                        {
                            Title = "Quarterly RBAC & Role Audit",
                            Description = "Reviewing elevated role permissions with Managers and Super Admins.",
                            EventType = "Audit",
                            EventDate = today.AddDays(1).ToString("yyyy-MM-dd"),
                            StartTime = "10:00 AM",
                            EndTime = "11:30 AM",
                            Location = "Security Conference Room A",
                            Organizer = "Vengadesh M",
                            Status = "Scheduled",
                            Priority = "High",
                            AttendeesCount = 8,
                            DeletedFlag = 1
                        },
                        new ScheduleEvent
                        {
                            Title = "New Team Lead Onboarding & Permission Grant",
                            Description = "Provisioning new workspace managers and reviewing access policies.",
                            EventType = "Training",
                            EventDate = today.AddDays(3).ToString("yyyy-MM-dd"),
                            StartTime = "02:30 PM",
                            EndTime = "03:30 PM",
                            Location = "Virtual / Google Meet",
                            Organizer = "Kaviya R",
                            Status = "Scheduled",
                            Priority = "Normal",
                            AttendeesCount = 4,
                            DeletedFlag = 1
                        },
                        new ScheduleEvent
                        {
                            Title = "Security Policy Governance Sync",
                            Description = "Monthly security council meeting to review newly registered users and logs.",
                            EventType = "Governance",
                            EventDate = today.AddDays(7).ToString("yyyy-MM-dd"),
                            StartTime = "09:00 AM",
                            EndTime = "10:00 AM",
                            Location = "Executive Boardroom",
                            Organizer = "Arun Kumar",
                            Status = "Scheduled",
                            Priority = "Urgent",
                            AttendeesCount = 12,
                            DeletedFlag = 1
                        }
                    );
                }

                // Ensure all default System Settings exist in PostgreSQL database
                var defaultSettings = new List<SystemSetting>
                {
                    new()
                    {
                        SettingKey = "app_name",
                        SettingValue = "Role Management System",
                        Category = "General",
                        Description = "Application name displayed across the system.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "app_url",
                        SettingValue = "http://localhost:5173",
                        Category = "General",
                        Description = "Public application origin URL.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "timezone",
                        SettingValue = "(GMT+05:30) Asia/Kolkata",
                        Category = "General",
                        Description = "Default organization timezone.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "date_format",
                        SettingValue = "DD MMM YYYY",
                        Category = "General",
                        Description = "Standard date format for workspace timestamp displays.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "items_per_page",
                        SettingValue = "10",
                        Category = "General",
                        Description = "Default table pagination limit per page.",
                        DataType = "number",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "enable_registration",
                        SettingValue = "true",
                        Category = "General",
                        Description = "Allow new users to register on login portal.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "email_verification",
                        SettingValue = "true",
                        Category = "General",
                        Description = "Require email verification for newly registered users.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "session_timeout",
                        SettingValue = "30 Minutes",
                        Category = "General",
                        Description = "Automatically logout user after period of inactivity.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "two_factor_auth",
                        SettingValue = "true",
                        Category = "Security",
                        Description = "Require 2FA for all admin and elevated accounts.",
                        DataType = "boolean",
                        UpdatedBy = "Security Team"
                    },
                    new()
                    {
                        SettingKey = "password_expiry",
                        SettingValue = "true",
                        Category = "Security",
                        Description = "Force password change every 90 days.",
                        DataType = "boolean",
                        UpdatedBy = "Security Team"
                    },
                    new()
                    {
                        SettingKey = "login_attempt_limit",
                        SettingValue = "true",
                        Category = "Security",
                        Description = "Lock account after 5 consecutive failed attempts.",
                        DataType = "boolean",
                        UpdatedBy = "Security Team"
                    },
                    new()
                    {
                        SettingKey = "maintenance_mode",
                        SettingValue = "false",
                        Category = "General",
                        Description = "Enable maintenance mode for non-admin users.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "smtp_host",
                        SettingValue = "smtp.gmail.com",
                        Category = "Email",
                        Description = "Outgoing SMTP mail server hostname.",
                        DataType = "string",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "smtp_port",
                        SettingValue = "587",
                        Category = "Email",
                        Description = "SMTP TLS/SSL connection port.",
                        DataType = "number",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "email_alerts_enabled",
                        SettingValue = "true",
                        Category = "Notifications",
                        Description = "Send instant Gmail alerts when permissions or roles are modified.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "browser_push_enabled",
                        SettingValue = "true",
                        Category = "Notifications",
                        Description = "Enable in-app desktop push notifications.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "dark_mode_enabled",
                        SettingValue = "false",
                        Category = "Appearance",
                        Description = "Enable dark mode theme across workspace.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    },
                    new()
                    {
                        SettingKey = "auto_backup_enabled",
                        SettingValue = "true",
                        Category = "Backup",
                        Description = "Automated nightly PostgreSQL database snapshot backups.",
                        DataType = "boolean",
                        UpdatedBy = "System Admin"
                    }
                };

                foreach (var item in defaultSettings)
                {
                    if (!await context.SystemSettings.AnyAsync(s => s.SettingKey == item.SettingKey))
                    {
                        context.SystemSettings.Add(item);
                    }
                }

                // Ensure all default Setting Categories exist in PostgreSQL database
                var defaultCategories = new List<SettingCategory>
                {
                    new()
                    {
                        Name = "General",
                        Description = "Configure basic application settings and preferences.",
                        Icon = "Settings",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Security",
                        Description = "Authentication policies, 2FA, password expiry, and login limits.",
                        Icon = "ShieldOutlined",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Email",
                        Description = "Configure outgoing SMTP mail server and email templates.",
                        Icon = "LanguageOutlined",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Notifications",
                        Description = "Email alerts, webhooks, and push notification channels.",
                        Icon = "NotificationsNoneOutlined",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Appearance",
                        Description = "Workspace theme, branding colors, and interface density.",
                        Icon = "PaletteOutlined",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Backup",
                        Description = "Automated database backups, exports, and disaster recovery.",
                        Icon = "StorageOutlined",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    }
                };

                foreach (var cat in defaultCategories)
                {
                    if (!await context.SettingCategories.AnyAsync(c => c.Name.ToLower() == cat.Name.ToLower()))
                    {
                        context.SettingCategories.Add(cat);
                    }
                }

                // Ensure all standard Event Types exist in PostgreSQL database
                var defaultEventTypes = new List<EventType>
                {
                    new()
                    {
                        Name = "Audit",
                        Description = "Compliance, security and access audit sessions",
                        Color = "#6366f1",
                        Icon = "FactCheck",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Training",
                        Description = "Staff learning and skill certification workshops",
                        Color = "#10b981",
                        Icon = "School",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Governance",
                        Description = "Executive committee and policy oversight",
                        Color = "#f59e0b",
                        Icon = "Gavel",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Review",
                        Description = "Sprint, code, and access governance reviews",
                        Color = "#0ea5e9",
                        Icon = "RateReview",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    },
                    new()
                    {
                        Name = "Certification",
                        Description = "System and credentials re-certification",
                        Color = "#f43f5e",
                        Icon = "Verified",
                        CreatedBy = "System Admin",
                        DeletedFlag = 1
                    }
                };

                foreach (var evt in defaultEventTypes)
                {
                    if (!await context.EventTypes.AnyAsync(e => e.Name.ToLower() == evt.Name.ToLower()))
                    {
                        context.EventTypes.Add(evt);
                    }
                }

                // Seed initial Audit Logs if table is empty
                if (!await context.AuditLogs.AnyAsync())
                {
                    context.AuditLogs.AddRange(
                        new AuditLog
                        {
                            Action = "Permission Matrix Updated",
                            Module = "Permissions",
                            PerformedBy = "Super Admin",
                            Details = "Granted 'users.manage' and 'roles.view' to Manager role tier.",
                            IpAddress = "192.168.1.10",
                            Status = "Success",
                            DeletedFlag = 1
                        },
                        new AuditLog
                        {
                            Action = "Member Profile Created",
                            Module = "Users",
                            PerformedBy = "Administrator",
                            Details = "Provisioned new member account and sent welcome credentials via Gmail.",
                            IpAddress = "192.168.1.15",
                            Status = "Success",
                            DeletedFlag = 1
                        },
                        new AuditLog
                        {
                            Action = "Role Scope Modified",
                            Module = "Roles",
                            PerformedBy = "Super Admin",
                            Details = "Updated description and authority parameters for Support role.",
                            IpAddress = "192.168.1.10",
                            Status = "Success",
                            DeletedFlag = 1
                        },
                        new AuditLog
                        {
                            Action = "User Authentication Success",
                            Module = "Auth",
                            PerformedBy = "Admin Member",
                            Details = "JWT bearer token issued with 120min lifetime.",
                            IpAddress = "127.0.0.1",
                            Status = "Success",
                            DeletedFlag = 1
                        }
                    );
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Database tables and seed data initialized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing database schema.");
            }
        }
    }
}
