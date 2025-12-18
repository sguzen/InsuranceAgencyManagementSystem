namespace IAMS.Application.DTOs.Customer
{
    /// <summary>
    /// Represents a customer statement for a specific currency
    /// </summary>
    public class CustomerStatementDto
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
        /// Currency code (e.g., TRY, USD, EUR)
        /// </summary>
        public string CurrencyCode { get; set; } = "TRY";

        /// <summary>
        /// Currency symbol (e.g., ₺, $, €)
        /// </summary>
        public string CurrencySymbol { get; set; } = "₺";

        /// <summary>
        /// Currency name
        /// </summary>
        public string CurrencyName { get; set; } = string.Empty;

        /// <summary>
        /// Opening balance at start date
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Closing balance at end date
        /// </summary>
        public decimal ClosingBalance { get; set; }

        /// <summary>
        /// Total debit amount in the period
        /// </summary>
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total credit amount in the period
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// List of transactions in the period
        /// </summary>
        public List<CustomerStatementTransactionDto> Transactions { get; set; } = new();

        /// <summary>
        /// Report generation date
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// File number or reference
        /// </summary>
        public string? FileNumber { get; set; }
    }
}
