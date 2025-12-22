using IAMS.Shared.QueryParams;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Customer;
using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.DTOs.InsuranceCompany;

namespace IAMS.Shared.DTOs.Policy
{
    public class PolicyDto
    {
        public int Id { get; set; }
        
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }

        // Sigortalı (Insured Person) - The person/entity that is insured
        public string? InsuredPersonName { get; set; }

        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public int? VehicleId { get; set; }

        // Renewal tracking - ID of the parent policy if this is a renewal
        public int? ParentPolicyId { get; set; }

        // Endorsement Information (Zeyilname)
        public string InnerCode { get; set; } = "000"; // Z.No - 3-digit code: 000 for main, 001+ for endorsements
        public StateType StateType { get; set; } // TIP column (P, T, V, R, X, Y)
        public bool IsEndorsement { get; set; }
        public string? EndorsementNumber { get; set; }
        public int? OriginalPolicyId { get; set; }
        public string? BranchCode { get; set; } // Kod column - External code for insurance type

        // Driver Information
        public int? DriverAge { get; set; } // YAS column
        public DriverType? DriverType { get; set; } // Sürücü - Single or Any

        // Marketer Information
        public string? Marketer { get; set; } // Pazarlamacı Adı

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; } = "TRY";
        public string CurrencySymbol { get; set; } = "₺";
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation properties
        public CustomerDto? Customer { get; set; }
        public InsuranceCompanyDto? InsuranceCompany { get; set; }
        public PolicyTypeInfoDto? PolicyType { get; set; }
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