using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Settings;
using IAMS.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.PolicyImport
{
    public class ImportAutoPaymentService : IImportAutoPaymentService
    {
        // The law covers the whole traffic family: traffic itself in every spelling seen in
        // source data (Trafik/Trafic/Traffic) and every Kasko variant (Kasko, Yarım Kasko,
        // Tam Kasko, ...) — substring match on code, name, or category catches them all.
        private static readonly string[] MandatoryMarkers = { "TRAFIK", "TRAFFIC", "TRAFIC", "KASKO" };

        private readonly ITenantService _tenantService;
        private readonly ILogger<ImportAutoPaymentService> _logger;

        // One import run = one request scope, so caching here means the agency setting is
        // read once per batch and a toggle mid-import cannot split a run between modes.
        private bool? _autoPayEnabled;

        public ImportAutoPaymentService(
            ITenantService tenantService,
            ILogger<ImportAutoPaymentService> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        public string AutoPaymentNote => "Trafik/Kasko poliçesi - Tam ödeme (Yasal gereklilik)";

        public bool RequiresFullPaymentByLaw(PolicyType policyType)
        {
            return ContainsMandatoryMarker(policyType.Code) ||
                   ContainsMandatoryMarker(policyType.Name) ||
                   ContainsMandatoryMarker(policyType.Category);
        }

        public async Task<bool> ShouldAutoPayAsync(PolicyType policyType, CancellationToken cancellationToken = default)
        {
            if (!RequiresFullPaymentByLaw(policyType))
            {
                return false;
            }

            if (_autoPayEnabled == null)
            {
                var settings = await _tenantService.GetTenantSettingAsync<PolicyImportSettingsDto>(PolicyImportSettingsDto.SettingKey);
                _autoPayEnabled = settings?.AutoPayMandatoryPolicies ?? true;

                if (_autoPayEnabled == false)
                {
                    _logger.LogInformation("Auto full payment for traffic/kasko policies is disabled for this agency");
                }
            }

            return _autoPayEnabled.Value;
        }

        private static bool ContainsMandatoryMarker(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Fold the Turkish dotted/dotless İ/ı to plain I so TRAFİK, Trafik and trafık
            // all match; ToUpperInvariant alone does not map U+0130/U+0131.
            var normalized = value.ToUpperInvariant().Replace('İ', 'I').Replace('ı', 'I');

            return MandatoryMarkers.Any(normalized.Contains);
        }
    }
}
