using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Shared.QueryParams;
using IAMS.Application.DTOs.Parametric;
using IAMS.Domain.Entities;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Mappings
{
    public class ParametricMappingProfile : Profile
    {
        public ParametricMappingProfile()
        {
            // Currency mappings
            CreateMap<Currency, CurrencyDto>();
            CreateMap<CurrencyDto, Currency>();

            // Country mappings
            CreateMap<Country, CountryDto>();
            CreateMap<CountryDto, Country>();

            // Occupation mappings
            CreateMap<Occupation, OccupationDto>();
            CreateMap<OccupationDto, Occupation>();

            // City mappings
            CreateMap<City, CityDto>();
            CreateMap<CityDto, City>();

            // District mappings
            CreateMap<District, DistrictDto>()
                .ForMember(dest => dest.CityName,
                    opt => opt.MapFrom(src => src.City != null ? src.City.NameTr : null));

            // Subdistrict mappings
            CreateMap<Subdistrict, SubdistrictDto>()
                .ForMember(dest => dest.DistrictName,
                    opt => opt.MapFrom(src => src.District != null ? src.District.NameTr : null));

            // Village mappings
            CreateMap<Village, VillageDto>()
                .ForMember(dest => dest.SubdistrictName,
                    opt => opt.MapFrom(src => src.Subdistrict != null ? src.Subdistrict.NameTr : null));

            // Customer mappings with parametric entity names
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Age,
                    opt => opt.MapFrom(src => src.Age))
                .ForMember(dest => dest.NationalityCountryName,
                    opt => opt.MapFrom(src => src.NationalityCountry != null ? src.NationalityCountry.NameTr : null))
                .ForMember(dest => dest.CityName,
                    opt => opt.MapFrom(src => src.City != null ? src.City.NameTr : null))
                .ForMember(dest => dest.DistrictName,
                    opt => opt.MapFrom(src => src.District != null ? src.District.NameTr : null))
                .ForMember(dest => dest.SubdistrictName,
                    opt => opt.MapFrom(src => src.Subdistrict != null ? src.Subdistrict.NameTr : null))
                .ForMember(dest => dest.VillageName,
                    opt => opt.MapFrom(src => src.Village != null ? src.Village.NameTr : null))
                .ForMember(dest => dest.OccupationName,
                    opt => opt.MapFrom(src => src.Occupation != null ? src.Occupation.NameTr : null));

            CreateMap<CreateOrUpdateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                // Navigation properties - handled separately in service
                .ForMember(dest => dest.NationalityCountry, opt => opt.Ignore())
                .ForMember(dest => dest.City, opt => opt.Ignore())
                .ForMember(dest => dest.District, opt => opt.Ignore())
                .ForMember(dest => dest.Subdistrict, opt => opt.Ignore())
                .ForMember(dest => dest.Village, opt => opt.Ignore())
                .ForMember(dest => dest.Occupation, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore());
        }
    }
}