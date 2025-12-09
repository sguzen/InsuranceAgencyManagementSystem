using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class PolicyPaymentConfiguration : IEntityTypeConfiguration<PolicyPayment>
    {
        public void Configure(EntityTypeBuilder<PolicyPayment> builder)
        {
            builder.HasKey(pp => pp.Id);

            builder.Property(pp => pp.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(pp => pp.PaymentDate)
                .IsRequired();

            builder.Property(pp => pp.DueDate)
                .IsRequired(false);

            builder.Property(pp => pp.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(pp => pp.Reference)
                .HasMaxLength(100);

            builder.Property(pp => pp.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(pp => pp.Notes)
                .HasMaxLength(1000);

            builder.Property(pp => pp.ExchangeRateToBase)
                .HasColumnType("decimal(18,6)");

            builder.Property(pp => pp.AmountInBaseCurrency)
                .HasColumnType("decimal(18,2)");

            // Indexes
            builder.HasIndex(pp => pp.PolicyId);
            builder.HasIndex(pp => pp.PaymentDate);
            builder.HasIndex(pp => pp.Status);
            builder.HasIndex(pp => pp.DueDate);

            // Relationships are configured in PolicyConfiguration
            builder.HasOne(pp => pp.Currency)
                .WithMany()
                .HasForeignKey(pp => pp.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(pp => !pp.IsDeleted);
        }
    }
}
