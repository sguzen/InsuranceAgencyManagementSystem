namespace IAMS.Domain.Entities
{
    public class CustomerInsuranceCompany : BaseEntity
    {
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string ExternalCustomerId { get; set; } = string.Empty; // Customer ID in insurance company's system
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncDate { get; set; }
        public string? SyncStatus { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;

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

        public void UpdateSyncStatus(string status, string updatedBy)
        {
            SyncStatus = status;
            LastSyncDate = DateTime.UtcNow;
            UpdateAuditInfo(updatedBy);
        }

        public bool RequiresSync => !LastSyncDate.HasValue || LastSyncDate < DateTime.UtcNow.AddDays(-1);
    }
}