using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceModel>
    {
        public void Configure(EntityTypeBuilder<InvoiceModel> builder)
        {
            builder.ToTable("invoices");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.InvoiceNumber)
                .HasColumnName("invoice_number")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(e => e.InvoiceNumber)
                .IsUnique();

            builder.Property(e => e.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(150);

            builder.Property(e => e.CustomerPhone)
                .HasColumnName("customer_phone")
                .HasMaxLength(50);

            builder.Property(e => e.CustomerAddress)
                .HasColumnName("customer_address");

            builder.Property(e => e.CustomerGstin)
                .HasColumnName("customer_gstin")
                .HasMaxLength(50);

            builder.Property(e => e.CompanyGstin)
                .HasColumnName("company_gstin")
                .HasMaxLength(50);

            builder.Property(e => e.InvoiceDate)
                .HasColumnName("invoice_date")
                .IsRequired();

            builder.Property(e => e.DueDate)
                .HasColumnName("due_date");

            builder.Property(e => e.Subtotal)
                .HasColumnName("subtotal")
                .HasPrecision(18, 2);

            builder.Property(e => e.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(e => e.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalAmountInWords)
                .HasColumnName("total_amount_in_words")
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            builder.Property(e => e.PaymentMethod)
                .HasColumnName("payment_method")
                .HasMaxLength(50);

            builder.Property(e => e.Notes)
                .HasColumnName("notes");

            builder.Property(e => e.TermsAndConditions)
                .HasColumnName("terms_and_conditions");

            builder.Property(e => e.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            builder.Property(e => e.CreatedByName)
                .HasColumnName("created_by_name")
                .HasMaxLength(150);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.HasMany(e => e.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
