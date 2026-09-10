using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<ProjectModel>
    {
        public void Configure(EntityTypeBuilder<ProjectModel> builder)
        {
            builder.ToTable("projects");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(e => e.LeadName)
                .HasColumnName("lead_name")
                .HasMaxLength(150);

            builder.Property(e => e.ProgressPercentage)
                .HasColumnName("progress_percentage");

            builder.Property(e => e.DueDate)
                .HasColumnName("due_date");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);
        }
    }
}
