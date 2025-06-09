using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int InsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public int PolicyTypeId { get; set; }
        public string PolicyTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
// ())
//                .ForMember(dest => dest.PolicyPayments, opt => opt.Ignore())
//                .ForMember(dest => dest.PolicyClaims, opt => opt.Ignore())
//                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PolicyStatus.Draft));

//CreateMap<UpdatePolicyDto, Policy>()
//    .ForMember(dest => dest.Id, opt => opt.Ignore())
//    .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
//    .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
//    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
//    .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
//    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
//    .ForMember(dest => dest.TenantId, opt => opt.Ignore())
//    .ForMember(dest => dest.PolicyNumber, opt => opt.Ignore()) // Don't allow policy number updates
//    .ForMember(dest => dest.Customer, opt => opt.Ignore())
//    .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
//    .ForMember(dest => dest.PolicyType, opt => opt.Ignore