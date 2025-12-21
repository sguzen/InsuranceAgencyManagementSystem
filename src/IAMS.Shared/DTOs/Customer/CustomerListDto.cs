using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Customer
{
    /// <summary>
    /// Lightweight DTO for customer list operations
    /// Optimized for paged lists - only essential fields
    /// </summary>
    public class CustomerListDto
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string IdentificationNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public CustomerStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }

        // Lightweight aggregates (computed in query)
        public int ActivePoliciesCount { get; set; }
        public int TotalPoliciesCount { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommissions { get; set; }
        public DateTime? LastPolicyDate { get; set; }
    }
}
