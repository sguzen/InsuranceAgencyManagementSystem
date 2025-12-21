namespace IAMS.Shared.DTOs.Customer
{
    /// <summary>
    /// Represents a single transaction line in a customer statement
    /// </summary>
    public class CustomerStatementTransactionDto
    {
        /// <summary>
        /// Document/Entry number (e.g., Policy number, Payment number, Invoice number)
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Transaction date
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Debit amount (money owed by customer)
        /// </summary>
        public decimal Debit { get; set; }

        /// <summary>
        /// Credit amount (money paid by customer or credited to account)
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Running balance after this transaction
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Transaction description/explanation
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Transaction type (e.g., "POL NO: PER", "BORDRO: POL NO: PER", etc.)
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>
        /// Related policy number if applicable
        /// </summary>
        public string? PolicyNumber { get; set; }

        /// <summary>
        /// Related invoice number if applicable
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Related payment reference if applicable
        /// </summary>
        public string? PaymentReference { get; set; }
    }
}
