using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
    {
        public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
        {
            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.AllocatedAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(pa => pa.AllocationDate)
                .IsRequired();

            builder.Property(pa => pa.AllocationType)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Domain.Enums.AllocationType.Automatic);

            builder.Property(pa => pa.Notes)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(pa => pa.CustomerPaymentId);
            builder.HasIndex(pa => pa.PolicyId);
            builder.HasIndex(pa => pa.AllocationDate);

            // Relationships
            builder.HasOne(pa => pa.CustomerPayment)
                .WithMany(cp => cp.PaymentAllocations)
                .HasForeignKey(pa => pa.CustomerPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pa => pa.Policy)
                .WithMany()
                .HasForeignKey(pa => pa.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(pa => !pa.IsDeleted);
        }
    }
}
