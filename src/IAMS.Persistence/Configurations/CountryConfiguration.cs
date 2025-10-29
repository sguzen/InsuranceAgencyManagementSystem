using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.NameTr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.NameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.PhoneCode)
                .HasMaxLength(10);

            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasMany(c => c.CustomersWithNationality)
                .WithOne(cu => cu.NationalityCountry)
                .HasForeignKey(cu => cu.NationalityCountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}