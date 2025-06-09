using System;

namespace IAMS.MultiTenancy.Entities
{
    public interface ITenantEntity
    {
        int Id { get; set; }
        string Name { get; set; }
        string Identifier { get; set; }
        string ConnectionString { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime? LastUpdated { get; set; }
        string SubscriptionPlan { get; set; }
        DateTime? SubscriptionExpiry { get; set; }
        int MaxUsers { get; set; }
        long MaxStorageBytes { get; set; }
        string ContactEmail { get; set; }
        string ContactPhone { get; set; }
        string TimeZone { get; set; }
        string Currency { get; set; }
        string Language { get; set; }
    }
}