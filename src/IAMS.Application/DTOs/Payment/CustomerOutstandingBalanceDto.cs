namespace IAMS.Application.DTOs.Payment
{
    public class CustomerOutstandingBalanceDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
        public int ActivePoliciesCount { get; set; }
        public int PendingPaymentsCount { get; set; }
    }
}
