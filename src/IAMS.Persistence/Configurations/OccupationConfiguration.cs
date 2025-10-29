using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class OccupationConfiguration : IEntityTypeConfiguration<Occupation>
    {
        public void Configure(EntityTypeBuilder<Occupation> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.NameTr)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.NameEn)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.Description)
                .HasMaxLength(500);

            builder.HasIndex(o => o.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Configure relationships
            builder.HasMany(o => o.Customers)
                .WithOne(c => c.Occupation)
                .HasForeignKey(c => c.OccupationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }
}