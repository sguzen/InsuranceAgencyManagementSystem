namespace IAMS.Shared.DTOs.PolicyType
{
    public class PolicyTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public int? MinimumTermMonths { get; set; }
        public int? MaximumTermMonths { get; set; }
        public decimal? MinimumPremium { get; set; }
        public decimal? MaximumPremium { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalPolicies { get; set; }
    }
}