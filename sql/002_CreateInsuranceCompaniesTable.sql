-- Migration: Create InsuranceCompanies master table
-- Date: 2026-02-06
-- Description: Creates the master InsuranceCompanies table and seeds Turkish insurance companies

-- =====================================================
-- 1. Create InsuranceCompanies table
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InsuranceCompanies')
BEGIN
    CREATE TABLE [InsuranceCompanies] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Code] NVARCHAR(50) NOT NULL,
        [LogoUrl] NVARCHAR(500) NULL,
        [Website] NVARCHAR(200) NULL,
        [ApiEndpoint] NVARCHAR(200) NULL,
        [IntegrationSettings] NVARCHAR(1000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedOn] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(256) NULL,
        [ModifiedBy] NVARCHAR(256) NULL,
        CONSTRAINT [PK_InsuranceCompanies] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_InsuranceCompanies_Code] UNIQUE ([Code])
    );

    PRINT 'Created InsuranceCompanies table';
END
GO

-- =====================================================
-- 2. Add FK from AgencyInsuranceCompanies to InsuranceCompanies
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AgencyInsuranceCompanies_InsuranceCompanies')
BEGIN
    -- First insert the insurance companies so we have valid FK targets
    -- Then add the FK constraint
    PRINT 'FK will be added after seeding data';
END
GO

-- =====================================================
-- 3. Seed Turkish Insurance Companies
-- =====================================================

-- Only insert if table is empty
IF NOT EXISTS (SELECT 1 FROM [InsuranceCompanies])
BEGIN
    SET IDENTITY_INSERT [InsuranceCompanies] ON;

    INSERT INTO [InsuranceCompanies] ([Id], [Name], [Code], [IsActive], [DisplayOrder], [CreatedOn])
    VALUES
        (1, N'Anadolu Sigorta', 'ANADOLU', 1, 1, '2024-01-01'),
        (2, N'Allianz Sigorta', 'ALLIANZ', 1, 2, '2024-01-01'),
        (3, N'Axa Sigorta', 'AXA', 1, 3, '2024-01-01'),
        (4, N'Aksigorta', 'AKSIGORTA', 1, 4, '2024-01-01'),
        (5, N'Sompo Sigorta', 'SOMPO', 1, 5, '2024-01-01'),
        (6, N'Mapfre Sigorta', 'MAPFRE', 1, 6, '2024-01-01'),
        (7, N'HDI Sigorta', 'HDI', 1, 7, '2024-01-01'),
        (8, N'Zurich Sigorta', 'ZURICH', 1, 8, '2024-01-01'),
        (9, N'Groupama Sigorta', 'GROUPAMA', 1, 9, '2024-01-01'),
        (10, N'Turkiye Sigorta', 'TURKIYE', 1, 10, '2024-01-01'),
        (11, N'Generali Sigorta', 'GENERALI', 1, 11, '2024-01-01'),
        (12, N'Doga Sigorta', 'DOGA', 1, 12, '2024-01-01'),
        (13, N'Neova Sigorta', 'NEOVA', 1, 13, '2024-01-01'),
        (14, N'Quick Sigorta', 'QUICK', 1, 14, '2024-01-01'),
        (15, N'Hepiyi Sigorta', 'HEPIYI', 1, 15, '2024-01-01'),
        (16, N'Magdeburger Sigorta', 'MAGDEBURGER', 1, 16, '2024-01-01'),
        (17, N'Koru Sigorta', 'KORU', 1, 17, '2024-01-01'),
        (18, N'Turk Nippon Sigorta', 'TURKNIPPON', 1, 18, '2024-01-01'),
        (19, N'Corpus Sigorta', 'CORPUS', 1, 19, '2024-01-01'),
        (20, N'Orient Sigorta', 'ORIENT', 1, 20, '2024-01-01');

    SET IDENTITY_INSERT [InsuranceCompanies] OFF;

    PRINT 'Seeded 20 Turkish insurance companies';
END
GO

-- =====================================================
-- 4. Add FK constraint (after data is seeded)
-- =====================================================

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AgencyInsuranceCompanies')
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InsuranceCompanies')
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AgencyInsuranceCompanies_InsuranceCompanies')
BEGIN
    ALTER TABLE [AgencyInsuranceCompanies]
    ADD CONSTRAINT [FK_AgencyInsuranceCompanies_InsuranceCompanies]
    FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies]([Id]);

    PRINT 'Added FK from AgencyInsuranceCompanies to InsuranceCompanies';
END
GO

PRINT 'Migration completed successfully';
GO
