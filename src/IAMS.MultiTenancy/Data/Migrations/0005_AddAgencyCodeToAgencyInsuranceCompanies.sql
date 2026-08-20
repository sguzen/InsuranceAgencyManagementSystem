-- Migration: Add per-insurer AgencyCode to AgencyInsuranceCompanies
-- Date: 2026-08-20
-- Target: MASTER database (TenantDb). Applied automatically at API startup by MasterDbMigrator
--         and journaled in dbo.__MasterDbMigrations.
-- Description: Each insurance company assigns the agency its own code ("ackod" in the
--              insurer's policy database). The MySQL policy import filters on this code,
--              so it must be stored per agency-insurance company link rather than once
--              per agency (Tenants.ExternalId).
--
-- Behaviour after deploy: links with a NULL AgencyCode fall back to Tenants.ExternalId
-- (previous behaviour) and log a warning. Set the correct code for each link in the
-- admin panel (Acentalar > Sigorta Sirketleri > Duzenle > "Acenta Kodu").

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AgencyInsuranceCompanies') AND name = 'AgencyCode')
BEGIN
    ALTER TABLE [dbo].[AgencyInsuranceCompanies] ADD [AgencyCode] NVARCHAR(10) NULL;
END
GO
