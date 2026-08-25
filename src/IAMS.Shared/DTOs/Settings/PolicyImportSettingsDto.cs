namespace IAMS.Shared.DTOs.Settings
{
    /// <summary>
    /// Per-agency policy-import settings, stored in the tenant's own database
    /// (TenantSettings table) under <see cref="SettingKey"/>.
    /// </summary>
    public class PolicyImportSettingsDto
    {
        public const string SettingKey = "policyImport";

        /// <summary>
        /// When true (default, the legally correct behavior), importing a traffic-family
        /// policy (Trafik/Traffic or any Kasko variant) automatically creates a completed
        /// payment for the full premium, so the policy carries no outstanding balance.
        /// Agencies that track real payments for these policies can turn this off;
        /// the change only affects imports started afterwards.
        /// </summary>
        public bool AutoPayMandatoryPolicies { get; set; } = true;
    }
}
