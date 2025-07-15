// File: src/IAMS.Domain/Entities/Policy.cs
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
            AddDomainEvent(new PolicySuspendedEvent(this, suspendedBy, reason));
        }

        public void ReactivatePolicy(string reactivatedBy)
        {
            if (Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "ReactivatePolicy",
                    "Only suspended policies can be reactivated",
                    TenantId);

            if (EndDate < DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyReactivation",
                    "Cannot reactivate an expired policy",
                    TenantId);

            Status = PolicyStatus.Active;
            UpdateAuditInfo(reactivatedBy);
            AddDomainEvent(new PolicyReactivatedEvent(this, reactivatedBy));
        }

        public void ExpirePolicy()
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "ExpirePolicy",
                    "Only active or suspended policies can be expired",
                    TenantId);

            if (EndDate > DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyExpiry",
                    "Cannot expire a policy before its end date",
                    TenantId);

            Status = PolicyStatus.Expired;
            AddDomainEvent(new PolicyExpiredEvent(this));
        }

        public Policy CreateRenewal(DateTime newStartDate, DateTime newEndDate,
            decimal newPremiumAmount, string renewedBy)
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Expired)
                throw new InvalidOperationDomainException(
                    "CreateRenewal",
                    "Only active or expired policies can be renewed",
                    TenantId);

            if (newStartDate >= newEndDate)
                throw new BusinessRuleViolationException(
                    "PolicyRenewal",
                    "Renewal start date must be before end date",
                    TenantId);

            var renewalPolicy = new Policy
            {
                PolicyNumber = GenerateRenewalPolicyNumber(),
                CustomerId = this.CustomerId,
                InsuranceCompanyId = this.InsuranceCompanyId,
                PolicyTypeId = this.PolicyTypeId,
                StartDate = newStartDate,
                EndDate = newEndDate,
                PremiumAmount = newPremiumAmount,
                CommissionRate = this.CommissionRate,
                Currency = this.Currency,
                Status = PolicyStatus.Draft,
                TenantId = this.TenantId
            };

            renewalPolicy.CalculateCommission();
            renewalPolicy.UpdateAuditInfo(renewedBy);

            AddDomainEvent(new PolicyRenewedEvent(this, renewalPolicy, renewedBy));

            return renewalPolicy;
        }

        // Update policy dates
        public void UpdateDates(DateTime startDate, DateTime endDate, string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateDates",
                    "Cannot update deleted policy",
                    TenantId);

            if (Status == PolicyStatus.Cancelled)
                throw new InvalidOperationDomainException(
                    "UpdateDates",
                    "Cannot update dates of cancelled policy",
                    TenantId);

            if (startDate >= endDate)
                throw new BusinessRuleViolationException(
                    "PolicyDates",
                    "Start date must be before end date",
                    TenantId);

            var oldStartDate = StartDate;
            var oldEndDate = EndDate;

            StartDate = startDate;
            EndDate = endDate;
            UpdateAuditInfo(updatedBy);

            AddDomainEvent(new PolicyDatesUpdatedEvent(this, updatedBy,
                oldStartDate, oldEndDate, startDate, endDate));
        }

        public void UpdatePremium(decimal newPremiumAmount, string updatedBy)
        {
            if (newPremiumAmount <= 0)
                throw new BusinessRuleViolationException(
                    "PremiumUpdate",
                    "Premium amount must be greater than zero",
                    TenantId);

            if (Status == PolicyStatus.Cancelled)
                throw new InvalidOperationDomainException(
                    "UpdatePremium",
                    "Cannot update premium of cancelled policy",
                    TenantId);

            var oldPremium = PremiumAmount;
            PremiumAmount = newPremiumAmount;
            CalculateCommission();
            UpdateAuditInfo(updatedBy);

            AddDomainEvent(new PolicyPremiumUpdatedEvent(this, oldPremium, newPremiumAmount, updatedBy));
        }

        public void CalculateCommission()
        {
            CommissionAmount = PremiumAmount * (CommissionRate / 100);
        }

        public void UpdateCommissionRate(decimal newCommissionRate, string updatedBy)
        {
            if (newCommissionRate < 0 || newCommissionRate > 100)
                throw new BusinessRuleViolationException(
                    "CommissionRateUpdate",
                    "Commission rate must be between 0 and 100",
                    TenantId);

            var oldRate = CommissionRate;
            CommissionRate = newCommissionRate;
            CalculateCommission();
            UpdateAuditInfo(updatedBy);

            AddDomainEvent(new PolicyCommissionRateUpdatedEvent(this, oldRate, newCommissionRate, updatedBy));
        }

        private string GenerateRenewalPolicyNumber()
        {
            // This should be implemented by a domain service
            // For now, append "-R" to indicate renewal
            return $"{PolicyNumber}-R{DateTime.Now:yyyyMMdd}";
        }

        // Enhanced validation
        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(PolicyNumber?.Trim()))
                errors.Add("Policy number is required");
            else if (!IsValidPolicyNumber(PolicyNumber))
                errors.Add("Policy number format is invalid");

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

            if (string.IsNullOrEmpty(Currency?.Trim()))
                errors.Add("Currency is required");

            if (errors.Any())
                throw new PolicyValidationException(this, errors);
        }

        private bool IsValidPolicyNumber(string policyNumber)
        {
            // Basic format validation - this should be configurable per insurance company
            return policyNumber.Length >= 3 && policyNumber.Length <= 50 &&
                   policyNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '/' || c == '_');
        }

        public bool IsExpiring(int daysAhead = 30)
        {
            return Status == PolicyStatus.Active &&
                   EndDate.Date <= DateTime.Today.AddDays(daysAhead) &&
                   EndDate.Date >= DateTime.Today;
        }

        public bool IsExpired()
        {
            return EndDate.Date < DateTime.Today;
        }

        public decimal GetTotalPremiumPaid()
        {
            return PolicyPayments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);
        }

        public decimal GetOutstandingPremium()
        {
            return PremiumAmount - GetTotalPremiumPaid();
        }

        public bool HasOverduePayments()
        {
            return PolicyPayments
                .Any(p => p.Status == PaymentStatus.Pending && p.PaymentDate < DateTime.Today);
        }
    }
}