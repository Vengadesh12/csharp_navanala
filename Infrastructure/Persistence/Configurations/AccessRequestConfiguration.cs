using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequestModel>
    {
        public void Configure(EntityTypeBuilder<AccessRequestModel> builder)
        {
            builder.ToTable("access_requests");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.UserEmail)
                .HasColumnName("user_email")
                .HasMaxLength(150);

            builder.Property(e => e.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(150);

            builder.Property(e => e.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(150);

            builder.Property(e => e.PermissionKey)
                .HasColumnName("permission_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.PermissionName)
                .HasColumnName("permission_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.Module)
                .HasColumnName("module")
                .HasMaxLength(100);

            builder.Property(e => e.Reason)
                .HasColumnName("reason")
                .IsRequired();

            builder.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.ReviewerId)
                .HasColumnName("reviewer_id");

            builder.Property(e => e.ReviewerName)
                .HasColumnName("reviewer_name")
                .HasMaxLength(150);

            builder.Property(e => e.ReviewerComments)
                .HasColumnName("reviewer_comments");

            builder.Property(e => e.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(e => e.UserId);
        }
    }
}
