using AutoMapper;
using IAMS.Domain.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Web.Mappings
{
    /// <summary>
    /// Web layer AutoMapper profile for Policy-related mappings that require infrastructure dependencies
    /// </summary>
    public class WebPolicyMappingProfile : Profile
    {
        public WebPolicyMappingProfile()
        {
            CreateMap<Policy, PolicyDto>()
                .ForMember(dest => dest.FormattedPolicyNumber, opt => opt.MapFrom<FormattedPolicyNumberResolver>());
        }
    }

    /// <summary>
    /// Custom resolver to format policy number as: {TenantExternalId}-{PolicyNumber}/{TecditNumber}
    /// Example: A001-0000038/001
    /// This resolver is in the Web layer because it requires ITenantContextAccessor (infrastructure dependency)
    /// </summary>
    public class FormattedPolicyNumberResolver : IValueResolver<Policy, PolicyDto, string?>
    {
        private readonly ITenantContextAccessor _tenantContext;

        public FormattedPolicyNumberResolver(ITenantContextAccessor tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public string? Resolve(Policy source, PolicyDto destination, string? destMember, ResolutionContext context)
        {
            var tenant = _tenantContext.CurrentTenant;
            var agencyNumber = tenant?.ExternalId ?? "N/A";
            var policyNumber = source.PolicyNumber;
            var tecditNumber = source.TecditNumber;

            // Format: AgencyNumber-PolicyNumber/TecditNumber
            // If no TecditNumber, just show AgencyNumber-PolicyNumber
            if (!string.IsNullOrEmpty(tecditNumber))
            {
                return $"{agencyNumber}-{policyNumber}/{tecditNumber}";
            }

            return $"{agencyNumber}-{policyNumber}";
        }
    }
}
