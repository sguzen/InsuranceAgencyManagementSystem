using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.TcNo, opt => opt.Ignore()) // Don't allow TC number updates
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore());
        }
    }
}