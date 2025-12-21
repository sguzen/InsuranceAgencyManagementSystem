namespace IAMS.Shared.DTOs.InsuranceCompany
{
    public class InsuranceCompanyDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Aggregated data
        public int ActivePoliciesCount { get; set; }
        public int TotalPolicies { get; set; }
        public decimal TotalPremiums { get; set; }
        public decimal TotalCommissions { get; set; }
        public DateTime? LastPolicyDate { get; set; }

        // Currency breakdown
        public List<CurrencyBreakdownDto> CurrencyBreakdowns { get; set; } = new List<CurrencyBreakdownDto>();
    }
}