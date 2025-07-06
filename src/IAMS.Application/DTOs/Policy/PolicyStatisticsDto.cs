using IAMS.Domain.Enums;

public class PolicyStatisticsDto
{
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiredPolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public int CancelledPolicies { get; set; }
    public decimal TotalPremiumAmount { get; set; }
    public decimal AveragePremiumAmount { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public int NewPoliciesThisMonth { get; set; }
    public int NewPoliciesThisWeek { get; set; }
    public Dictionary<PolicyStatus, int> PoliciesByStatus { get; set; } = new();
    public Dictionary<string, int> PoliciesByType { get; set; } = new();
    public Dictionary<string, decimal> PremiumByInsuranceCompany { get; set; } = new();
}