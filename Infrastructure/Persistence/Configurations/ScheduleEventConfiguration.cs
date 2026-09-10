using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class ScheduleEventConfiguration : IEntityTypeConfiguration<ScheduleEventModel>
    {
        public void Configure(EntityTypeBuilder<ScheduleEventModel> builder)
        {
            builder.ToTable("schedules");

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
                .HasMaxLength(1000);

            builder.Property(e => e.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(50);

            builder.Property(e => e.EventDate)
                .HasColumnName("event_date");

            builder.Property(e => e.StartTime)
                .HasColumnName("start_time")
                .HasMaxLength(50);

            builder.Property(e => e.EndTime)
                .HasColumnName("end_time")
                .HasMaxLength(50);

            builder.Property(e => e.Location)
                .HasColumnName("location")
                .HasMaxLength(200);

            builder.Property(e => e.Organizer)
                .HasColumnName("organizer")
                .HasMaxLength(150);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasMaxLength(50);

            builder.Property(e => e.AttendeesCount)
                .HasColumnName("attendees_count");

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
