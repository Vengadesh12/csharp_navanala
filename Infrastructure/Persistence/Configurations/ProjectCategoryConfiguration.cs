using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class ProjectCategoryConfiguration : IEntityTypeConfiguration<ProjectCategoryModel>
    {
        public void Configure(EntityTypeBuilder<ProjectCategoryModel> builder)
        {
            builder.ToTable("project_categories");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}
