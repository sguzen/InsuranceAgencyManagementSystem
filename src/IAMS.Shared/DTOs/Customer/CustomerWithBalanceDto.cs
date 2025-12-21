using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Customer
{
    public class CustomerWithBalanceDto
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? MobilePhoneNumber { get; set; }
        public CustomerStatus Status { get; set; }
        public int ActivePolicyCount { get; set; }
        public DateTime CreatedOn { get; set; }

        // Balance specific fields
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalPaid { get; set; }
    }
}
