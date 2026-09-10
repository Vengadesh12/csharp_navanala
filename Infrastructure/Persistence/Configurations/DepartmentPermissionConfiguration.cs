using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class DepartmentPermissionConfiguration : IEntityTypeConfiguration<DepartmentPermissionModel>
    {
        public void Configure(EntityTypeBuilder<DepartmentPermissionModel> builder)
        {
            builder.ToTable("departmentpermissions");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DepartmentId)
                .HasColumnName("DepartmentId")
                .IsRequired();

            builder.Property(e => e.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.HasIndex(e => new { e.DepartmentId, e.PermissionId });

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }
}
