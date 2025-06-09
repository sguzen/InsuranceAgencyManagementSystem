namespace IAMS.Application.Constants
{
    public static class PermissionNames
    {
        // Customer permissions
        public const string ViewCustomers = "customers.view";
        public const string CreateCustomers = "customers.create";
        public const string EditCustomers = "customers.edit";
        public const string DeleteCustomers = "customers.delete";

        // Policy permissions
        public const string ViewPolicies = "policies.view";
        public const string CreatePolicies = "policies.create";
        public const string EditPolicies = "policies.edit";
        public const string DeletePolicies = "policies.delete";
        public const string RenewPolicies = "policies.renew";

        // Insurance Company permissions
        public const string ViewInsuranceCompanies = "companies.view";
        public const string CreateInsuranceCompanies = "companies.create";
        public const string EditInsuranceCompanies = "companies.edit";
        public const string DeleteInsuranceCompanies = "companies.delete";

        // Payment permissions
        public const string ViewPayments = "payments.view";
        public const string CreatePayments = "payments.create";
        public const string EditPayments = "payments.edit";
        public const string DeletePayments = "payments.delete";

        // Claim permissions
        public const string ViewClaims = "claims.view";
        public const string CreateClaims = "claims.create";
        public const string EditClaims = "claims.edit";
        public const string DeleteClaims = "claims.delete";

        // Reporting permissions (module-specific)
        public const string ViewReports = "reports.view";
        public const string CreateReports = "reports.create";
        public const string ExportReports = "reports.export";

        // Accounting permissions (module-specific)
        public const string ViewAccounting = "accounting.view";
        public const string ManageCommissions = "accounting.commissions";
        public const string ViewFinancials = "accounting.financials";

        // Integration permissions (module-specific)
        public const string ViewIntegrations = "integrations.view";
        public const string ManageIntegrations = "integrations.manage";
        public const string SyncData = "integrations.sync";

        // Admin permissions
        public const string ManageUsers = "admin.users";
        public const string ManageRoles = "admin.roles";
        public const string ManagePermissions = "admin.permissions";
        public const string ManageSettings = "admin.settings";
        public const string ManageTenant = "admin.tenant";
        public const string ManageModules = "admin.modules";
    }
}