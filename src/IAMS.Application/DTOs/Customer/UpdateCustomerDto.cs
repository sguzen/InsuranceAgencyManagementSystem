using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Customer
{
    public class UpdateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? KktcNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public CustomerStatus Status { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
}