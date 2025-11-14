using AutoMapper;
using IAMS.Application.DTOs.Commission;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class CommissionMappingProfile : Profile
    {
        public CommissionMappingProfile()
        {
            CreateMap<CommissionRate, CommissionRateDto>()
                .ForMember(dest => dest.InsuranceCompanyName, opt => opt.MapFrom(src => src.InsuranceCompany.Name))
                .ForMember(dest => dest.PolicyTypeName, opt => opt.MapFrom(src => src.PolicyType.Name))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired))
                .ForMember(dest => dest.IsFuture, opt => opt.MapFrom(src => src.IsFuture));

            CreateMap<CreateOrUpdateCommissionRateDto, CommissionRate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore());

            CreateMap<Policy, CommissionSummaryDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer.CustomerType == Domain.Enums.CustomerType.Individual
                        ? $"{src.Customer.FirstName} {src.Customer.LastName}"
                        : src.Customer.CompanyName))
                .ForMember(dest => dest.InsuranceCompanyName, opt => opt.MapFrom(src => src.InsuranceCompany.Name))
                .ForMember(dest => dest.PolicyTypeName, opt => opt.MapFrom(src => src.PolicyType.Name))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.PolicyStatus, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
