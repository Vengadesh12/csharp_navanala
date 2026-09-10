using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserModel>
    {
        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.ToTable("users");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Email)
                .HasColumnName("Email")
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(e => e.Email)
                .IsUnique();

            builder.Property(e => e.PasswordHash)
                .HasColumnName("Password")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.RoleId)
                .HasColumnName("RoleId");

            builder.Property(e => e.DesignationId)
                .HasColumnName("DesignationId");

            builder.Property(e => e.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(50);

            builder.Property(e => e.Age)
                .HasColumnName("Age");

            builder.Property(e => e.Address)
                .HasColumnName("Address")
                .HasMaxLength(500);

            builder.Property(e => e.ProfileImage)
                .HasColumnName("ProfileImage")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("DeletedFlag")
                .HasDefaultValue(1);

            builder.Property(e => e.IsFirstLogin)
                .HasColumnName("IsFirstLogin")
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Ignore(e => e.Password);
        }
    }
}
