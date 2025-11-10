using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicyDto
    {
        public int Id { get; set; }
        
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public int? VehicleId { get; set; }

        // Renewal tracking - ID of the parent policy if this is a renewal
        public int? ParentPolicyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation properties
        public CustomerDto? Customer { get; set; }
        public InsuranceCompanyDto? InsuranceCompany { get; set; }
        public PolicyTypeDto? PolicyType { get; set; }
        public VehicleDto? Vehicle { get; set; }

        // Calculated properties
        public bool IsActive => Status == PolicyStatus.Active && !IsExpired;
        public bool IsExpired => EndDate < DateTime.Today;
        public int DaysUntilExpiry => (EndDate - DateTime.Today).Days;
        public decimal OutstandingAmount { get; set; }
        public bool IsFullyPaid => OutstandingAmount <= 0;
        public bool CanRenew => Status == PolicyStatus.Expired ||
                                (Status == PolicyStatus.Active && EndDate <= DateTime.Today.AddDays(60));
    }
}