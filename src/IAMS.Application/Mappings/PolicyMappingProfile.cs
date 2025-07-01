using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            CreateMap<Policy, PolicyDto>();

            CreateMap<CreatePolicyDto, Policy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.PolicyStatus.Draft))
                .ForMember(dest => dest.CommissionAmount, opt => opt.Ignore())
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
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore());
        }
    }
}