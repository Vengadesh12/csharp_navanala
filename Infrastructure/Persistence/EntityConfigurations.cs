using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Entities;
using System;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    // =========================================================================
    // 4. Fluent API Entity Type Configurations (IEntityTypeConfiguration<T>)
    // =========================================================================

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .HasColumnName("Password")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId");

            builder.Property(x => x.DesignationId)
                .HasColumnName("DesignationId");

            builder.Property(x => x.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(50);

            builder.Property(x => x.Age)
                .HasColumnName("Age");

            builder.Property(x => x.Address)
                .HasColumnName("Address")
                .HasMaxLength(500);

            builder.Property(x => x.ProfileImage)
                .HasColumnName("ProfileImage")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.IsFirstLogin)
                .HasColumnName("IsFirstLogin")
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Ignore(x => x.Password);
        }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("departments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
    {
        public void Configure(EntityTypeBuilder<Designation> builder)
        {
            builder.ToTable("designations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DepartmentId)
                .HasColumnName("DepartmentId");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.HasOne(x => x.Department)
                .WithMany(d => d.Designations)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.PermissionKey)
                .HasColumnName("PermissionKey")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("rolepermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class DepartmentPermissionConfiguration : IEntityTypeConfiguration<DepartmentPermission>
    {
        public void Configure(EntityTypeBuilder<DepartmentPermission> builder)
        {
            builder.ToTable("departmentpermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.DepartmentId)
                .HasColumnName("DepartmentId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("user_sessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            builder.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100);

            builder.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("text");

            builder.Property(x => x.LoginTime)
                .HasColumnName("login_time")
                .IsRequired();

            builder.Property(x => x.LogoutTime)
                .HasColumnName("logout_time");

            builder.Property(x => x.SessionToken)
                .HasColumnName("session_token")
                .HasColumnType("text");

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("menus");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.MenuKey)
                .HasColumnName("menukey")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Label)
                .HasColumnName("label")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.Route)
                .HasColumnName("route")
                .HasMaxLength(200);

            builder.Property(x => x.GroupName)
                .HasColumnName("groupname")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.OrderIndex)
                .HasColumnName("orderindex")
                .HasDefaultValue(0);

            builder.Property(x => x.PermissionKey)
                .HasColumnName("permissionkey")
                .HasMaxLength(100);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deletedflag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Module)
                .HasColumnName("module")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PerformedBy)
                .HasColumnName("performed_by")
                .HasMaxLength(150);

            builder.Property(x => x.Details)
                .HasColumnName("details");

            builder.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("reports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id");

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Format)
                .HasColumnName("format")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.FileSize)
                .HasColumnName("file_size")
                .HasMaxLength(50);

            builder.Property(x => x.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class ReportCategoryConfiguration : IEntityTypeConfiguration<ReportCategory>
    {
        public void Configure(EntityTypeBuilder<ReportCategory> builder)
        {
            builder.ToTable("report_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("projects");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.LeadName)
                .HasColumnName("lead_name")
                .HasMaxLength(150);

            builder.Property(x => x.ProgressPercentage)
                .HasColumnName("progress_percentage");

            builder.Property(x => x.DueDate)
                .HasColumnName("due_date");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class ProjectCategoryConfiguration : IEntityTypeConfiguration<ProjectCategory>
    {
        public void Configure(EntityTypeBuilder<ProjectCategory> builder)
        {
            builder.ToTable("project_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class ScheduleEventConfiguration : IEntityTypeConfiguration<ScheduleEvent>
    {
        public void Configure(EntityTypeBuilder<ScheduleEvent> builder)
        {
            builder.ToTable("schedules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(50);

            builder.Property(x => x.EventDate)
                .HasColumnName("event_date");

            builder.Property(x => x.StartTime)
                .HasColumnName("start_time")
                .HasMaxLength(50);

            builder.Property(x => x.EndTime)
                .HasColumnName("end_time")
                .HasMaxLength(50);

            builder.Property(x => x.Location)
                .HasColumnName("location")
                .HasMaxLength(200);

            builder.Property(x => x.Organizer)
                .HasColumnName("organizer")
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.AttendeesCount)
                .HasColumnName("attendees_count");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("system_settings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.SettingKey)
                .HasColumnName("setting_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.SettingValue)
                .HasColumnName("setting_value");

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.DataType)
                .HasColumnName("data_type")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(150);
        }
    }

    public class SettingCategoryConfiguration : IEntityTypeConfiguration<SettingCategory>
    {
        public void Configure(EntityTypeBuilder<SettingCategory> builder)
        {
            builder.ToTable("setting_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
    {
        public void Configure(EntityTypeBuilder<EventType> builder)
        {
            builder.ToTable("event_types");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.Color)
                .HasColumnName("color")
                .HasMaxLength(50);

            builder.Property(x => x.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
        {
            builder.ToTable("approval_requests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(x => x.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.Comments)
                .HasColumnName("comments")
                .HasMaxLength(1000);

            builder.Property(x => x.ReviewedById)
                .HasColumnName("reviewed_by_id");

            builder.Property(x => x.ReviewedByName)
                .HasColumnName("reviewed_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.ToTable("purchases");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.ApprovalRequestId)
                .HasColumnName("approval_request_id");

            builder.Property(x => x.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150);

            builder.Property(x => x.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(x => x.VendorName)
                .HasColumnName("vendor_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.VendorContact)
                .HasColumnName("vendor_contact")
                .HasMaxLength(100);

            builder.Property(x => x.VendorEmail)
                .HasColumnName("vendor_email")
                .HasMaxLength(150);

            builder.Property(x => x.QuotationNumber)
                .HasColumnName("quotation_number")
                .HasMaxLength(100);

            builder.Property(x => x.QuotationAmount)
                .HasColumnName("quotation_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.QuotationDate)
                .HasColumnName("quotation_date");

            builder.Property(x => x.DeliveryTimeline)
                .HasColumnName("delivery_timeline")
                .HasMaxLength(100);

            builder.Property(x => x.PaymentTerms)
                .HasColumnName("payment_terms")
                .HasMaxLength(200);

            builder.Property(x => x.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(x => x.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }

    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("invoices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.InvoiceNumber)
                .HasColumnName("invoice_number")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(150);

            builder.Property(x => x.CustomerPhone)
                .HasColumnName("customer_phone")
                .HasMaxLength(50);

            builder.Property(x => x.CustomerAddress)
                .HasColumnName("customer_address");

            builder.Property(x => x.CustomerGstin)
                .HasColumnName("customer_gstin")
                .HasMaxLength(50);

            builder.Property(x => x.CompanyGstin)
                .HasColumnName("company_gstin")
                .HasMaxLength(50);

            builder.Property(x => x.InvoiceDate)
                .HasColumnName("invoice_date")
                .IsRequired();

            builder.Property(x => x.DueDate)
                .HasColumnName("due_date");

            builder.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmountInWords)
                .HasColumnName("total_amount_in_words")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.PaymentMethod)
                .HasColumnName("payment_method")
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .HasColumnName("notes");

            builder.Property(x => x.TermsAndConditions)
                .HasColumnName("terms_and_conditions");

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(x => x.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasMany(x => x.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("invoice_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.InvoiceId)
                .HasColumnName("invoice_id")
                .IsRequired();

            builder.Property(x => x.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description");

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(x => x.OrderIndex)
                .HasColumnName("order_index")
                .HasDefaultValue(0);

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.ToTable("userpermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id");

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
    {
        public void Configure(EntityTypeBuilder<AccessRequest> builder)
        {
            builder.ToTable("access_requests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.UserEmail)
                .HasColumnName("user_email")
                .HasMaxLength(150);

            builder.Property(x => x.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(150);

            builder.Property(x => x.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(150);

            builder.Property(x => x.PermissionKey)
                .HasColumnName("permission_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PermissionName)
                .HasColumnName("permission_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Module)
                .HasColumnName("module")
                .HasMaxLength(100);

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(x => x.ReviewerId)
                .HasColumnName("reviewer_id");

            builder.Property(x => x.ReviewerName)
                .HasColumnName("reviewer_name")
                .HasMaxLength(150);

            builder.Property(x => x.ReviewerComments)
                .HasColumnName("reviewer_comments");

            builder.Property(x => x.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(x => x.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}
