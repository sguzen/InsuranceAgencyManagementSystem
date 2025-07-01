using AutoMapper;
using IAMS.Domain.Entities;
using IAMS.Application.DTOs.InsuranceCompany;

namespace IAMS.Application.Mappings
{
    public class InsuranceCompanyMappingProfile : Profile
    {
        public InsuranceCompanyMappingProfile()
        {
            CreateMap<InsuranceCompany, InsuranceCompanyDto>()
                .ForMember(dest => dest.ActivePoliciesCount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPremiums, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCommissions, opt => opt.Ignore())
                .ForMember(dest => dest.LastPolicyDate, opt => opt.Ignore());

            CreateMap<CreateInsuranceCompanyDto, InsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
               // .ForMember(dest => dest.PolicyTypes, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());

            CreateMap<UpdateInsuranceCompanyDto, InsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
              //  .ForMember(dest => dest.PolicyTypes, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());
        }
    }
}