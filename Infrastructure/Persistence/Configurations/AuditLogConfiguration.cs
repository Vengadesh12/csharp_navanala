using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogModel>
    {
        public void Configure(EntityTypeBuilder<AuditLogModel> builder)
        {
            builder.ToTable("audit_logs");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Action)
                .HasColumnName("action")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Module)
                .HasColumnName("module")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.PerformedBy)
                .HasColumnName("performed_by")
                .HasMaxLength(150);

            builder.Property(e => e.Details)
                .HasColumnName("details");

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }
}
