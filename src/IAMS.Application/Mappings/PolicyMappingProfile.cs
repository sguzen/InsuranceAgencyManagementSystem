using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Mappings
{
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            CreateMap<Policy, PolicyDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
                .ForMember(dest => dest.InsuranceCompanyName, opt => opt.MapFrom(src => src.InsuranceCompany.Name))
                .ForMember(dest => dest.PolicyTypeName, opt => opt.MapFrom(src => src.PolicyType.Name));

            CreateMap<CreatePolicyDto, Policy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PolicyStatus.Draft));

            CreateMap<UpdatePolicyDto, Policy>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyNumber, opt => opt.Ignore()) // Don't allow policy number updates
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore());
        }
    }
}