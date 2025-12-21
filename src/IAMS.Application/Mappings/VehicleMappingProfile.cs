using AutoMapper;
using IAMS.Shared.DTOs.Vehicle;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class VehicleMappingProfile : Profile
    {
        public VehicleMappingProfile()
        {
            // Vehicle mappings
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyId, opt => opt.Ignore()) // Use default value from entity
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore());

            CreateMap<UpdateVehicleDto, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyId, opt => opt.Ignore()) // Use default value from entity
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore());

            // VehicleBrand mappings
            CreateMap<VehicleBrand, VehicleBrandDto>();
            CreateMap<VehicleBrandDto, VehicleBrand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Models, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            // VehicleModel mappings
            CreateMap<VehicleModel, VehicleModelDto>();
            CreateMap<VehicleModelDto, VehicleModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            // Vehicle mappings
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.BrandName ?? src.Brand.Name ?? string.Empty))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.ModelName ?? src.Model.Name ?? string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : string.Empty))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : "TRY"))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.CurrencyId, opt => opt.Ignore()) // Will be set manually in handler
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore());
        }
    }
}
