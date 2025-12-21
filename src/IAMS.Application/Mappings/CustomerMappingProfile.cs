using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Shared.DTOs.CustomerMapping;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Existing Customer mappings
            CreateMap<Customer, CustomerDto>();

            // Lightweight list DTO mapping - optimized for ProjectTo
            // AutoMapper will automatically only select required fields when using ProjectTo
            // Only essential fields for list view - excludes Email, Phone, Status, CreatedOn for optimization
            CreateMap<Customer, CustomerListDto>()
                .ForMember(dest => dest.ActivePoliciesCount, opt => opt.MapFrom(src =>
                    src.Policies.Count(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)))
                .ForMember(dest => dest.TotalPoliciesCount, opt => opt.MapFrom(src =>
                    src.Policies.Count(p => !p.IsDeleted)))
                .ForMember(dest => dest.TotalPremium, opt => opt.MapFrom(src =>
                    src.Policies.Where(p => !p.IsDeleted).Sum(p => (decimal?)p.PremiumAmount) ?? 0))
                .ForMember(dest => dest.TotalCommissions, opt => opt.MapFrom(src =>
                    src.Policies.Where(p => !p.IsDeleted).Sum(p => (decimal?)p.CommissionAmount) ?? 0))
                .ForMember(dest => dest.LastPolicyDate, opt => opt.MapFrom(src =>
                    src.Policies.Where(p => !p.IsDeleted).Max(p => (DateTime?)p.CreatedOn)));

            // Map from lightweight DTO to full DTO
            CreateMap<CustomerListDto, CustomerDto>()
                .ForMember(dest => dest.PolicyCount, opt => opt.MapFrom(src => src.TotalPoliciesCount))
                .ForMember(dest => dest.ActivePolicyCount, opt => opt.MapFrom(src => src.ActivePoliciesCount))
                .ForMember(dest => dest.TotalPolicyCount, opt => opt.MapFrom(src => src.TotalPoliciesCount))
                .ForMember(dest => dest.HasActivePolicies, opt => opt.MapFrom(src => src.ActivePoliciesCount > 0));

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