using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermissionModel>
    {
        public void Configure(EntityTypeBuilder<UserPermissionModel> builder)
        {
            builder.ToTable("userpermissions");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(e => e.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.HasIndex(e => new { e.UserId, e.PermissionId });

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }
}
