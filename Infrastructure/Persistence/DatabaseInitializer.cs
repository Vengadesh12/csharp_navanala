using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                // Ensure PostgreSQL database and tables exist via EF Core model schema
                await context.Database.EnsureCreatedAsync();

                // Ensure all updated_at and created_at columns exist on all existing database tables
                await context.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE IF EXISTS ""users"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""ProfileImage"" VARCHAR(500) DEFAULT NULL;

                    ALTER TABLE IF EXISTS ""roles"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS ""departments"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS ""designations"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS ""permissions"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS ""rolepermissions"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS ""departmentpermissions"" 
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS user_sessions 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                        ALTER COLUMN session_token TYPE text,
                        ALTER COLUMN user_agent TYPE text;

                    ALTER TABLE IF EXISTS menus 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS audit_logs 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS reports 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS file_name VARCHAR(255) DEFAULT NULL;

                    ALTER TABLE IF EXISTS report_categories 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS projects 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS project_categories 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS schedules 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS system_settings 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS setting_categories 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS event_types 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS approval_requests 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS purchases 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS invoices 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    ALTER TABLE IF EXISTS invoice_items 
                        ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP;

                    DO $$
                    BEGIN
                        IF EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_name='users' AND column_name='profileimage'
                        ) AND NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_name='users' AND column_name='ProfileImage'
                        ) THEN
                            ALTER TABLE ""users"" RENAME COLUMN profileimage TO ""ProfileImage"";
                        END IF;
                    END $$;
                ");

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
                    ("departments.view", "View Departments", "View workspace departments and designation hierarchy."),
                    ("departments.create", "Create Departments", "Create new organizational departments."),
                    ("departments.edit", "Edit Departments", "Update department details and designation mappings."),
                    ("departments.delete", "Delete Departments", "Deactivate or remove departments."),
                    ("departments.manage", "Manage Departments", "Full administrative control over departments and designation assignments."),
                    ("permissions.manage", "Permission Matrix Governance", "Assign capabilities and authorize operations."),
                    ("dashboard.view", "View Dashboard", "Access and view the workspace operational dashboard."),
                    ("reports.view", "View Reports", "Access and download security and audit reports."),
                    ("projects.view", "View Projects", "See active workspace projects and track progress."),
                    ("calendar.view", "View Calendar", "Access schedule events and calendar sessions."),
                    ("settings.view", "View Settings", "Access system configurations and workspace parameters."),
                    ("audit.view", "View Audit Log", "Inspect immutable workspace audit logs and system events."),
                    ("user_activity.view", "View User Activity", "Inspect user login, logout activity history and view currently active logged-in users."),
                    ("user_activity.force_logout", "Force Logout Sessions", "Immediately terminate active user sessions and force logout members."),
                    ("user_activity.manage", "Manage User Activity", "Terminate active user sessions and manage login sessions."),
                    ("approvals.view", "View Approvals", "Access and view create approval workspace."),
                    ("approvals.create", "Create Approval", "Raise approval requests for hardware, software, laptops, and resources."),
                    ("approvals.manage", "Approve or Reject Approvals", "Review, approve or reject employee approval requests."),
                    ("purchases.view", "View Purchases & Quotations", "Access approved products, vendor quotes, and procurement tracking."),
                    ("purchases.create", "Add Vendor Quotation", "Add supplier quotes and commercial terms for approved products."),
                    ("purchases.manage", "Manage Procurement", "Full control over vendor quotations, PO issue, and purchase order lifecycles."),
                    ("invoices.view", "View Invoices", "Access and view billing & customer invoices."),
                    ("invoices.create", "Add Invoice", "Create and generate customer invoices with products and calculations."),
                    ("invoices.edit", "Edit Invoice", "Modify existing invoice records and line items."),
                    ("invoices.delete", "Delete Invoice", "Remove or cancel customer invoice records."),
                    ("invoices.manage", "Manage Invoices & GST", "Full administrative authority over invoices, tax settings, and GST number configuration.")
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

                // Seed standard departments
                var standardDepartments = new (string Name, string Desc)[]
                {
                    ("Software Development", "Core engineering, application architecture, and development teams."),
                    ("DevOps & Infrastructure", "Cloud platforms, CI/CD automation, and IT system reliability."),
                    ("Human Resources", "People operations, talent acquisition, and employee relations."),
                    ("Product Management", "Product roadmaps, feature strategy, and delivery management."),
                    ("Project Management", "Project execution, sprint planning, and team coordination."),
                    ("Quality Assurance", "Software test automation, QA verification, and release standards."),
                    ("UI/UX Design", "User experience research, visual design, and interface design systems.")
                };

                foreach (var dept in standardDepartments)
                {
                    var exists = await context.Departments.AnyAsync(d => d.Name.ToLower() == dept.Name.ToLower());
                    if (!exists)
                    {
                        context.Departments.Add(new Department
                        {
                            Name = dept.Name,
                            Description = dept.Desc,
                            DeletedFlag = 1,
                            CreatedAt = DateTime.UtcNow
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

                // Map departments to designations using department ID
                var allDbDepartments = await context.Departments.Where(d => d.DeletedFlag == 1).ToListAsync();
                var allDbDesignations = await context.Designations.Where(d => d.DeletedFlag == 1).ToListAsync();

                var departmentMapping = new Dictionary<string, string[]>
                {
                    ["Software Development"] = new[] { "Backend Developer", "Frontend Developer", "Full Stack Developer", "Software Engineer", "Senior Software Engineer" },
                    ["DevOps & Infrastructure"] = new[] { "DevOps Engineer", "System Administrator" },
                    ["Human Resources"] = new[] { "HR Manager" },
                    ["Product Management"] = new[] { "Product Manager" },
                    ["Project Management"] = new[] { "Project Manager" },
                    ["Quality Assurance"] = new[] { "QA Engineer" },
                    ["UI/UX Design"] = new[] { "UI/UX Designer" }
                };

                foreach (var (deptName, desNames) in departmentMapping)
                {
                    var targetDept = allDbDepartments.FirstOrDefault(d => d.Name.Equals(deptName, StringComparison.OrdinalIgnoreCase));
                    if (targetDept != null)
                    {
                        foreach (var desName in desNames)
                        {
                            var targetDes = allDbDesignations.FirstOrDefault(d => d.Name.Equals(desName, StringComparison.OrdinalIgnoreCase));
                            if (targetDes != null && targetDes.DepartmentId != targetDept.Id)
                            {
                                targetDes.DepartmentId = targetDept.Id;
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Seed standard roles if missing
                var standardRoles = new (int Id, string Name, string Desc)[]
                {
                    (1, "Employee", "Standard employee workspace account."),
                    (2, "Super Admin", "Full workspace administrative access."),
                    (3, "Manager", "Workspace manager with elevated permissions.")
                };

                foreach (var r in standardRoles)
                {
                    var roleExists = await context.Roles.AnyAsync(role => role.Id == r.Id || role.Name.ToLower() == r.Name.ToLower());
                    if (!roleExists)
                    {
                        context.Roles.Add(new Role
                        {
                            Name = r.Name,
                            Description = r.Desc,
                            DeletedFlag = 1
                        });
                    }
                }
                await context.SaveChangesAsync();

                // Seed default Super Admin user account if missing
                var adminExists = await context.Users.AnyAsync(u => u.Email.ToLower() == "admin@example.com");

                // Ensure all roles have their baseline permissions assigned
                var allPermissions = await context.Permissions.ToListAsync();
                var rolePermissionsMap = new Dictionary<int, string[]>
                {
                    // Role 1 (Employee): Standard workspace view capabilities + approvals create
                    [1] = new[] { "dashboard.view", "users.view", "departments.view", "projects.view", "calendar.view", "reports.view", "user_activity.view", "approvals.view", "approvals.create" },
                    // Role 2 (Super Admin): All system permissions
                    [2] = standardPermissions.Select(p => p.Key).ToArray(),
                    // Role 3 (Manager): Workspace management capabilities + approvals management + purchases + invoices
                    [3] = new[] { "dashboard.view", "users.view", "roles.view", "departments.view", "departments.edit", "departments.manage", "reports.view", "projects.view", "calendar.view", "settings.view", "audit.view", "user_activity.view", "user_activity.force_logout", "user_activity.manage", "approvals.view", "approvals.create", "approvals.manage", "purchases.view", "purchases.create", "purchases.manage", "invoices.view", "invoices.create", "invoices.edit", "invoices.delete", "invoices.manage" }
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

                // Baseline Department Permissions Map
                var allDepartments = await context.Departments.Where(d => d.DeletedFlag == 1).ToListAsync();
                var departmentPermissionsMap = new Dictionary<string, string[]>
                {
                    ["Software Development"] = new[] { "dashboard.view", "projects.view", "calendar.view", "departments.view", "approvals.view", "approvals.create" },
                    ["DevOps & Infrastructure"] = new[] { "dashboard.view", "settings.view", "audit.view", "user_activity.view", "departments.view", "approvals.view", "approvals.create" },
                    ["Human Resources"] = new[] { "dashboard.view", "users.view", "users.create", "users.edit", "departments.view", "reports.view", "approvals.view", "approvals.create", "purchases.view", "purchases.create", "purchases.manage" },
                    ["Product Management"] = new[] { "dashboard.view", "projects.view", "reports.view", "calendar.view", "departments.view", "approvals.view", "approvals.create" },
                    ["Project Management"] = new[] { "dashboard.view", "projects.view", "calendar.view", "reports.view", "departments.view", "approvals.view", "approvals.create" },
                    ["Quality Assurance"] = new[] { "dashboard.view", "projects.view", "reports.view", "departments.view", "approvals.view", "approvals.create" },
                    ["UI/UX Design"] = new[] { "dashboard.view", "projects.view", "calendar.view", "departments.view", "approvals.view", "approvals.create" }
                };

                foreach (var (deptName, permKeys) in departmentPermissionsMap)
                {
                    var deptEntity = allDepartments.FirstOrDefault(d => d.Name.Equals(deptName, StringComparison.OrdinalIgnoreCase));
                    if (deptEntity != null)
                    {
                        foreach (var permKey in permKeys)
                        {
                            var permissionEntity = allPermissions.FirstOrDefault(p => p.PermissionKey.Equals(permKey, StringComparison.OrdinalIgnoreCase));
                            if (permissionEntity != null)
                            {
                                var hasDeptPerm = await context.DepartmentPermissions
                                    .AnyAsync(dp => dp.DepartmentId == deptEntity.Id && dp.PermissionId == permissionEntity.Id);
                                if (!hasDeptPerm)
                                {
                                    context.DepartmentPermissions.Add(new DepartmentPermission
                                    {
                                        DepartmentId = deptEntity.Id,
                                        PermissionId = permissionEntity.Id
                                    });
                                }
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();

                // Seed dynamic Menus if empty or missing
                var defaultMenus = new List<Menu>
                {
                    new() { MenuKey = "dashboard.view", Label = "Dashboard", Icon = "◫", Route = "/dashboard", GroupName = "Core Access", Description = "System metrics & access summary", OrderIndex = 1, PermissionKey = "dashboard.view", DeletedFlag = 1 },
                    new() { MenuKey = "users.view", Label = "User Directory", Icon = "▦", Route = "/add-user", GroupName = "Core Access", Description = "Manage members & assign roles", OrderIndex = 2, PermissionKey = "users.view", DeletedFlag = 1 },
                    new() { MenuKey = "roles.view", Label = "Roles", Icon = "♙", Route = "/roles", GroupName = "Core Access", Description = "Configure workspace roles", OrderIndex = 3, PermissionKey = "roles.view", DeletedFlag = 1 },
                    new() { MenuKey = "departments.view", Label = "Departments", Icon = "🏢", Route = "/departments", GroupName = "Core Access", Description = "Department hierarchy & designation mapping", OrderIndex = 4, PermissionKey = "departments.view", DeletedFlag = 1 },
                    new() { MenuKey = "permissions.manage", Label = "Permission Matrix", Icon = "⚿", Route = "/permissions", GroupName = "Core Access", Description = "Role permission assignments", OrderIndex = 5, PermissionKey = "permissions.manage", DeletedFlag = 1 },
                    new() { MenuKey = "approvals.view", Label = "Create Approval", Icon = "✓", Route = "/create-approval", GroupName = "Management", Description = "Raise and manage employee product & resource approvals", OrderIndex = 6, PermissionKey = "approvals.view", DeletedFlag = 1 },
                    new() { MenuKey = "purchases.view", Label = "Purchases", Icon = "🛒", Route = "/purchases", GroupName = "Management", Description = "Procure approved products and manage vendor quotations", OrderIndex = 7, PermissionKey = "purchases.view", DeletedFlag = 1 },
                    new() { MenuKey = "invoices.view", Label = "Invoice", Icon = "🧾", Route = "/invoices", GroupName = "Management", Description = "Generate and manage customer invoices with GST calculations and PDF download", OrderIndex = 8, PermissionKey = "invoices.view", DeletedFlag = 1 },
                    new() { MenuKey = "request_access.view", Label = "Request Access", Icon = "🔑", Route = "/request-access", GroupName = "Management", Description = "Request system permissions and review access requests", OrderIndex = 9, PermissionKey = null, DeletedFlag = 1 },
                    new() { MenuKey = "user_activity.view", Label = "User Activity", Icon = "⏱", Route = "/user-activity", GroupName = "Operations & Audit", Description = "Live active sessions & login/logout tracking", OrderIndex = 10, PermissionKey = "user_activity.view", DeletedFlag = 1 },
                    new() { MenuKey = "audit.view", Label = "Audit Logs", Icon = "◌", Route = "/audit", GroupName = "Operations & Audit", Description = "Activity & security events", OrderIndex = 11, PermissionKey = "audit.view", DeletedFlag = 1 },
                    new() { MenuKey = "reports.view", Label = "Reports", Icon = "▤", Route = "/reports", GroupName = "Operations & Audit", Description = "Insights & exports", OrderIndex = 12, PermissionKey = "reports.view", DeletedFlag = 1 },
                    new() { MenuKey = "projects.view", Label = "Projects", Icon = "◇", Route = "/projects", GroupName = "Operations & Audit", Description = "Project initiatives", OrderIndex = 13, PermissionKey = "projects.view", DeletedFlag = 1 },
                    new() { MenuKey = "calendar.view", Label = "Schedule", Icon = "□", Route = "/calendar", GroupName = "Operations & Audit", Description = "Team rhythm & reviews", OrderIndex = 14, PermissionKey = "calendar.view", DeletedFlag = 1 },
                    new() { MenuKey = "settings.view", Label = "Settings", Icon = "⚙", Route = "/settings", GroupName = "Preferences", Description = "Workspace configuration", OrderIndex = 15, PermissionKey = "settings.view", DeletedFlag = 1 }
                };

                foreach (var menu in defaultMenus)
                {
                    if (!await context.Menus.AnyAsync(m => m.MenuKey.ToLower() == menu.MenuKey.ToLower()))
                    {
                        context.Menus.Add(menu);
                    }
                }
                await context.SaveChangesAsync();

                // Seed sample approval requests if table is empty
                var hasApprovals = await context.Approvals.AnyAsync();
                if (!hasApprovals)
                {
                    var sampleUser = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == "admin@example.com")
                                     ?? await context.Users.FirstOrDefaultAsync();
                    var sampleUserId = sampleUser?.Id ?? 1;
                    var sampleUserName = sampleUser?.Name ?? "Alex Morgan";
                    var sampleUserEmail = sampleUser?.Email ?? "alex.morgan@example.com";

                    var sampleRequests = new List<ApprovalRequest>
                    {
                        new()
                        {
                            UserId = sampleUserId,
                            EmployeeName = sampleUserName,
                            EmployeeEmail = sampleUserEmail,
                            DepartmentName = "Software Development",
                            ItemName = "Apple MacBook Pro 16-inch M3 Max (36GB RAM, 1TB SSD)",
                            Category = "Hardware & Devices",
                            Description = "Current workstation has insufficient memory for running local multi-tier Docker microservice architecture and container builds simultaneously.",
                            Quantity = 1,
                            Priority = "High",
                            EstimatedAmount = 249900.00m,
                            Status = "Pending",
                            Comments = null,
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            UpdatedAt = DateTime.UtcNow.AddDays(-2),
                            DeletedFlag = 1
                        },
                        new()
                        {
                            UserId = sampleUserId,
                            EmployeeName = "Sarah Connor",
                            EmployeeEmail = "sarah.connor@example.com",
                            DepartmentName = "Quality Assurance",
                            ItemName = "Dell UltraSharp 27-inch 4K USB-C Hub Monitor (U2723QE)",
                            Category = "Hardware & Devices",
                            Description = "Secondary 4K display required for automated regression test execution and multi-browser viewport cross-testing.",
                            Quantity = 1,
                            Priority = "Medium",
                            EstimatedAmount = 48900.00m,
                            Status = "Approved",
                            Comments = "Approved. Workstation peripheral equipment will be dispatched from IT inventory.",
                            ReviewedById = sampleUserId,
                            ReviewedByName = "David Miller (Manager)",
                            ReviewedAt = DateTime.UtcNow.AddDays(-1),
                            CreatedAt = DateTime.UtcNow.AddDays(-3),
                            UpdatedAt = DateTime.UtcNow.AddDays(-1),
                            DeletedFlag = 1
                        },
                        new()
                        {
                            UserId = sampleUserId,
                            EmployeeName = "Marcus Vance",
                            EmployeeEmail = "marcus.vance@example.com",
                            DepartmentName = "DevOps & Infrastructure",
                            ItemName = "JetBrains All Products Pack Commercial License",
                            Category = "Software & Tools",
                            Description = "Annual developer toolchain license for Rider, WebStorm, and DataGrip IDE access.",
                            Quantity = 1,
                            Priority = "Medium",
                            EstimatedAmount = 24500.00m,
                            Status = "Pending",
                            Comments = null,
                            CreatedAt = DateTime.UtcNow.AddHours(-18),
                            UpdatedAt = DateTime.UtcNow.AddHours(-18),
                            DeletedFlag = 1
                        },
                        new()
                        {
                            UserId = sampleUserId,
                            EmployeeName = "Emily Watson",
                            EmployeeEmail = "emily.watson@example.com",
                            DepartmentName = "UI/UX Design",
                            ItemName = "Ergonomic Height-Adjustable Standing Desk (Dual Motor)",
                            Category = "Office Equipment",
                            Description = "Ergonomic standing desk required due to back posture strain during long design sprint sessions.",
                            Quantity = 1,
                            Priority = "Low",
                            EstimatedAmount = 35000.00m,
                            Status = "Pending",
                            Comments = null,
                            CreatedAt = DateTime.UtcNow.AddHours(-6),
                            UpdatedAt = DateTime.UtcNow.AddHours(-6),
                            DeletedFlag = 1
                        }
                    };

                    context.Approvals.AddRange(sampleRequests);
                    await context.SaveChangesAsync();
                }

                // Seed sample invoices if table is empty
                var hasInvoices = await context.Invoices.AnyAsync();
                if (!hasInvoices)
                {
                    var sampleUser = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == "admin@example.com")
                                     ?? await context.Users.FirstOrDefaultAsync();
                    var sampleUserId = sampleUser?.Id ?? 1;
                    var sampleUserName = sampleUser?.Name ?? "Super Admin";

                    var sampleInvoices = new List<Invoice>
                    {
                        new()
                        {
                            InvoiceNumber = "INV-2026-0001",
                            CustomerName = "Acme Global Solutions Pvt Ltd",
                            CustomerEmail = "billing@acmeglobal.com",
                            CustomerPhone = "+91 98765 43210",
                            CustomerAddress = "Plot 42, Hitec City, Phase II, Hyderabad, Telangana 500081",
                            CustomerGstin = "36AACCA1234F1Z9",
                            CompanyGstin = "36AAAAA0000A1Z5",
                            InvoiceDate = DateTime.UtcNow.AddDays(-5),
                            DueDate = DateTime.UtcNow.AddDays(10),
                            Subtotal = 100000.00m,
                            TaxRate = 18.00m,
                            TaxAmount = 18000.00m,
                            DiscountAmount = 0.00m,
                            TotalAmount = 118000.00m,
                            TotalAmountInWords = "Rupees One Lakh Eighteen Thousand Only",
                            Status = "Paid",
                            PaymentMethod = "Bank Transfer",
                            Notes = "Annual Enterprise Cloud Architecture & Microservices Consulting retainer fee.",
                            TermsAndConditions = "Payment due within 15 days of invoice issue date. 18% GST applicable as per Indian Tax rules.",
                            CreatedByUserId = sampleUserId,
                            CreatedByName = sampleUserName,
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            DeletedFlag = 1,
                            Items = new List<InvoiceItem>
                            {
                                new()
                                {
                                    ProductName = "Enterprise Cloud Architecture Consulting",
                                    Description = "Architecture assessment and multi-tenant microservices deployment setup.",
                                    Quantity = 1,
                                    UnitPrice = 60000.00m,
                                    TaxRate = 18.00m,
                                    TaxAmount = 10800.00m,
                                    TotalAmount = 70800.00m,
                                    OrderIndex = 1,
                                    DeletedFlag = 1
                                },
                                new()
                                {
                                    ProductName = "DevOps CI/CD Automation & Security Audit",
                                    Description = "Kubernetes orchestration pipelines, secret scanning, and automated release gates.",
                                    Quantity = 1,
                                    UnitPrice = 40000.00m,
                                    TaxRate = 18.00m,
                                    TaxAmount = 7200.00m,
                                    TotalAmount = 47200.00m,
                                    OrderIndex = 2,
                                    DeletedFlag = 1
                                }
                            }
                        },
                        new()
                        {
                            InvoiceNumber = "INV-2026-0002",
                            CustomerName = "Zenith Infotech Ltd",
                            CustomerEmail = "accounts@zenithinfotech.io",
                            CustomerPhone = "+91 91234 56789",
                            CustomerAddress = "Cyber Towers, 4th Floor, Whitefield, Bangalore, Karnataka 560066",
                            CustomerGstin = "29AAACZ9876E1Z2",
                            CompanyGstin = "36AAAAA0000A1Z5",
                            InvoiceDate = DateTime.UtcNow.AddDays(-1),
                            DueDate = DateTime.UtcNow.AddDays(14),
                            Subtotal = 45000.00m,
                            TaxRate = 18.00m,
                            TaxAmount = 8100.00m,
                            DiscountAmount = 0.00m,
                            TotalAmount = 53100.00m,
                            TotalAmountInWords = "Rupees Fifty-Three Thousand One Hundred Only",
                            Status = "Pending",
                            PaymentMethod = "UPI",
                            Notes = "Custom UI/UX Design System and Front-end component toolkit delivery.",
                            TermsAndConditions = "Standard payment terms apply. Please remit payment via bank NEFT/RTGS or UPI.",
                            CreatedByUserId = sampleUserId,
                            CreatedByName = sampleUserName,
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            DeletedFlag = 1,
                            Items = new List<InvoiceItem>
                            {
                                new()
                                {
                                    ProductName = "UI/UX Design System & Mobile App Prototype",
                                    Description = "Figma design system tokens, responsive component wireframes, and design specs.",
                                    Quantity = 1,
                                    UnitPrice = 45000.00m,
                                    TaxRate = 18.00m,
                                    TaxAmount = 8100.00m,
                                    TotalAmount = 53100.00m,
                                    OrderIndex = 1,
                                    DeletedFlag = 1
                                }
                            }
                        }
                    };

                    context.Invoices.AddRange(sampleInvoices);
                    await context.SaveChangesAsync();
                }

                // System configuration and foundation metadata initialized below

                // Ensure all default System Settings exist in PostgreSQL database
                var defaultSettings = new List<SystemSetting>
                {
                    new()
                    {
                        SettingKey = "app_name",
                        SettingValue = "MyBackend Technologies",
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

                // Seed default project categories
                var defaultProjectCategories = new (string Name, string Desc)[]
                {
                    ("RBAC Rollout", "Role-based access control rollouts and permission matrices"),
                    ("DevOps", "CI/CD pipelines, containerization, and infrastructure automation"),
                    ("Security", "Security audits, credentials rotation, and compliance"),
                    ("Finance", "Financial reporting, billing reconciliation, and budgets"),
                    ("Governance", "Access policies, compliance reviews, and governance boards")
                };

                foreach (var cat in defaultProjectCategories)
                {
                    if (!await context.ProjectCategories.AnyAsync(c => c.Name.ToLower() == cat.Name.ToLower()))
                    {
                        context.ProjectCategories.Add(new ProjectCategory
                        {
                            Name = cat.Name,
                            Description = cat.Desc,
                            DeletedFlag = 1,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Seed default report categories
                var defaultReportCategories = new (string Name, string Desc)[]
                {
                    ("Compliance", "Compliance and regulatory adherence reports"),
                    ("Security", "Security audits, vulnerability scans, and access control"),
                    ("Role Mapping", "Role-to-permission mapping and access reviews"),
                    ("Access Audit", "User login activity and privilege escalation audit"),
                    ("User Directory", "User directory exports and account status reports"),
                    ("Financial Audit", "Billing, expense reconciliation, and financial audits"),
                    ("Governance", "Organizational policies and governance oversight")
                };

                foreach (var cat in defaultReportCategories)
                {
                    if (!await context.ReportCategories.AnyAsync(c => c.Name.ToLower() == cat.Name.ToLower()))
                    {
                        context.ReportCategories.Add(new ReportCategory
                        {
                            Name = cat.Name,
                            Description = cat.Desc,
                            DeletedFlag = 1,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync();

                // Backfill existing reports category_id if null using EF Core
                try
                {
                    var reportCategories = await context.ReportCategories.ToListAsync();
                    var unassignedReports = await context.Reports
                        .Where(r => r.CategoryId == null || r.CategoryId == 0)
                        .ToListAsync();

                    if (unassignedReports.Count > 0)
                    {
                        foreach (var report in unassignedReports)
                        {
                            var match = reportCategories.FirstOrDefault(rc =>
                                string.Equals(rc.Name.Trim(), report.Category.Trim(), StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                report.CategoryId = match.Id;
                            }
                        }

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Note: reports category_id backfill executed with notice.");
                }

                // Ensure audit_logs has historical activity records for dashboard trend charts
                try
                {
                    var auditLogCount = await context.AuditLogs.CountAsync();
                    if (auditLogCount < 10)
                    {
                        var nowUtc = DateTime.UtcNow;
                        var sampleAuditLogs = new List<AuditLog>
                        {
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "superadmin@example.com", Details = "Super Admin authenticated via OTP/JWT", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddHours(-2), DeletedFlag = 1 },
                            new() { Action = "Role Assigned", Module = "Roles", PerformedBy = "superadmin@example.com", Details = "Assigned 'Admin' role to new member", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddHours(-5), DeletedFlag = 1 },
                            new() { Action = "Permission Granted", Module = "Permissions", PerformedBy = "superadmin@example.com", Details = "Granted 'audit.view' capability to Manager role", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-1).AddHours(3), DeletedFlag = 1 },
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "admin@example.com", Details = "Admin logged in successfully", IpAddress = "192.168.1.10", Status = "Success", CreatedAt = nowUtc.AddDays(-1).AddHours(-4), DeletedFlag = 1 },
                            new() { Action = "User Created", Module = "Users", PerformedBy = "superadmin@example.com", Details = "Registered workspace user Sarah Jenkins", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-2).AddHours(2), DeletedFlag = 1 },
                            new() { Action = "Setting Updated", Module = "Settings", PerformedBy = "superadmin@example.com", Details = "Updated security policy 2FA enforcement to optional", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-2).AddHours(-3), DeletedFlag = 1 },
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "manager@example.com", Details = "Manager logged in successfully", IpAddress = "192.168.1.15", Status = "Success", CreatedAt = nowUtc.AddDays(-3).AddHours(4), DeletedFlag = 1 },
                            new() { Action = "Security Audit Export", Module = "Reports", PerformedBy = "superadmin@example.com", Details = "Generated security audit compliance export", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-3).AddHours(-2), DeletedFlag = 1 },
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "editor@example.com", Details = "Editor logged in successfully", IpAddress = "192.168.1.22", Status = "Success", CreatedAt = nowUtc.AddDays(-4).AddHours(1), DeletedFlag = 1 },
                            new() { Action = "Role Modified", Module = "Roles", PerformedBy = "superadmin@example.com", Details = "Updated description for Editor role", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-4).AddHours(-5), DeletedFlag = 1 },
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "admin@example.com", Details = "Admin logged in successfully", IpAddress = "192.168.1.10", Status = "Success", CreatedAt = nowUtc.AddDays(-5).AddHours(3), DeletedFlag = 1 },
                            new() { Action = "Password Changed", Module = "Auth", PerformedBy = "sarah.j@example.com", Details = "User self-service password reset completed", IpAddress = "192.168.1.45", Status = "Success", CreatedAt = nowUtc.AddDays(-5).AddHours(-2), DeletedFlag = 1 },
                            new() { Action = "User Login", Module = "Auth", PerformedBy = "superadmin@example.com", Details = "Super Admin authenticated via OTP/JWT", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-6).AddHours(2), DeletedFlag = 1 },
                            new() { Action = "User Session Terminated", Module = "UserActivity", PerformedBy = "superadmin@example.com", Details = "Terminated expired user session", IpAddress = "127.0.0.1", Status = "Success", CreatedAt = nowUtc.AddDays(-6).AddHours(-4), DeletedFlag = 1 }
                        };

                        context.AuditLogs.AddRange(sampleAuditLogs);
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Note: audit logs seeding check executed with notice.");
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Database tables and system schema initialized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing database schema.");
            }
        }
    }
}
