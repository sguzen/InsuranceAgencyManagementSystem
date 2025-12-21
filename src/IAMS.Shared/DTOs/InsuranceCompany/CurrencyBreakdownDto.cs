namespace IAMS.Shared.DTOs.InsuranceCompany
{
    public class CurrencyBreakdownDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal TotalPremiums { get; set; }
        public decimal TotalCommissions { get; set; }
        public int PolicyCount { get; set; }
    }
}
