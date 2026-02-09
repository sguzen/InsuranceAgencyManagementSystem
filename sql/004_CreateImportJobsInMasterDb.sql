-- Migration: Create ImportJobs table in Master Database (TenantDb)
-- Date: 2026-02-09
-- Description: Creates ImportJobs table in master database for background service access
-- Run this against the MASTER database (TenantDb), NOT tenant databases

-- =====================================================
-- Create ImportJobs table in Master Database
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportJobs')
BEGIN
    CREATE TABLE [ImportJobs] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [AgencyId] INT NOT NULL,
        [InsuranceCompanyId] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [StartedAt] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        [TotalRecords] INT NOT NULL DEFAULT 0,
        [ImportedRecords] INT NOT NULL DEFAULT 0,
        [FailedRecords] INT NOT NULL DEFAULT 0,
        [SkippedRecords] INT NOT NULL DEFAULT 0,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [ImportLog] NVARCHAR(MAX) NULL,
        [FilterStartDate] DATETIME2 NULL,
        [FilterEndDate] DATETIME2 NULL,
        [RequestedBy] NVARCHAR(256) NOT NULL DEFAULT '',
        [CreatedBy] NVARCHAR(256) NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy] NVARCHAR(256) NULL,
        [ModifiedOn] DATETIME2 NULL,
        CONSTRAINT [PK_ImportJobs] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ImportJobs_Tenants] FOREIGN KEY ([AgencyId])
            REFERENCES [Tenants]([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_ImportJobs_InsuranceCompanies] FOREIGN KEY ([InsuranceCompanyId])
            REFERENCES [InsuranceCompanies]([Id]) ON DELETE RESTRICT
    );

    -- Create indexes for efficient querying
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_Status] ON [ImportJobs]([Status]) INCLUDE ([CreatedOn]);
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_AgencyId] ON [ImportJobs]([AgencyId]);
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_InsuranceCompanyId] ON [ImportJobs]([InsuranceCompanyId]);
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_AgencyId_Status] ON [ImportJobs]([AgencyId], [Status]);

    PRINT 'Created ImportJobs table in master database with indexes';
END
GO

PRINT 'Migration completed successfully';
GO
