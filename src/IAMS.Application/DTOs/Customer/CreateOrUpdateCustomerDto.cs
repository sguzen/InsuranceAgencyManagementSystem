using IAMS.Domain.Enums;

public class CreateOrUpdateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternativePhone { get; set; }
    public string? KktcNo { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public CustomerStatus Status { get; set; }
    public AddressDto? Address { get; set; }
    public string? Notes { get; set; }
    public string? Occupation { get; set; }
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
}