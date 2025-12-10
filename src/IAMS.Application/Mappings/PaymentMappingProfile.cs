using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Main mapping with all navigation properties
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

            // Lightweight list DTO mapping - optimized for ProjectTo
            // AutoMapper will automatically only select required fields when using ProjectTo
            CreateMap<PolicyPayment, PolicyPaymentListDto>()
                .ForMember(dest => dest.PolicyNumber, opt => opt.MapFrom(src => src.Policy.PolicyNumber))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Policy.Customer.FirstName + " " + src.Policy.Customer.LastName))
                .ForMember(dest => dest.InsuranceCompanyName, opt => opt.MapFrom(src => src.Policy.InsuranceCompany.Name))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Policy.Currency.Code))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // Map from lightweight DTO to full DTO
            CreateMap<PolicyPaymentListDto, PolicyPaymentDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src =>
                    src.CurrencyCode == "TRY" ? "₺" :
                    src.CurrencyCode == "USD" ? "$" :
                    src.CurrencyCode == "EUR" ? "€" : src.CurrencyCode));

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