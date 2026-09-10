using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionModel>
    {
        public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
        {
            builder.ToTable("rolepermissions");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(e => e.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(e => e.Access)
                .HasColumnName("Access")
                .HasMaxLength(10)
                .HasDefaultValue("Allow")
                .IsRequired();

            builder.HasIndex(e => new { e.RoleId, e.PermissionId });

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }
}
