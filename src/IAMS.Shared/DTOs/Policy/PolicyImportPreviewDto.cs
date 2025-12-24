using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Policy
{
    /// <summary>
    /// DTO for previewing imported policies before saving
    /// Sigortalı (insured customer) is always taken from Excel data and auto-created/matched
    /// Operator only needs to specify the Policy Owner (who pays)
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

        // Sigortalı (Insured Customer) - from Excel, will be auto-created/matched during import
        public string? InsuredCustomerName { get; set; }
        public string? InsuredCustomerIdentifier { get; set; }

        // Policy Owner (Cari Kart) - operator must specify
        public bool PolicyOwnerSameAsInsured { get; set; } = true; // Default: same as sigortalı

        // If PolicyOwnerSameAsInsured = false, operator must either:
        // 1. Select existing customer (set PolicyOwnerCustomerId)
        public int? PolicyOwnerCustomerId { get; set; }

        // 2. Create new customer (set CreateNewPolicyOwner = true and provide name/ID/phone)
        public bool CreateNewPolicyOwner { get; set; }
        public string? PolicyOwnerName { get; set; }
        public string? PolicyOwnerIdentifier { get; set; }
        public string? PolicyOwnerPhone { get; set; }

        // Additional fields for new customer creation
        public CustomerType PolicyOwnerCustomerType { get; set; } = CustomerType.Individual;
        public int? PolicyOwnerNationalityCountryId { get; set; } // Required for Individual type

        // Validation: Policy owner must be specified
        public bool IsValid => PolicyOwnerSameAsInsured ||
                              PolicyOwnerCustomerId.HasValue ||
                              (CreateNewPolicyOwner &&
                               !string.IsNullOrEmpty(PolicyOwnerName) &&
                               !string.IsNullOrEmpty(PolicyOwnerIdentifier) &&
                               (PolicyOwnerCustomerType == CustomerType.Corporate || PolicyOwnerNationalityCountryId.HasValue));

        // All original import data for saving
        public ImportPolicyDto OriginalImportData { get; set; } = null!;
    }
}
