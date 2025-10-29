using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class DistrictConfiguration : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(d => d.NameTr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.NameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(d => new { d.CityId, d.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasOne(d => d.City)
                .WithMany(c => c.Districts)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Customers)
                .WithOne(cu => cu.District)
                .HasForeignKey(cu => cu.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Subdistricts)
                .WithOne(s => s.District)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}