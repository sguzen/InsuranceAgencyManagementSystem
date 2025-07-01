using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Customer
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? KktcNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age => DateOfBirth?.CalculateAge();
        public Gender? Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public CustomerStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Aggregated data
        public int ActivePoliciesCount { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommissions { get; set; }
        public DateTime? LastPolicyDate { get; set; }
        public int PolicyCount { get; set; }

    }

    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}