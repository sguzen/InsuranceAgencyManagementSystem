using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class VehicleMappingProfile : Profile
    {
        public VehicleMappingProfile()
        {
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
        }
    }
}
