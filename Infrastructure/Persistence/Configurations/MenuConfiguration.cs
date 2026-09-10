using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class MenuConfiguration : IEntityTypeConfiguration<MenuModel>
    {
        public void Configure(EntityTypeBuilder<MenuModel> builder)
        {
            builder.ToTable("menus");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.MenuKey)
                .HasColumnName("menukey")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.MenuKey)
                .IsUnique();

            builder.Property(e => e.Label)
                .HasColumnName("label")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Icon)
                .HasColumnName("icon")
                .HasMaxLength(50);

            builder.Property(e => e.Route)
                .HasColumnName("route")
                .HasMaxLength(200);

            builder.Property(e => e.GroupName)
                .HasColumnName("groupname")
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.OrderIndex)
                .HasColumnName("orderindex")
                .HasDefaultValue(0);

            builder.Property(e => e.PermissionKey)
                .HasColumnName("permissionkey")
                .HasMaxLength(100);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deletedflag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}
