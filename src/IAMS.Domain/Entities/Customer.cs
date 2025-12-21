using IAMS.Domain.Enums;
using IAMS.Domain.Events;
using IAMS.Domain.Exceptions;
using IAMS.Domain.ValueObjects;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IAMS.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        // Address fields
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }  // Mahalle
        public int? SubdistrictId { get; set; } // Bucak
        public int? VillageId { get; set; }   // Köy

        // Contact
        public int? OccupationId { get; set; } // Meslek kodu - parametrik
        public int? NationalityCountryId { get; set; } // Uyruk - parametrik (052-Türkiye, 601-KKTC)

        // Phone with country code
        public string? MobilePhoneCountryCode { get; set; }
        public string? MobilePhoneNumber { get; set; }
        public string? HomePhone { get; set; }

        public string IdentificationNumber { get; set; } = string.Empty; 

        public DateTime? DateOfBirth { get; set; }
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public CustomerType Type { get; set; } = CustomerType.Individual;
        public IdentificationType IdentificationType { get; set; } = IdentificationType.IdCard;
        public Gender Gender { get; set; } = Gender.Male;
        public string? Notes { get; set; }

        // Navigation properties - Parametric tables
        public virtual City? City { get; set; }
        public virtual District? District { get; set; }
        public virtual Subdistrict? Subdistrict { get; set; }
        public virtual Village? Village { get; set; }
        public virtual Occupation? Occupation { get; set; }
        public virtual Country? NationalityCountry { get; set; }

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; } = new List<CustomerInsuranceCompany>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();


        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive => Status == CustomerStatus.Active && !IsDeleted;
        public int? Age => DateOfBirth.HasValue
            ? DateTime.Today.Year - DateOfBirth.Value.Year - (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : null;

        // Business methods
        public void Activate(string activatedBy)
        {
            if (Status == CustomerStatus.Blacklisted)
                throw new InvalidOperationDomainException(
                    "ActivateCustomer",
                    "Cannot activate a blacklisted customer");

            Status = CustomerStatus.Active;
            UpdateAuditInfo(activatedBy);
        }

        public void Deactivate(string deactivatedBy, string? reason = null)
        {
            Status = CustomerStatus.Inactive;
            if (!string.IsNullOrWhiteSpace(reason))
                Notes = $"{Notes}\nDeactivated: {reason}".Trim();
            UpdateAuditInfo(deactivatedBy);
        }

        public void Blacklist(string blacklistedBy, string reason)
        {
            Status = CustomerStatus.Blacklisted;
            Notes = $"{Notes}\nBlacklisted: {reason}".Trim();
            UpdateAuditInfo(blacklistedBy);
        }

        public void MapToInsuranceCompany(int insuranceCompanyId, string externalCustomerId, string mappedBy)
        {
            var existingMapping = CustomerInsuranceCompanies
                .FirstOrDefault(c => c.InsuranceCompanyId == insuranceCompanyId);

            if (existingMapping != null)
            {
                existingMapping.ExternalCustomerId = externalCustomerId;
                existingMapping.UpdateAuditInfo(mappedBy);
            }
            else
            {
                var mapping = new CustomerInsuranceCompany
                {
                    CustomerId = Id,
                    InsuranceCompanyId = insuranceCompanyId,
                    ExternalCustomerId = externalCustomerId,
                    RegisteredDate = DateTime.UtcNow,
                    CreatedBy = mappedBy
                };

                CustomerInsuranceCompanies.Add(mapping);
                AddDomainEvent(new CustomerMappedToInsuranceCompanyEvent(mapping, mappedBy));
            }
        }

        public decimal GetTotalPremiumAmount()
        {
            return GetActivePolicies().Sum(p => p.PremiumAmount);
        }

        public bool HasPolicyWithInsuranceCompany(int insuranceCompanyId)
        {
            return Policies.Any(p => p.InsuranceCompanyId == insuranceCompanyId && p.Status == PolicyStatus.Active);
        }

        public string? GetExternalCustomerId(int insuranceCompanyId)
        {
            return CustomerInsuranceCompanies
                .FirstOrDefault(c => c.InsuranceCompanyId == insuranceCompanyId && c.IsActive)?
                .ExternalCustomerId;
        }

        // Validation methods
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove common phone number formatting
            var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)\+]", "");

            // Check if it's all digits and has reasonable length
            return Regex.IsMatch(cleanPhone, @"^\d{7,15}$");
        }

        private static bool IsValidTcNo(string IdentificationNo)
        {
            if (string.IsNullOrWhiteSpace(IdentificationNo) || IdentificationNo.Length != 11)
                return false;

            return IdentificationNo.All(char.IsDigit);
        }

        protected override void Validate()
        {
            var errors = new List<string>();

            // FirstName is always required (for individuals it's first name, for corporate it's company name)
            if (string.IsNullOrWhiteSpace(FirstName))
                errors.Add("First name is required");

            // LastName is only required for individual customers
            if (Type == CustomerType.Individual && string.IsNullOrWhiteSpace(LastName))
                errors.Add("Last name is required");

            if (!IsValidEmail(Email))
                errors.Add("Valid email is required");

            if (!IsValidPhone(Phone))
                errors.Add("Valid phone number is required");

            if (string.IsNullOrWhiteSpace(IdentificationNumber))
                errors.Add("Identification number is required");

            // DateOfBirth validation only for individual customers
            if (Type == CustomerType.Individual)
            {
                if (DateOfBirth.HasValue && DateOfBirth >= DateTime.Today)
                    errors.Add("Date of birth must be in the past");

                if (Age.HasValue && (Age < 0 || Age > 150))
                    errors.Add("Invalid age");
            }

            if (errors.Any())
                throw new CustomerValidationException(this, errors);
        }

        public static Customer Create(
             string customerCode,
             CustomerType type,
             string firstName,
             string lastName,
             Gender gender,
             string email,
             IdentificationType identificationType,
             string identificationNumber,
             string createdBy,
             DateTime? dateOfBirth = null,
             int? nationalityCountryId = null,
             string? address1 = null,
             string? mobilePhoneNumber = null)
        {
            var customer = new Customer
            {
                CustomerCode = customerCode,
                Type = type,
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                Email = email,
                IdentificationType = identificationType,
                IdentificationNumber = identificationNumber,
                DateOfBirth = dateOfBirth,
                NationalityCountryId = nationalityCountryId,
                Address1 = address1,
                MobilePhoneNumber = mobilePhoneNumber,
                Status = CustomerStatus.Active,
                CreatedBy = createdBy
            };

            customer.Validate();
            customer.AddDomainEvent(new CustomerRegisteredEvent(customer, createdBy));

            return customer;
        }

        // Business methods
        public void UpdatePersonalInfo(
            string firstName,
            string lastName,
            Gender gender,
            DateTime? dateOfBirth,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdatePersonalInfo",
                    "Cannot update deleted customer");

            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            DateOfBirth = dateOfBirth;

            UpdateAuditInfo(updatedBy);
            Validate();
        }

        public void UpdateContactInfo(
            string email,
            string? mobilePhoneCountryCode,
            string? mobilePhoneNumber,
            string? homePhone,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateContactInfo",
                    "Cannot update deleted customer");

            Email = email;
            MobilePhoneCountryCode = mobilePhoneCountryCode;
            MobilePhoneNumber = mobilePhoneNumber;
            HomePhone = homePhone;

            UpdateAuditInfo(updatedBy);
            Validate();
        }

        public void UpdateAddress(
            string? address1,
            string? address2,
            int? cityId,
            int? districtId,
            int? subdistrictId,
            int? villageId,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateAddress",
                    "Cannot update deleted customer");

            Address1 = address1;
            Address2 = address2;
            CityId = cityId;
            DistrictId = districtId;
            SubdistrictId = subdistrictId;
            VillageId = villageId;

            UpdateAuditInfo(updatedBy);
        }

        public void UpdateIdentification(
            IdentificationType identificationType,
            string identificationNumber,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateIdentification",
                    "Cannot update deleted customer");

            IdentificationType = identificationType;
            IdentificationNumber = identificationNumber;

            UpdateAuditInfo(updatedBy);
            Validate();
        }

        public void UpdateProfessionalInfo(
            int? occupationId,
            int? nationalityCountryId,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateProfessionalInfo",
                    "Cannot update deleted customer");

            OccupationId = occupationId;
            NationalityCountryId = nationalityCountryId;

            UpdateAuditInfo(updatedBy);
        }

        public void UpdateStatus(CustomerStatus status, string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateStatus",
                    "Cannot update deleted customer");

            var oldStatus = Status;
            Status = status;

            UpdateAuditInfo(updatedBy);

            if (oldStatus != status)
            {
                AddDomainEvent(new CustomerStatusChangedEvent(this, oldStatus, status, updatedBy));
            }
        }

        public void UpdateNotes(string? notes, string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateNotes",
                    "Cannot update deleted customer");

            Notes = notes;
            UpdateAuditInfo(updatedBy);
        }

        // Soft delete customer
        public void Delete(string deletedBy)
        {
            if (IsDeleted)
                return; // Already deleted

            // Check if customer has active policies
            var activePolicies = GetActivePolicies();
            if (activePolicies.Any())
            {
                throw new BusinessRuleViolationException(
                    "CustomerDeletion",
                    "Cannot delete customer with active policies");
            }

            IsDeleted = true;
            DeletedOn = DateTime.UtcNow;
            DeletedBy = deletedBy;
            Status = CustomerStatus.Inactive;

            AddDomainEvent(new CustomerDeletedEvent(this, deletedBy));
        }

        // Restore deleted customer
        public void Restore(string restoredBy)
        {
            if (!IsDeleted)
                return; // Not deleted

            IsDeleted = false;
            DeletedOn = null;
            DeletedBy = null;
            Status = CustomerStatus.Active;

            UpdateAuditInfo(restoredBy);
            AddDomainEvent(new CustomerRestoredEvent(this, restoredBy));
        }

        // Business logic methods
        public List<Policy> GetActivePolicies()
        {
            return Policies?.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted).ToList() ?? new List<Policy>();
        }

        public List<Policy> GetPoliciesByStatus(PolicyStatus status)
        {
            return Policies?.Where(p => p.Status == status && !p.IsDeleted).ToList() ?? new List<Policy>();
        }

        public decimal GetTotalPremiums()
        {
            return Policies?.Where(p => !p.IsDeleted).Sum(p => p.PremiumAmount) ?? 0;
        }

        public decimal GetTotalCommissions()
        {
            return Policies?.Where(p => !p.IsDeleted).Sum(p => p.CommissionAmount) ?? 0;
        }

        public DateTime? GetLastPolicyDate()
        {
            return Policies?.Where(p => !p.IsDeleted).Max(p => p?.CreatedOn);
        }

        public bool HasActivePolicies()
        {
            return GetActivePolicies().Any();
        }

        public bool IsEligibleForNewPolicy()
        {
            return Status == CustomerStatus.Active &&
                   !IsDeleted &&
                   Age >= 18;
        }

        public int GetPolicyCount()
        {
            return Policies?.Count(p => !p.IsDeleted) ?? 0;
        }

        public decimal GetAveragePolicyValue()
        {
            var policies = Policies?.Where(p => !p.IsDeleted).ToList();
            if (policies == null || !policies.Any())
                return 0;

            return policies.Average(p => p.PremiumAmount);
        }
    }
}