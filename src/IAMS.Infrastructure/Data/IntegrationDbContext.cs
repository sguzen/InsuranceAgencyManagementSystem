// IAMS.Infrastructure/Data/IntegrationDbContext.cs
using Microsoft.EntityFrameworkCore;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.Data
{
    public class IntegrationDbContext : DbContext
    {
        public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options)
        {
        }

        public DbSet<IntegrationLogEntity> IntegrationLogs { get; set; }
        public DbSet<ScheduledReportEntity> ScheduledReports { get; set; }
        public DbSet<FileMetadataEntity> FileMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure IntegrationLog entity
            modelBuilder.Entity<IntegrationLogEntity>(entity =>
            {
                entity.ToTable("IntegrationLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.HasIndex(e => new { e.Provider, e.CreatedAt });
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure ScheduledReport entity
            modelBuilder.Entity<ScheduledReportEntity>(entity =>
            {
                entity.ToTable("ScheduledReports");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ReportType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CronExpression).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmailRecipients).HasMaxLength(1000);
                entity.Property(e => e.Parameters).HasColumnType("nvarchar(max)");
            });

            // Configure FileMetadata entity
            modelBuilder.Entity<FileMetadataEntity>(entity =>
            {
                entity.ToTable("FileMetadata");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.StoredPath).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.UploadedBy).HasMaxLength(100);
                entity.HasIndex(e => e.StoredPath).IsUnique();
                entity.HasIndex(e => e.UploadedAt);
            });
        }
    }

    public class IntegrationLogEntity
    {
        public int Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
    }

    public class ScheduledReportEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty; // JSON
        public string CronExpression { get; set; } = string.Empty;
        public string EmailRecipients { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class FileMetadataEntity
    {
        public int Id { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string StoredPath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsDeleted { get; set; }
    }
}