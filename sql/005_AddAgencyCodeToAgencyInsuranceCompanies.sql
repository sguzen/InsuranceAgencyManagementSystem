-- Migration: Add per-insurer AgencyCode to AgencyInsuranceCompanies
-- Date: 2026-08-20
-- Description: Each insurance company assigns the agency its own code ("ackod" in the
--              insurer's policy database). The MySQL policy import filters on this code,
--              so it must be stored per agency-insurance company link rather than once
--              per agency (Tenants.ExternalId).
-- Run this against the MASTER database (TenantDb), NOT tenant databases
--
-- Behaviour after deploy: links with a NULL AgencyCode fall back to Tenants.ExternalId
-- (previous behaviour) and log a warning. Set the correct code for each link in the
-- admin panel (Acentalar > Sigorta Sirketleri > Duzenle > "Acenta Kodu").

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AgencyInsuranceCompanies') AND name = 'AgencyCode')
BEGIN
    ALTER TABLE [AgencyInsuranceCompanies] ADD [AgencyCode] NVARCHAR(10) NULL;
    PRINT 'Added AgencyCode column to AgencyInsuranceCompanies table';
END
ELSE
BEGIN
    PRINT 'AgencyCode column already exists on AgencyInsuranceCompanies';
END
GO

-- Report links that still rely on the tenant-level fallback so they can be fixed up.
SELECT aic.Id            AS AgencyInsuranceCompanyId,
       t.Name            AS Agency,
       aic.InsuranceCompanyName,
       t.ExternalId      AS FallbackAgencyCode
FROM   [AgencyInsuranceCompanies] aic
JOIN   [Tenants] t ON t.Id = aic.AgencyId
WHERE  aic.IsDeleted = 0
  AND  aic.AgencyCode IS NULL
  AND  (aic.DbServer IS NOT NULL OR aic.ConnectionString IS NOT NULL)
ORDER BY t.Name, aic.InsuranceCompanyName;
GO
