using AutoMapper;
using IAMS.Application.DTOs.PolicyType;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class PolicyTypeMappingProfile : Profile
    {
        public PolicyTypeMappingProfile()
        {
            CreateMap<PolicyType, PolicyTypeDto>()
                .ForMember(dest => dest.TotalPolicies, opt => opt.MapFrom(src => src.Policies.Count));

            CreateMap<CreatePolicyTypeDto, PolicyType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());

            CreateMap<UpdatePolicyTypeDto, PolicyType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());
        }
    }
}