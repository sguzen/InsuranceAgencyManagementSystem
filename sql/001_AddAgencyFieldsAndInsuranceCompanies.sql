-- Migration: Add Agency-specific fields to Tenants table and create AgencyInsuranceCompanies table
-- Date: 2026-02-06
-- Description: Extends TenantEntity with agency management fields and creates join table for insurance companies

-- =====================================================
-- 1. Add new columns to Tenants table
-- =====================================================

-- Add Status column (AgencyStatus enum: 0=Active, 1=Inactive, 2=Suspended, 3=Trial, 4=Expired, 5=PendingSetup)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Status')
BEGIN
    ALTER TABLE [Tenants] ADD [Status] INT NOT NULL DEFAULT 0;
    PRINT 'Added Status column to Tenants table';
END
GO

-- Add SubscriptionType column (enum: 0=Basic, 1=Standard, 2=Premium, 3=Enterprise, 4=Trial)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'SubscriptionType')
BEGIN
    ALTER TABLE [Tenants] ADD [SubscriptionType] INT NOT NULL DEFAULT 0;
    PRINT 'Added SubscriptionType column to Tenants table';
END
GO

-- Add TrialExpiryDate column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'TrialExpiryDate')
BEGIN
    ALTER TABLE [Tenants] ADD [TrialExpiryDate] DATETIME2 NULL;
    PRINT 'Added TrialExpiryDate column to Tenants table';
END
GO

-- Add MaxPolicies column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'MaxPolicies')
BEGIN
    ALTER TABLE [Tenants] ADD [MaxPolicies] INT NOT NULL DEFAULT 1000;
    PRINT 'Added MaxPolicies column to Tenants table';
END
GO

-- Add ModuleSettings column (JSON)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'ModuleSettings')
BEGIN
    ALTER TABLE [Tenants] ADD [ModuleSettings] NVARCHAR(MAX) NULL;
    PRINT 'Added ModuleSettings column to Tenants table';
END
GO

-- Add Settings column (JSON)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Settings')
BEGIN
    ALTER TABLE [Tenants] ADD [Settings] NVARCHAR(MAX) NULL;
    PRINT 'Added Settings column to Tenants table';
END
GO

-- =====================================================
-- 2. Create AgencyInsuranceCompanies table
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AgencyInsuranceCompanies')
BEGIN
    CREATE TABLE [AgencyInsuranceCompanies] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [AgencyId] INT NOT NULL,
        [InsuranceCompanyId] INT NOT NULL,
        [InsuranceCompanyName] NVARCHAR(100) NOT NULL DEFAULT '',
        [InsuranceCompanyCode] NVARCHAR(50) NOT NULL DEFAULT '',
        [DbServer] NVARCHAR(200) NULL,
        [DbName] NVARCHAR(200) NULL,
        [DbUsername] NVARCHAR(200) NULL,
        [DbPassword] NVARCHAR(500) NULL,
        [ConnectionString] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [Notes] NVARCHAR(1000) NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedOn] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(256) NULL,
        [ModifiedBy] NVARCHAR(256) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedOn] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(256) NULL,
        CONSTRAINT [PK_AgencyInsuranceCompanies] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AgencyInsuranceCompanies_Tenants_AgencyId] FOREIGN KEY ([AgencyId]) REFERENCES [Tenants]([Id]) ON DELETE CASCADE
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_AgencyInsuranceCompanies_AgencyId] ON [AgencyInsuranceCompanies]([AgencyId]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AgencyInsuranceCompanies_AgencyId_InsuranceCompanyId] ON [AgencyInsuranceCompanies]([AgencyId], [InsuranceCompanyId]) WHERE [IsDeleted] = 0;

    PRINT 'Created AgencyInsuranceCompanies table with indexes';
END
GO

PRINT 'Migration completed successfully';
GO
