namespace IAMS.Application.DTOs.Commission
{
    public class CommissionRateDto
    {
        public int Id { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public int PolicyTypeId { get; set; }
        public string PolicyTypeName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public bool IsFuture { get; set; }
    }
}
