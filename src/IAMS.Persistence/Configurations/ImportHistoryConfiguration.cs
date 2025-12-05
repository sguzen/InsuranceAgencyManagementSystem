using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMS.Persistence.Configurations
{
    public class ImportHistoryConfiguration : IEntityTypeConfiguration<ImportHistory>
    {
        public void Configure(EntityTypeBuilder<ImportHistory> builder)
        {
            builder.HasKey(ih => ih.Id);

            builder.Property(ih => ih.SourceType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(ih => ih.FileName)
                .HasMaxLength(500);

            builder.Property(ih => ih.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ih => ih.ImportedBy)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ih => ih.Notes)
                .HasMaxLength(2000);

            // Relationships
            builder.HasOne(ih => ih.ImportConfiguration)
                .WithMany(ic => ic.ImportHistories)
                .HasForeignKey(ih => ih.ImportConfigurationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ih => ih.InsuranceCompany)
                .WithMany()
                .HasForeignKey(ih => ih.InsuranceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(ih => ih.SourceType);
            builder.HasIndex(ih => ih.ImportConfigurationId);
            builder.HasIndex(ih => ih.InsuranceCompanyId);
            builder.HasIndex(ih => ih.StartedAt);
            builder.HasIndex(ih => ih.Status);
            builder.HasIndex(ih => new { ih.StartedAt, ih.Status });

            // Global query filter for soft delete
            builder.HasQueryFilter(ih => !ih.IsDeleted);
        }
    }
}
