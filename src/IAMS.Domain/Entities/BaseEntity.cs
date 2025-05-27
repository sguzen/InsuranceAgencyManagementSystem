namespace IAMS.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int TenantId { get; set; }
    }
}