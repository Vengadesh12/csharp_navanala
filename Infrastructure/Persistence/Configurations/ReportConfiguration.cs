using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<ReportModel>
    {
        public void Configure(EntityTypeBuilder<ReportModel> builder)
        {
            builder.ToTable("reports");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            builder.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(e => e.Format)
                .HasColumnName("format")
                .HasMaxLength(50);

            builder.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(150);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.FileSize)
                .HasColumnName("file_size")
                .HasMaxLength(50);

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasIndex(e => e.CategoryId);
        }
    }
}
