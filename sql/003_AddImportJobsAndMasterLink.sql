-- Migration: Add ImportJobs table and MasterInsuranceCompanyId link
-- Date: 2026-02-07
-- Description: Adds secure policy import infrastructure to tenant database
-- Run this against TENANT databases (not TenantDb master)

-- =====================================================
-- 1. Add MasterInsuranceCompanyId to InsuranceCompanies
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InsuranceCompanies') AND name = 'MasterInsuranceCompanyId')
BEGIN
    ALTER TABLE [InsuranceCompanies] ADD [MasterInsuranceCompanyId] INT NULL;
    PRINT 'Added MasterInsuranceCompanyId column to InsuranceCompanies table';
END
GO

-- Create index for faster lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InsuranceCompanies_MasterInsuranceCompanyId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_InsuranceCompanies_MasterInsuranceCompanyId]
    ON [InsuranceCompanies]([MasterInsuranceCompanyId])
    WHERE [MasterInsuranceCompanyId] IS NOT NULL;
    PRINT 'Created index on MasterInsuranceCompanyId';
END
GO

-- =====================================================
-- 2. Remove deprecated columns from InsuranceCompanies
-- (ApiEndpoint, ApiKey, IntegrationSettings moved to master DB)
-- =====================================================

-- Note: Only remove if you're sure they're not being used
-- Uncomment if ready to remove:
-- IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InsuranceCompanies') AND name = 'ApiEndpoint')
-- BEGIN
--     ALTER TABLE [InsuranceCompanies] DROP COLUMN [ApiEndpoint];
--     PRINT 'Removed ApiEndpoint column';
-- END
-- GO

-- IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InsuranceCompanies') AND name = 'ApiKey')
-- BEGIN
--     ALTER TABLE [InsuranceCompanies] DROP COLUMN [ApiKey];
--     PRINT 'Removed ApiKey column';
-- END
-- GO

-- IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InsuranceCompanies') AND name = 'IntegrationSettings')
-- BEGIN
--     ALTER TABLE [InsuranceCompanies] DROP COLUMN [IntegrationSettings];
--     PRINT 'Removed IntegrationSettings column';
-- END
-- GO

-- =====================================================
-- 3. Create ImportJobs table
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportJobs')
BEGIN
    CREATE TABLE [ImportJobs] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [InsuranceCompanyId] INT NOT NULL,
        [MasterInsuranceCompanyId] INT NOT NULL,
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
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ImportJobs] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ImportJobs_InsuranceCompanies] FOREIGN KEY ([InsuranceCompanyId])
            REFERENCES [InsuranceCompanies]([Id]) ON DELETE CASCADE
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_Status] ON [ImportJobs]([Status]) INCLUDE ([CreatedOn]);
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_InsuranceCompanyId] ON [ImportJobs]([InsuranceCompanyId]);
    CREATE NONCLUSTERED INDEX [IX_ImportJobs_MasterInsuranceCompanyId] ON [ImportJobs]([MasterInsuranceCompanyId]);

    PRINT 'Created ImportJobs table with indexes';
END
GO

PRINT 'Migration completed successfully';
GO
