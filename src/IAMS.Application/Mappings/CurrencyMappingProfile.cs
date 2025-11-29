using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class CurrencyMappingProfile : Profile
    {
        public CurrencyMappingProfile()
        {
            // Currency mappings
            CreateMap<Currency, CurrencyDto>();
            CreateMap<CurrencyDto, Currency>();

            // CurrencyExchangeRate mappings
            CreateMap<CurrencyExchangeRate, ExchangeRateDto>()
                .ForMember(dest => dest.FromCurrencyCode, opt => opt.Ignore())
                .ForMember(dest => dest.FromCurrencySymbol, opt => opt.Ignore())
                .ForMember(dest => dest.ToCurrencyCode, opt => opt.Ignore())
                .ForMember(dest => dest.ToCurrencySymbol, opt => opt.Ignore());

            CreateMap<CreateExchangeRateDto, CurrencyExchangeRate>();

            CreateMap<ExchangeRateDto, CurrencyExchangeRate>();

            // Money DTO mapping
            CreateMap<MoneyDto, CurrencyConversionDto>()
                .ForMember(dest => dest.OriginalAmount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.OriginalCurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
                .ForMember(dest => dest.TargetCurrencyId, opt => opt.Ignore())
                .ForMember(dest => dest.ConvertedAmount, opt => opt.Ignore())
                .ForMember(dest => dest.ExchangeRate, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyCode, opt => opt.Ignore());
        }
    }
}
