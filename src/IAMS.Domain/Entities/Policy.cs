using IAMS.Domain.Enums;
using IAMS.Domain.ValueObjects;
using IAMS.Domain.Events;
using IAMS.Domain.Exceptions;

namespace IAMS.Domain.Entities
{
    public class Policy : BaseEntity
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
        public string Currency { get; set; } = "TRY";

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
        public virtual PolicyType PolicyType { get; set; } = null!;
        public virtual ICollection<PolicyPayment> PolicyPayments { get; set; } = new List<PolicyPayment>();
        public virtual ICollection<PolicyClaim> PolicyClaims { get; set; } = new List<PolicyClaim>();

        // Value objects
        public Money GetPremiumMoney() => new Money(PremiumAmount, Currency);
        public Money GetCommissionMoney() => new Money(CommissionAmount, Currency);
        public DateRange GetPolicyPeriod() => new DateRange(StartDate, EndDate);

        // Business methods
        public void ActivatePolicy(string activatedBy)
        {
            if (Status != PolicyStatus.Draft)
                throw new InvalidOperationDomainException(
                    "ActivatePolicy",
                    "Only draft policies can be activated",
                    TenantId);

            if (StartDate > DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyActivation",
                    "Cannot activate a policy with a future start date",
                    TenantId);

            Status = PolicyStatus.Active;
            UpdateAuditInfo(activatedBy);
            AddDomainEvent(new PolicyCreatedEvent(this, activatedBy));
        }

        public void CancelPolicy(string cancelledBy, string? reason = null)
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "CancelPolicy",
                    "Only active or suspended policies can be cancelled",
                    TenantId);

            Status = PolicyStatus.Cancelled;
            UpdateAuditInfo(cancelledBy);
            AddDomainEvent(new PolicyCancelledEvent(this, cancelledBy, reason));
        }

        public void SuspendPolicy(string suspendedBy, string? reason = null)
        {
            if (Status != PolicyStatus.Active)
                throw new InvalidOperationDomainException(
                    "SuspendPolicy",
                    "Only active policies can be suspended",
                    TenantId);

            Status = PolicyStatus.Suspended;
            UpdateAuditInfo(suspendedBy);
        }

        public void ExpirePolicy()
        {
            if (EndDate <= DateTime.Today && Status == PolicyStatus.Active)
            {
                Status = PolicyStatus.Expired;
                AddDomainEvent(new PolicyExpiredEvent(this));
            }
        }

        public bool IsActive => Status == PolicyStatus.Active && !IsExpired;
        public bool IsExpired => EndDate < DateTime.Today;
        public bool CanRenew => Status == PolicyStatus.Expired ||
                                (Status == PolicyStatus.Active && EndDate <= DateTime.Today.AddDays(60));

        public decimal CalculateTotalPaid()
        {
            return PolicyPayments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);
        }

        public decimal GetOutstandingAmount()
        {
            return PremiumAmount - CalculateTotalPaid();
        }

        public bool IsFullyPaid()
        {
            return GetOutstandingAmount() <= 0;
        }

        protected override void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(PolicyNumber))
                errors.Add("Policy number is required");

            if (CustomerId <= 0)
                errors.Add("Customer ID is required");

            if (InsuranceCompanyId <= 0)
                errors.Add("Insurance Company ID is required");

            if (PolicyTypeId <= 0)
                errors.Add("Policy Type ID is required");

            if (StartDate >= EndDate)
                errors.Add("Start date must be before end date");

            if (PremiumAmount <= 0)
                errors.Add("Premium amount must be greater than zero");

            if (CommissionRate < 0 || CommissionRate > 100)
                errors.Add("Commission rate must be between 0 and 100");

            if (errors.Any())
                throw new PolicyValidationException(this, errors);
        }
    }
}