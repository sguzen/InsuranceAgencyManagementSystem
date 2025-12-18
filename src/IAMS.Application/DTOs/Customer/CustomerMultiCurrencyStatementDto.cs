namespace IAMS.Application.DTOs.Customer
{
    /// <summary>
    /// Represents a multi-currency customer statement with separate statements per currency
    /// </summary>
    public class CustomerMultiCurrencyStatementDto
    {
        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Customer code
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Customer full address
        /// </summary>
        public string? CustomerAddress { get; set; }

        /// <summary>
        /// Statement start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Statement end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Report generation date
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// List of statements, one per currency
        /// </summary>
        public List<CustomerStatementDto> CurrencyStatements { get; set; } = new();

        /// <summary>
        /// Total number of transactions across all currencies
        /// </summary>
        public int TotalTransactionCount => CurrencyStatements.Sum(s => s.Transactions.Count);

        /// <summary>
        /// Number of currencies with activity
        /// </summary>
        public int ActiveCurrencyCount => CurrencyStatements.Count;

        /// <summary>
        /// Summary of balances by currency
        /// </summary>
        public Dictionary<string, decimal> BalancesByCurrency =>
            CurrencyStatements.ToDictionary(s => s.CurrencyCode, s => s.ClosingBalance);
    }
}
