using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PolicyNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.PremiumAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CommissionAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CommissionRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(p => p.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(p => p.PolicyNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(p => new { p.CustomerId});
            builder.HasIndex(p => new { p.InsuranceCompanyId });

            // Configure relationships
            builder.HasOne(p => p.Customer)
                .WithMany(c => c.Policies)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.InsuranceCompany)
                .WithMany(ic => ic.Policies)
                .HasForeignKey(p => p.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PolicyType)
                .WithMany(pt => pt.Policies)
                .HasForeignKey(p => p.PolicyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.PolicyPayments)
                .WithOne(pp => pp.Policy)
                .HasForeignKey(pp => pp.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PolicyClaims)
                .WithOne(pc => pc.Policy)
                .HasForeignKey(pc => pc.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing relationship for policy renewals
            builder.HasOne(p => p.ParentPolicy)
                .WithMany()
                .HasForeignKey(p => p.ParentPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.ParentPolicyId);

            // Self-referencing relationship for endorsements
            builder.HasOne(p => p.OriginalPolicy)
                .WithMany()
                .HasForeignKey(p => p.OriginalPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.OriginalPolicyId);

            // Endorsement/StateType fields (all policies are treated as endorsements now)
            builder.Property(p => p.InnerCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("000");

            builder.Property(p => p.StateType)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(p => p.InnerCode);
            builder.HasIndex(p => new { p.PolicyNumber, p.InnerCode });

            builder.Property(p => p.BranchCode)
                .HasMaxLength(50);

            // Driver fields
            builder.Property(p => p.DriverType)
                .HasConversion<int?>();

            // Marketer stored as string
            builder.Property(p => p.Marketer)
                .HasMaxLength(200);

            // Additional indexes for legacy fields
            builder.HasIndex(p => p.BranchCode);

            // Global query filter for soft delete and tenant isolation
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}