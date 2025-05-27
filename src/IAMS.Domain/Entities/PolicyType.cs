namespace IAMS.Domain.Entities
{
    public class PolicyType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CommissionRate> CommissionRates { get; set; } = new List<CommissionRate>();
    }
}