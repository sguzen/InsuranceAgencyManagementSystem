using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.SubtotalAmount)
                .HasPrecision(18, 2);

            builder.Property(i => i.TaxAmount)
                .HasPrecision(18, 2);

            builder.Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(i => i.TaxRate)
                .HasPrecision(5, 2);

            builder.Property(i => i.Notes)
                .HasMaxLength(1000);

            builder.Property(i => i.PaymentReference)
                .HasMaxLength(100);

            // Unique constraint on invoice number
            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships with Restrict to avoid cascade path conflicts
            builder.HasOne(i => i.Policy)
                .WithMany()
                .HasForeignKey(i => i.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Currency)
                .WithMany()
                .HasForeignKey(i => i.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.InvoiceItems)
                .WithOne(ii => ii.Invoice)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }
}
