using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<PolicyPayment, PolicyPaymentDto>()
                .ForMember(dest => dest.PolicyNumber, opt => opt.MapFrom(src => src.Policy.PolicyNumber))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Policy != null && src.Policy.Customer != null
                        ? $"{src.Policy.Customer.FirstName} {src.Policy.Customer.LastName}"
                        : string.Empty))
                .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src =>
                    src.Policy != null && src.Policy.Currency != null
                        ? src.Policy.Currency.Symbol
                        : "₺"));

            CreateMap<CreatePolicyPaymentDto, PolicyPayment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Policy, opt => opt.Ignore());
        }
    }
}