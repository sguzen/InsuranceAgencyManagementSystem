using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Policy
{
    /// <summary>
    /// DTO for previewing imported policies before saving
    /// Allows operator to map customers before import
    /// </summary>
    public class PolicyImportPreviewDto
    {
        // Import row tracking
        public int RowNumber { get; set; }

        // Policy Information from Import
        public string? PolicyNumber { get; set; }
        public string InnerCode { get; set; } = "000";
        public StateType StateType { get; set; } = StateType.YeniPolice;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PolicyTypeCode { get; set; }
        public string? PolicyTypeName { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionAmount { get; set; }
        public string? CurrencyCode { get; set; }

        // Vehicle Information (for display)
        public string? PlateNumber { get; set; }
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleYear { get; set; }

        // Customer Information from Import (Sigortalı - Insured Person)
        public string? InsuredCustomerName { get; set; }
        public string? InsuredCustomerIdentifier { get; set; }

        // Mapping Fields (to be filled by operator)
        // Sigortalı (Insured Customer) mapping
        public int? InsuredCustomerId { get; set; } // Selected existing customer ID
        public bool CreateNewInsuredCustomer { get; set; } // If true, create new customer

        // Policy Owner (Cari Kart) mapping
        public int? PolicyOwnerCustomerId { get; set; } // Selected existing customer ID
        public bool PolicyOwnerSameAsInsured { get; set; } // If true, use same as InsuredCustomerId
        public bool CreateNewPolicyOwner { get; set; } // If true, create new customer
        public string? PolicyOwnerName { get; set; } // Name for new customer if creating
        public string? PolicyOwnerIdentifier { get; set; } // Identifier for new customer if creating

        // Suggested matches (populated by search)
        public List<CustomerMatchDto> SuggestedInsuredMatches { get; set; } = new();
        public List<CustomerMatchDto> SuggestedOwnerMatches { get; set; } = new();

        // Validation
        public bool IsValid => (InsuredCustomerId.HasValue || CreateNewInsuredCustomer) &&
                              (PolicyOwnerCustomerId.HasValue || PolicyOwnerSameAsInsured || CreateNewPolicyOwner);

        // All original import data for saving
        public ImportPolicyDto OriginalImportData { get; set; } = null!;
    }

    /// <summary>
    /// DTO for suggested customer matches during import mapping
    /// </summary>
    public class CustomerMatchDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? IdentificationNumber { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int MatchScore { get; set; } // 0-100, based on name/ID similarity
    }
}
