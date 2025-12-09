namespace IAMS.Shared.Models
{
    /// <summary>
    /// Value object representing currency breakdown data from repository layer.
    /// Maps to CurrencyBreakdownDto in Application layer.
    /// </summary>
    public class CurrencyBreakdown
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal TotalPremiums { get; set; }
        public decimal TotalCommissions { get; set; }
        public int PolicyCount { get; set; }
    }
}
