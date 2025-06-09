namespace IAMS.Domain.Entities
{
    public class CommissionRate : BaseEntity
    {
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
        public virtual PolicyType PolicyType { get; set; } = null!;

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

        public void SetExpiryDate(DateTime expiryDate, string updatedBy)
        {
            if (expiryDate <= EffectiveDate)
                throw new ArgumentException("Expiry date must be after effective date");

            ExpiryDate = expiryDate;
            UpdateAuditInfo(updatedBy);
        }

        public bool IsEffectiveOn(DateTime date)
        {
            return IsActive
                   && EffectiveDate <= date
                   && (ExpiryDate == null || ExpiryDate >= date);
        }

        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.Today;
        public bool IsFuture => EffectiveDate > DateTime.Today;
    }
}