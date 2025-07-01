namespace IAMS.Application.Models
{
    public class DashboardDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int ExpiringPoliciesThisMonth { get; set; }
        public decimal TotalPremiumAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int NewPoliciesThisMonth { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalInsuranceCompanies { get; set; }
        public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<PolicyStatusSummaryDto> PolicyStatusSummary { get; set; } = new();
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public int PolicyCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int PolicyCount { get; set; }
        public decimal TotalPremium { get; set; }
    }

    public class PolicyStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}