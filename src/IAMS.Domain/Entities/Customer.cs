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
        public string? Address { get; set; }
        public string KktcNo { get; set; } = string.Empty; // Turkish Cypriot ID Number
        public DateTime DateOfBirth { get; set; }
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public CustomerType Type { get; set; } = CustomerType.Individual;
        public IdentificationType IdentificationType { get; set; } = IdentificationType.KkTcNo;
        public Gender Gender { get; set; } = Gender.Male;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; } = new List<CustomerInsuranceCompany>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive => Status == CustomerStatus.Active && !IsDeleted;
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

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

        public void UpdateContactInfo(string email, string phone, string? address, string updatedBy)
        {
            if (!IsValidEmail(email))
                throw new BusinessRuleViolationException(
                    "EmailValidation",
                    "Invalid email format");

            if (!IsValidPhone(phone))
                throw new BusinessRuleViolationException(
                    "PhoneValidation",
                    "Invalid phone format");

            Email = email;
            Phone = phone;
            Address = address;
            UpdateAuditInfo(updatedBy);
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

        private static bool IsValidTcNo(string kktcNo)
        {
            if (string.IsNullOrWhiteSpace(kktcNo) || kktcNo.Length != 11)
                return false;

            return kktcNo.All(char.IsDigit);
        }

        protected override void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FirstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(LastName))
                errors.Add("Last name is required");

            if (!IsValidEmail(Email))
                errors.Add("Valid email is required");

            if (!IsValidPhone(Phone))
                errors.Add("Valid phone number is required");

            if (IdentificationType == IdentificationType.KkTcNo && !IsValidTcNo(KktcNo))
                errors.Add("Valid TC number is required");

            if (DateOfBirth >= DateTime.Today)
                errors.Add("Date of birth must be in the past");

            if (Age < 0 || Age > 150)
                errors.Add("Invalid age");

            if (errors.Any())
                throw new CustomerValidationException(this, errors);
        }

        public static Customer Create(
            string customerCode,
            string firstName,
            string lastName,
            string email,
            string phone,
            string kktcNo,
            DateTime dateOfBirth,
            string createdBy,
            CustomerType type = CustomerType.Individual,
            string? address = null)
        {
            var customer = new Customer
            {
                CustomerCode = customerCode,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                KktcNo = kktcNo,
                DateOfBirth = dateOfBirth,
                Type = type,
                Address = address,
                Status = CustomerStatus.Active,
                CreatedBy = createdBy
            };

            customer.Validate();
            customer.AddDomainEvent(new CustomerRegisteredEvent(customer, createdBy));

            return customer;
        }
    public void UpdatePersonalInfo(
            string firstName,
            string lastName,
            string? kktcNo,
            DateTime? dateOfBirth,
            Gender? gender,
            string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdatePersonalInfo",
                    "Cannot update deleted customer");

            FirstName = firstName;
            LastName = lastName;
            KktcNo = kktcNo;
            if (dateOfBirth != null)
                DateOfBirth = dateOfBirth.Value;
            Gender = gender ?? Gender.Male;

            UpdateAuditInfo(updatedBy);
            Validate();
        }


        // Update customer status
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

        // Update notes
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