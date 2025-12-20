using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
    {
        public void Configure(EntityTypeBuilder<CustomerPayment> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(cp => cp.AllocatedAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(cp => cp.ExchangeRateToBase)
                .HasColumnType("decimal(18,6)");

            builder.Property(cp => cp.AmountInBaseCurrency)
                .HasColumnType("decimal(18,2)");

            builder.Property(cp => cp.Reference)
                .HasMaxLength(100);

            builder.Property(cp => cp.Notes)
                .HasMaxLength(1000);

            builder.Property(cp => cp.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(cp => cp.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Domain.Enums.PaymentStatus.Completed);

            builder.Property(cp => cp.AllocationStatus)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Domain.Enums.AllocationStatus.Unallocated);

            // Indexes
            builder.HasIndex(cp => cp.CustomerId);
            builder.HasIndex(cp => cp.PaymentDate);
            builder.HasIndex(cp => cp.CurrencyId);
            builder.HasIndex(cp => cp.Status);
            builder.HasIndex(cp => cp.AllocationStatus);

            // Relationships
            builder.HasOne(cp => cp.Customer)
                .WithMany()
                .HasForeignKey(cp => cp.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cp => cp.Currency)
                .WithMany()
                .HasForeignKey(cp => cp.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cp => cp.PaymentAllocations)
                .WithOne(pa => pa.CustomerPayment)
                .HasForeignKey(pa => pa.CustomerPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            builder.HasQueryFilter(cp => !cp.IsDeleted);
        }
    }
}
