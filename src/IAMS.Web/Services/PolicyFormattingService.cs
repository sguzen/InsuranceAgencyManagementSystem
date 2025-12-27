using IAMS.MultiTenancy.Interfaces;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Web.Services
{
    /// <summary>
    /// Service for formatting policy-related display data in the Web layer
    /// </summary>
    public interface IPolicyFormattingService
    {
        void FormatPolicyNumber(PolicyDto policy);
        void FormatPolicyNumbers(IEnumerable<PolicyDto> policies);
    }

    public class PolicyFormattingService : IPolicyFormattingService
    {
        private readonly ITenantContextAccessor _tenantContext;

        public PolicyFormattingService(ITenantContextAccessor tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public void FormatPolicyNumber(PolicyDto policy)
        {
            if (policy == null) return;

            var tenant = _tenantContext.CurrentTenant;
            var agencyNumber = tenant?.ExternalId ?? "N/A";
            var policyNumber = policy.PolicyNumber;
            var tecditNumber = policy.TecditNumber;

            // Format: AgencyNumber-PolicyNumber/TecditNumber
            // If no TecditNumber, just show AgencyNumber-PolicyNumber
            if (!string.IsNullOrEmpty(tecditNumber))
            {
                policy.FormattedPolicyNumber = $"{agencyNumber}-{policyNumber}/{tecditNumber}";
            }
            else
            {
                policy.FormattedPolicyNumber = $"{agencyNumber}-{policyNumber}";
            }
        }

        public void FormatPolicyNumbers(IEnumerable<PolicyDto> policies)
        {
            if (policies == null) return;

            foreach (var policy in policies)
            {
                FormatPolicyNumber(policy);
            }
        }
    }
}
