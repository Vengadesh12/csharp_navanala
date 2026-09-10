using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSessionModel>
    {
        public void Configure(EntityTypeBuilder<UserSessionModel> builder)
        {
            builder.ToTable("user_sessions");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150);

            builder.Property(e => e.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100);

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(50);

            builder.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("text");

            builder.Property(e => e.LoginTime)
                .HasColumnName("login_time")
                .IsRequired();

            builder.Property(e => e.LogoutTime)
                .HasColumnName("logout_time");

            builder.Property(e => e.SessionToken)
                .HasColumnName("session_token")
                .HasColumnType("text");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(e => e.UserId);
        }
    }
}
