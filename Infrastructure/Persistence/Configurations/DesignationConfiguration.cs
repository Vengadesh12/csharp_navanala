using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class DesignationConfiguration : IEntityTypeConfiguration<DesignationModel>
    {
        public void Configure(EntityTypeBuilder<DesignationModel> builder)
        {
            builder.ToTable("designations");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            builder.Property(e => e.DepartmentId)
                .HasColumnName("DepartmentId");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.HasOne(e => e.Department)
                .WithMany(d => d.Designations)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(e => e.DepartmentId);
        }
    }
}
