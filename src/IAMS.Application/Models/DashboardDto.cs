// src/IAMS.Application/Models/DashboardDto.cs
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
        public int PendingClaims { get; set; }
        public int OverduePayments { get; set; }
        public List<InsuranceCompanyStatsDto> InsuranceCompanyStats { get; set; } = new();
        public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
    }

    public class InsuranceCompanyStatsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int PolicyCount { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommission { get; set; }
    }

    public class MonthlyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NewPolicies { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommission { get; set; }
    }
}