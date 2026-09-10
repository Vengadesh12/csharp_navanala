using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<PermissionModel>
    {
        public void Configure(EntityTypeBuilder<PermissionModel> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.PermissionKey)
                .HasColumnName("PermissionKey")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.PermissionKey)
                .IsUnique();

            builder.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }
}
