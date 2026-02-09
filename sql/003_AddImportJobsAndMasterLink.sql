-- Migration: Add MasterInsuranceCompanyId link to tenant InsuranceCompanies
-- Date: 2026-02-07
-- Description: Links tenant InsuranceCompanies to master InsuranceCompanies
-- Run this against TENANT databases (not TenantDb master)
-- NOTE: ImportJobs table has been moved to master database (see 004_CreateImportJobsInMasterDb.sql)

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

PRINT 'Migration completed successfully';
GO
