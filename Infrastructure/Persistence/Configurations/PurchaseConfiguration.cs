using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<PurchaseModel>
    {
        public void Configure(EntityTypeBuilder<PurchaseModel> builder)
        {
            builder.ToTable("purchases");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ApprovalRequestId)
                .HasColumnName("approval_request_id");

            builder.Property(e => e.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100);

            builder.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(e => e.EstimatedAmount)
                .HasColumnName("estimated_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150);

            builder.Property(e => e.EmployeeEmail)
                .HasColumnName("employee_email")
                .HasMaxLength(150);

            builder.Property(e => e.DepartmentName)
                .HasColumnName("department_name")
                .HasMaxLength(100);

            builder.Property(e => e.VendorName)
                .HasColumnName("vendor_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.VendorContact)
                .HasColumnName("vendor_contact")
                .HasMaxLength(100);

            builder.Property(e => e.VendorEmail)
                .HasColumnName("vendor_email")
                .HasMaxLength(150);

            builder.Property(e => e.QuotationNumber)
                .HasColumnName("quotation_number")
                .HasMaxLength(100);

            builder.Property(e => e.QuotationAmount)
                .HasColumnName("quotation_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.QuotationDate)
                .HasColumnName("quotation_date");

            builder.Property(e => e.DeliveryTimeline)
                .HasColumnName("delivery_timeline")
                .HasMaxLength(100);

            builder.Property(e => e.PaymentTerms)
                .HasColumnName("payment_terms")
                .HasMaxLength(200);

            builder.Property(e => e.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(e => e.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasIndex(e => e.CreatedByUserId);
        }
    }
}
