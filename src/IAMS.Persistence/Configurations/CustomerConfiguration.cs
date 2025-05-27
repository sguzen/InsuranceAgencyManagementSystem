using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            builder.Property(c => c.TcNo)
                .IsRequired()
                .HasMaxLength(11);

            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.TcNo)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.TenantId);

            // Configure relationships
            builder.HasMany(c => c.Policies)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.CustomerInsuranceCompanies)
                .WithOne(cic => cic.Customer)
                .HasForeignKey(cic => cic.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete and tenant isolation
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }

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

            builder.HasIndex(p => new { p.CustomerId, p.TenantId });
            builder.HasIndex(p => new { p.InsuranceCompanyId, p.TenantId });
            builder.HasIndex(p => p.TenantId);

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

            // Global query filter for soft delete and tenant isolation
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }

    public class InsuranceCompanyConfiguration : IEntityTypeConfiguration<InsuranceCompany>
    {
        public void Configure(EntityTypeBuilder<InsuranceCompany> builder)
        {
            builder.HasKey(ic => ic.Id);

            builder.Property(ic => ic.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ic => ic.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(ic => ic.ContactPerson)
                .HasMaxLength(100);

            builder.Property(ic => ic.Email)
                .HasMaxLength(200);

            builder.Property(ic => ic.Phone)
                .HasMaxLength(20);

            builder.Property(ic => ic.Address)
                .HasMaxLength(500);

            builder.Property(ic => ic.ApiEndpoint)
                .HasMaxLength(500);

            builder.Property(ic => ic.ApiKey)
                .HasMaxLength(200);

            builder.HasIndex(ic => ic.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(ic => ic.TenantId);

            // Configure relationships
            builder.HasMany(ic => ic.Policies)
                .WithOne(p => p.InsuranceCompany)
                .HasForeignKey(p => p.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ic => ic.CustomerInsuranceCompanies)
                .WithOne(cic => cic.InsuranceCompany)
                .HasForeignKey(cic => cic.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ic => ic.CommissionRates)
                .WithOne(cr => cr.InsuranceCompany)
                .HasForeignKey(cr => cr.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete and tenant isolation
            builder.HasQueryFilter(ic => !ic.IsDeleted);
        }
    }

    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Identifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.ConnectionString)
                .HasMaxLength(1000);

            builder.HasIndex(t => t.Identifier)
                .IsUnique();
        }
    }
}