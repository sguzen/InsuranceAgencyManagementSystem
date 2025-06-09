namespace IAMS.Domain.Entities
{
    public class PolicyType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Category { get; set; }
        public int? MinimumTermMonths { get; set; }
        public int? MaximumTermMonths { get; set; }
        public decimal? MinimumPremium { get; set; }
        public decimal? MaximumPremium { get; set; }

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CommissionRate> CommissionRates { get; set; } = new List<CommissionRate>();

        // Business methods
        public void Activate(string activatedBy)
        {
            IsActive = true;
            UpdateAuditInfo(activatedBy);
        }

        public void Deactivate(string deactivatedBy)
        {
            IsActive = false;
            UpdateAuditInfo(deactivatedBy);
        }

        public bool IsValidPremiumAmount(decimal premiumAmount)
        {
            if (MinimumPremium.HasValue && premiumAmount < MinimumPremium.Value)
                return false;

            if (MaximumPremium.HasValue && premiumAmount > MaximumPremium.Value)
                return false;

            return true;
        }

        public bool IsValidTermLength(int termMonths)
        {
            if (MinimumTermMonths.HasValue && termMonths < MinimumTermMonths.Value)
                return false;

            if (MaximumTermMonths.HasValue && termMonths > MaximumTermMonths.Value)
                return false;

            return true;
        }
    }
}