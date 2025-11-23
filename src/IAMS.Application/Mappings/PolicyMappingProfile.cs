using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            // Map PolicyType entity to the Policy.PolicyTypeDto (simple version)
            CreateMap<PolicyType, PolicyTypeDto>()
                .ForMember(dest => dest.DefaultCommissionRate, opt => opt.MapFrom(src => 0)); // Default value if not available

            CreateMap<Policy, PolicyDto>()
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : "TRY"));

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
                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore());
        }
    }
}