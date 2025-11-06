using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
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

            builder.Property(ic => ic.Description)
                .HasMaxLength(1000);

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
}