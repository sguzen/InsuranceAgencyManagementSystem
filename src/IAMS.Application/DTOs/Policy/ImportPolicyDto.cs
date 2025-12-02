using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    /// <summary>
    /// DTO for importing policies and endorsements from Excel
    /// Maps to the Excel columns provided by the user
    /// </summary>
    public class ImportPolicyDto
    {
        // Column A - Branch Code (Kod)
        public string? BranchCode { get; set; }

        // Policy/Endorsement identification
        public string? PolicyNumber { get; set; }
        public string InnerCode { get; set; } = "000"; // 3-digit code: 000 for main, 001+for endorsements
        public StateType StateType { get; set; } = StateType.MainPolicy; // Zeyil Tipi

        [Obsolete("Use InnerCode instead")]
        public string? EndorsementNumber { get; set; } // Legacy field
        [Obsolete("Use InnerCode != \"000\" instead")]
        public bool IsEndorsement { get; set; }

        // Customer Information
        public string? CustomerIdentifier { get; set; } // TC/Tax number to lookup customer
        public string? CustomerName { get; set; }
        public string? CustomerCountryCode { get; set; } // From Kimlik No (e.g., "601")
        public string? CustomerIdType { get; set; } // From Kimlik No (e.g., "KN")

        // Insurance Company
        public string? InsuranceCompanyCode { get; set; }
        public string? InsuranceCompanyName { get; set; }

        // Policy Type
        public string? PolicyTypeCode { get; set; }
        public string? PolicyTypeName { get; set; }

        // Dates
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Financial
        public decimal PremiumAmount { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionAmount { get; set; }
        public string? CurrencyCode { get; set; }

        // Driver Information
        public int? DriverAge { get; set; } // YAS column
        public string? DriverTypeText { get; set; } // Sürücü - will be parsed to enum

        // Marketer Information
        public string? MarketerCode { get; set; } // paz.kod
        public string? MarketerName { get; set; } // pazarlamaci adi

        // Vehicle Information
        public string? PlateNumber { get; set; }
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleYear { get; set; }

        // Payment Information
        public decimal? PaidAmount { get; set; } // Initial payment amount
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }

        // Additional fields
        public string? Notes { get; set; }
        public PolicyStatus Status { get; set; } = PolicyStatus.Active;

        // For tracking import
        public int RowNumber { get; set; }
    }
}
