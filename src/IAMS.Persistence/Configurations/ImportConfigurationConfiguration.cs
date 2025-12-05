using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class ImportConfigurationConfiguration : IEntityTypeConfiguration<ImportConfiguration>
    {
        public void Configure(EntityTypeBuilder<ImportConfiguration> builder)
        {
            builder.HasKey(ic => ic.Id);

            builder.Property(ic => ic.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ic => ic.Description)
                .HasMaxLength(1000);

            builder.Property(ic => ic.SourceType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(ic => ic.ApiBaseUrl)
                .HasMaxLength(500);

            builder.Property(ic => ic.ApiKey)
                .HasMaxLength(500);

            builder.Property(ic => ic.ApiUsername)
                .HasMaxLength(200);

            builder.Property(ic => ic.ApiPassword)
                .HasMaxLength(500);

            builder.Property(ic => ic.PoliciesEndpoint)
                .HasMaxLength(500);

            builder.Property(ic => ic.LastSyncStatus)
                .HasMaxLength(100);

            // Relationship with InsuranceCompany
            builder.HasOne(ic => ic.InsuranceCompany)
                .WithMany()
                .HasForeignKey(ic => ic.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(ic => ic.Name).IsUnique();
            builder.HasIndex(ic => ic.InsuranceCompanyId);
            builder.HasIndex(ic => ic.SourceType);
            builder.HasIndex(ic => ic.IsActive);
            builder.HasIndex(ic => ic.EnableAutoSync);

            // Global query filter for soft delete
            builder.HasQueryFilter(ic => !ic.IsDeleted);
        }
    }
}
