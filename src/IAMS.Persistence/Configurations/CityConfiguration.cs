using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
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

            builder.HasIndex(c => c.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasMany(c => c.Customers)
                .WithOne(cu => cu.City)
                .HasForeignKey(cu => cu.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Districts)
                .WithOne(d => d.City)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}