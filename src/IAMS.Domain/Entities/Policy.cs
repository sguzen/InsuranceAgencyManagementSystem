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

            StartDate = startDate;
            EndDate = endDate;

            UpdateAuditInfo(updatedBy);
            AddDomainEvent(new PolicyDatesUpdatedEvent(this, updatedBy));
        }

        // Update premium amount
        public void UpdatePremium(decimal premiumAmount, string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdatePremium",
                    "Cannot update deleted policy",
                    TenantId);

            if (Status == PolicyStatus.Cancelled)
                throw new InvalidOperationDomainException(
                    "UpdatePremium",
                    "Cannot update premium of cancelled policy",
                    TenantId);

            if (premiumAmount <= 0)
                throw new BusinessRuleViolationException(
                    "PolicyPremium",
                    "Premium amount must be greater than zero",
                    TenantId);

            var oldPremium = PremiumAmount;
            PremiumAmount = premiumAmount;

            // Recalculate commission with existing rate
            CommissionAmount = premiumAmount * (CommissionRate / 100);

            UpdateAuditInfo(updatedBy);
            AddDomainEvent(new PolicyPremiumUpdatedEvent(this, oldPremium, premiumAmount, updatedBy));
        }

        // Update notes
        public void UpdateNotes(string? notes, string updatedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationDomainException(
                    "UpdateNotes",
                    "Cannot update deleted policy",
                    TenantId);

            Notes = notes;
            UpdateAuditInfo(updatedBy);
        }

        // Renew policy
        public Policy RenewPolicy(
            DateTime newStartDate,
            DateTime newEndDate,
            decimal newPremiumAmount,
            decimal newCommissionRate,
            string renewedBy)
        {
            if (!CanRenew)
                throw new BusinessRuleViolationException(
                    "PolicyRenewal",
                    "Policy cannot be renewed in current state",
                    TenantId);

            var renewalPolicy = Create(
                GenerateRenewalPolicyNumber(),
                CustomerId,
                InsuranceCompanyId,
                PolicyTypeId,
                newStartDate,
                newEndDate,
                newPremiumAmount,
                newCommissionRate,
                renewedBy,
                TenantId,
                Currency);

            renewalPolicy.Notes = $"Renewal of policy {PolicyNumber}";

            AddDomainEvent(new PolicyRenewedEvent(this, renewalPolicy, renewedBy));

            return renewalPolicy;
        }

        private Policy Create(string v, int customerId, int insuranceCompanyId, int policyTypeId, DateTime newStartDate, DateTime newEndDate, decimal newPremiumAmount, decimal newCommissionRate, string renewedBy, int tenantId, string currency)
        {
            throw new NotImplementedException();
        }

        private string GenerateRenewalPolicyNumber()
        {
            // Simple renewal numbering - in real implementation, use IPolicyNumberGenerator
            return $"{PolicyNumber}-R{DateTime.Now:yyyyMMdd}";
        }

        // Get policy performance metrics
        public decimal GetPaymentCompletionRate()
        {
            if (PremiumAmount == 0) return 100;

            var totalPaid = CalculateTotalPaid();
            return (totalPaid / PremiumAmount) * 100;
        }

        public int GetDaysActive()
        {
            if (Status != PolicyStatus.Active) return 0;

            var activeFrom = Math.Max(StartDate.Ticks, CreatedOn.Ticks);
            var activeTo = Math.Min(DateTime.Today.Ticks, EndDate.Ticks);

            return (int)TimeSpan.FromTicks(activeTo - activeFrom).TotalDays;
        }

        public bool IsNearExpiry(int daysThreshold = 30)
        {
            return Status == PolicyStatus.Active &&
                   EndDate <= DateTime.Today.AddDays(daysThreshold) &&
                   EndDate >= DateTime.Today;
        }

        public void Delete(string v)
        {
            throw new NotImplementedException();
        }
    }
}