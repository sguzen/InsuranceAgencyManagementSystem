# Database scripts

## Master database (TenantDb) — automatic from 0005 onwards

Schema changes to the **master** database live in
`src/IAMS.MultiTenancy/Data/Migrations/NNNN_Description.sql`. They are embedded in
`IAMS.MultiTenancy` and applied by `MasterDbMigrator` when **IAMS.Api** starts
(before it serves requests), each script once, in file-name order, in its own
transaction. Applied scripts are recorded in `dbo.__MasterDbMigrations`.

- Write scripts idempotently (`IF NOT EXISTS` guards) — databases patched by hand
  before this mechanism existed are then handled without special-casing.
- Deploy/restart the **API first**; Admin and Web share the master DB but do not migrate it.
- To run scripts out-of-band instead, set `MasterDb:AutoMigrate=false` for the API and
  execute the files manually, then insert a row into `dbo.__MasterDbMigrations`
  (`ScriptName` = `IAMS.MultiTenancy.Data.Migrations.NNNN_Description.sql`).

Useful check after `0005`: links that still rely on the tenant-level fallback code.

```sql
SELECT aic.Id, t.Name AS Agency, aic.InsuranceCompanyName, t.ExternalId AS FallbackAgencyCode
FROM   dbo.AgencyInsuranceCompanies aic
JOIN   dbo.Tenants t ON t.Id = aic.AgencyId
WHERE  aic.IsDeleted = 0 AND aic.AgencyCode IS NULL
  AND  (aic.DbServer IS NOT NULL OR aic.ConnectionString IS NOT NULL)
ORDER BY t.Name, aic.InsuranceCompanyName;
```

## Historical manual scripts (`001`–`004`)

The files in this folder predate the migrator and were run by hand. They are **not**
journaled and are kept for reference / fresh installs only:

| Script | Target |
|---|---|
| `001_AddAgencyFieldsAndInsuranceCompanies.sql` | master |
| `002_CreateInsuranceCompaniesTable.sql` | see header |
| `003_AddImportJobsAndMasterLink.sql` | tenant databases |
| `004_CreateImportJobsInMasterDb.sql` | master |

Tenant-database schema is still managed separately (`TenantDbScript.sql`, `TenantDatabaseService`).
