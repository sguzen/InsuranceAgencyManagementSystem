using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class SubdistrictConfiguration : IEntityTypeConfiguration<Subdistrict>
    {
        public void Configure(EntityTypeBuilder<Subdistrict> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(s => s.NameTr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.NameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(s => new { s.DistrictId, s.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasOne(s => s.District)
                .WithMany(d => d.Subdistricts)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Customers)
                .WithOne(cu => cu.Subdistrict)
                .HasForeignKey(cu => cu.SubdistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Villages)
                .WithOne(v => v.Subdistrict)
                .HasForeignKey(v => v.SubdistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}