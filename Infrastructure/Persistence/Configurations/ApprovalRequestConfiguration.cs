using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequestModel>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequestModel> builder)
        {
            builder.ToTable("approval_requests");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(e => e.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(e => e.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(e => e.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.Comments)
                .HasColumnName("comments")
                .HasMaxLength(1000);

            builder.Property(e => e.ReviewedById)
                .HasColumnName("reviewed_by_id");

            builder.Property(e => e.ReviewedByName)
                .HasColumnName("reviewed_by_name")
                .HasMaxLength(150);

            builder.Property(e => e.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasIndex(e => e.UserId);
        }
    }
}
