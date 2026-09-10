using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSettingModel>
    {
        public void Configure(EntityTypeBuilder<SystemSettingModel> builder)
        {
            builder.ToTable("system_settings");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.SettingKey)
                .HasColumnName("setting_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.SettingKey)
                .IsUnique();

            builder.Property(e => e.SettingValue)
                .HasColumnName("setting_value");

            builder.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.DataType)
                .HasColumnName("data_type")
                .HasMaxLength(50);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(150);
        }
    }
}
