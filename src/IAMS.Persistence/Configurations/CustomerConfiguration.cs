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

            // Basic Information
            builder.Property(c => c.CustomerCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            // Address fields
            builder.Property(c => c.Address1)
                .HasMaxLength(500);

            builder.Property(c => c.Address2)
                .HasMaxLength(500);

            // Contact Information
            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.MobilePhoneCountryCode)
                .HasMaxLength(10);

            builder.Property(c => c.MobilePhoneNumber)
                .HasMaxLength(20);

            builder.Property(c => c.HomePhone)
                .HasMaxLength(20);

            // Identity Information
            builder.Property(c => c.IdentificationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Notes)
                .HasMaxLength(2000);

            // Legacy fields (deprecated)
            builder.Property(c => c.IdentificationNumber)
                .HasMaxLength(11);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            // Indexes
            builder.HasIndex(c => c.CustomerCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(c => new { c.IdentificationType, c.IdentificationNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships - Parametric tables
            builder.HasOne(c => c.City)
                .WithMany(ci => ci.Customers)
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.District)
                .WithMany(d => d.Customers)
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Subdistrict)
                .WithMany(s => s.Customers)
                .HasForeignKey(c => c.SubdistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Village)
                .WithMany(v => v.Customers)
                .HasForeignKey(c => c.VillageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Occupation)
                .WithMany(o => o.Customers)
                .HasForeignKey(c => c.OccupationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.NationalityCountry)
                .WithMany(co => co.CustomersWithNationality)
                .HasForeignKey(c => c.NationalityCountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships - Domain entities
            builder.HasMany(c => c.Policies)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.CustomerInsuranceCompanies)
                .WithOne(cic => cic.Customer)
                .HasForeignKey(cic => cic.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore CustomerPayments navigation property to avoid shadow foreign key
            builder.Ignore(c => c.CustomerPayments);

            // Global query filter for soft delete
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}