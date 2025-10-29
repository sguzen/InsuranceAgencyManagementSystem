using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Customer
{
    public class CreateOrUpdateCustomerDto
    {
        // Type & Basic Info
        public CustomerType Type { get; set; } = CustomerType.Individual;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; } = Gender.Male;

        // Personal Info
        public DateTime? DateOfBirth { get; set; }
        public int? NationalityCountryId { get; set; }

        // Identity
        public IdentificationType IdentificationType { get; set; } = IdentificationType.IdCard;
        public string IdentificationNumber { get; set; } = string.Empty;

        // Contact
        public string Email { get; set; } = string.Empty;
        public string? MobilePhoneCountryCode { get; set; }
        public string? MobilePhoneNumber { get; set; }
        public string? HomePhone { get; set; }

        // Address
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubdistrictId { get; set; }
        public int? VillageId { get; set; }

        // Professional
        public int? OccupationId { get; set; }

        // Status
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public string? Notes { get; set; }
    }
}