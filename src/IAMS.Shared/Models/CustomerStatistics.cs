using IAMS.Domain.Enums;

namespace IAMS.Shared.Models
{
    /// <summary>
    /// Value object representing customer statistics data from repository layer.
    /// Maps to CustomerStatisticsDto in Application layer.
    /// </summary>
    public class CustomerStatistics
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int NewCustomersThisWeek { get; set; }
        public decimal CustomerGrowthPercentage { get; set; }
        public int CustomersWithActivePolicies { get; set; }
        public int CustomersWithoutPolicies { get; set; }
        public double AverageCustomerAge { get; set; }
        public Dictionary<CustomerStatus, int> CustomersByStatus { get; set; } = new();
        public Dictionary<Gender, int> CustomersByGender { get; set; } = new();
    }
}
