using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Mappings
{
    public class InsuranceCompanyMappingProfile : Profile
    {
        public InsuranceCompanyMappingProfile()
        {
            CreateMap<InsuranceCompany, InsuranceCompanyDto>()
                .ForMember(dest => dest.TotalPolicies, opt => opt.MapFrom(src => src.Policies.Count))
                .ForMember(dest => dest.ActivePolicies, opt => opt.MapFrom(src => src.Policies.Count(p => p.Status == PolicyStatus.Active)));

            CreateMap<CreateInsuranceCompanyDto, InsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());

            CreateMap<UpdateInsuranceCompanyDto, InsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionRates, opt => opt.Ignore());
        }
    }
}