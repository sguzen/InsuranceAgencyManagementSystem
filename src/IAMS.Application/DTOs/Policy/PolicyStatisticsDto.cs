namespace IAMS.Application.DTOs.Policy
{
    public class PolicyStatisticsDto
    {
        // Count statistics
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int DraftPolicies { get; set; }
        public int ExpiredPolicies { get; set; }
        public int CancelledPolicies { get; set; }
        public int SuspendedPolicies { get; set; }
        public int ExpiringPolicies { get; set; }

        // Financial statistics
        public decimal TotalPremiums { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal AveragePremium { get; set; }
        public decimal AverageCommissionRate { get; set; }

        // Period statistics
        public int NewPoliciesThisMonth { get; set; }
        public int RenewalsThisMonth { get; set; }
        public int CancellationsThisMonth { get; set; }
        public int ExpirationsThisMonth { get; set; }

        // Performance metrics
        public double RenewalRate { get; set; }
        public double CancellationRate { get; set; }
        public double GrowthRate { get; set; }

        // By category breakdowns
        public Dictionary<string, int> PoliciesByType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByType { get; set; } = new();
        public Dictionary<string, int> PoliciesByCompany { get; set; } = new();
        public Dictionary<string, decimal> RevenueByCompany { get; set; } = new();
        public Dictionary<string, int> PoliciesByMonth { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    }
}