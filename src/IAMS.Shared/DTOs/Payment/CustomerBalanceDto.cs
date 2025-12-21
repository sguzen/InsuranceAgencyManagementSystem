namespace IAMS.Shared.DTOs.Payment
{
    public class CustomerBalanceDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<CurrencyBalanceDto> Balances { get; set; } = new();
    }

    public class CurrencyBalanceDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
        public bool IsPositive => Balance >= 0;
    }
}
