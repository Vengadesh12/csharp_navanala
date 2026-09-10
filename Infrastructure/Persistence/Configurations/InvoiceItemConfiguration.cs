using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBackend.Domain.Models;

namespace MyBackend.Infrastructure.Persistence.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItemModel>
    {
        public void Configure(EntityTypeBuilder<InvoiceItemModel> builder)
        {
            builder.ToTable("invoice_items");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.InvoiceId)
                .HasColumnName("invoice_id")
                .IsRequired();

            builder.Property(e => e.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description");

            builder.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(e => e.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(18, 2);

            builder.Property(e => e.TaxRate)
                .HasColumnName("tax_rate")
                .HasPrecision(5, 2);

            builder.Property(e => e.TaxAmount)
                .HasColumnName("tax_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2);

            builder.Property(e => e.OrderIndex)
                .HasColumnName("order_index")
                .HasDefaultValue(0);

            builder.Property(e => e.DeletedFlag)
                .HasColumnName("deleted_flag")
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(e => e.InvoiceId);
        }
    }
}
