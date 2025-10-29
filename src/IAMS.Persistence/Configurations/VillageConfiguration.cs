using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class VillageConfiguration : IEntityTypeConfiguration<Village>
    {
        public void Configure(EntityTypeBuilder<Village> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(v => v.NameTr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.NameEn)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(v => new { v.SubdistrictId, v.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasOne(v => v.Subdistrict)
                .WithMany(s => s.Villages)
                .HasForeignKey(v => v.SubdistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(v => v.Customers)
                .WithOne(cu => cu.Village)
                .HasForeignKey(cu => cu.VillageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(v => !v.IsDeleted);
        }
    }
}