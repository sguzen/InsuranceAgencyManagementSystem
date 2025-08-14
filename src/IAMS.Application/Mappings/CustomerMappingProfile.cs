using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.CustomerMapping;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Existing Customer mappings
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateOrUpdateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerCode, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.CustomerStatus.Active))
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                //.ForMember(dest => dest.CustomerMappings, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerInsuranceCompanies, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));


            // Customer-Insurance Company Mapping
            CreateMap<CustomerInsuranceCompany, CustomerMappingDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
                .ForMember(dest => dest.InsuranceCompanyName, opt => opt.MapFrom(src => src.InsuranceCompany.Name));

            CreateMap<CreateCustomerMappingDto, CustomerInsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.LastSyncDate, opt => opt.Ignore());

            CreateMap<UpdateCustomerMappingDto, CustomerInsuranceCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore());
        }
    }
}