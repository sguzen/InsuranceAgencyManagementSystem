using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Policy;
using IAMS.MultiTenancy.Interfaces;

namespace IAMS.Application.Mappings
{
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            // Map PolicyType entity to the Policy.PolicyTypeInfoDto (simple version)
            CreateMap<PolicyType, PolicyTypeInfoDto>()
                .ForMember(dest => dest.DefaultCommissionRate, opt => opt.MapFrom(src => 0)); // Default value if not available

            CreateMap<Policy, PolicyDto>()
                .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : "TRY"))
                .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : "₺"))
                .ForMember(dest => dest.FormattedPolicyNumber, opt => opt.MapFrom<FormattedPolicyNumberResolver>());

            CreateMap<CreatePolicyDto, Policy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.PolicyStatus.Draft))
                .ForMember(dest => dest.CommissionAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyId, opt => opt.Ignore()) // Will be set in handler after currency lookup
                .ForMember(dest => dest.Currency, opt => opt.Ignore()) // Navigation property, ignore during mapping
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore()) // Navigation property, ignore during mapping
                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore());

            CreateMap<UpdatePolicyDto, Policy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyId, opt => opt.Ignore()) // Will be set in handler after currency lookup
                .ForMember(dest => dest.Currency, opt => opt.Ignore()) // Navigation property, ignore during mapping
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore()) // Navigation property, ignore during mapping
                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore());
        }
    }

    /// <summary>
    /// Custom resolver to format policy number as: {TenantExternalId}-{PolicyNumber}/{TecditNumber}
    /// Example: A001-0000038/001
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