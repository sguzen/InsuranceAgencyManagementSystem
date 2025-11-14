using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.HasKey(ii => ii.Id);

            builder.Property(ii => ii.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ii => ii.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(ii => ii.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(ii => ii.Notes)
                .HasMaxLength(500);

            // Relationship configured in InvoiceConfiguration

            // Global query filter for soft delete
            builder.HasQueryFilter(ii => !ii.IsDeleted);
        }
    }
}
