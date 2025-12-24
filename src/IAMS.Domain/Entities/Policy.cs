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
        public string? TecditNumber { get; set; } // Tecdit/endorsement number from Tec column

        // Policy Owner - The Cari Kart customer who pays for the policy
        public int CustomerId { get; set; }

        // Sigortalı (Insured Person) - Stored as string: "Name - Kimlik/Pasaport"
        // The person who is insured (may or may not be the same as CustomerId)
        public string? EnsuredEntity { get; set; }

        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; } = null!;
        // Exchange rate at time of policy creation (for historical accuracy)
        public decimal? ExchangeRateToBase { get; set; } // Rate to TRY at creation time
        public decimal? PremiumAmountInBaseCurrency { get; set; } // Calculated amount in TRY

        // Vehicle Information (for car insurance)
        public int? VehicleId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }

        // Traffic Insurance Specific Fields
        public string? TrafficPolicyNumber { get; set; } // Trafik Poliçe No (for insurance company)
        public DateTime? TrafficStartDate { get; set; }
        public DateTime? TrafficEndDate { get; set; }

        // Comprehensive Insurance Specific Fields (Kasko)
        public string? ComprehensivePolicyNumber { get; set; } // Kasko Poliçe No
        public decimal? DeductibleAmount { get; set; } // Muafiyet Tutarı
        public bool? HasGlassCoverage { get; set; } // Cam Teminatı
        public bool? HasTheftCoverage { get; set; } // Hırsızlık Teminatı
        public bool? HasNaturalDisasterCoverage { get; set; } // Doğal Afet Teminatı
        public bool? HasDriverAccidentCoverage { get; set; } // Sürücü Ferdi Kaza
        public int? DriverAccidentCoverageAmount { get; set; } // Sürücü FK Teminat Tutarı

        // Discount Information
        public decimal? NoClaimDiscountRate { get; set; } // Hasar İndirimi Oranı
        public int? NoClaimYears { get; set; } // Hasarsız Geçen Yıl Sayısı
        public decimal? FleetDiscountRate { get; set; } // Filo İndirimi

        // Previous Policy Information
        public string? PreviousPolicyNumber { get; set; }
        public int? PreviousInsuranceCompanyId { get; set; }
        public DateTime? PreviousPolicyEndDate { get; set; }

        // Renewal Tracking - Reference to the parent policy if this is a renewal
        public int? ParentPolicyId { get; set; }
        public virtual Policy? ParentPolicy { get; set; }

        // Endorsement Information (Zeyilname)
        // All policies are treated as endorsements: 000 = main policy, 001+ = endorsements
        public string InnerCode { get; set; } = "000"; // 3-digit code: 000 for main, 001, 002, etc.
        public StateType StateType { get; set; } = StateType.YeniPolice; // TIP column (P, T, V, R, X, Y)

        public int? OriginalPolicyId { get; set; } // Reference to original policy if InnerCode != "000"
        public virtual Policy? OriginalPolicy { get; set; }
        public string? BranchCode { get; set; } // External code for insurance type (Kod column)

        // Driver Information
        public int? DriverAge { get; set; } // YAS column
        public DriverType? DriverType { get; set; } // Sürücü - Single or Any

        // Marketer Information (Pazarlamacı)
        public string? Marketer { get; set; } // Pazarlamacı Adı - stored as string

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!
        public virtual PolicyType PolicyType { get; set; } = null!;
        public virtual ICollection<PolicyPayment> PolicyPayments { get; set; } = new List<PolicyPayment>();
        public virtual ICollection<PolicyClaim> PolicyClaims { get; set; } = new List<PolicyClaim>();

        public virtual InsuranceCompany? PreviousInsuranceCompany { get; set; }


        // Value objects
        public Money GetPremiumMoney() => new Money(PremiumAmount, CurrencyId);
        public Money GetCommissionMoney() => new Money(CommissionAmount, CurrencyId);
        public DateRange GetPolicyPeriod() => new DateRange(StartDate, EndDate);

        // Business methods
        public void ActivatePolicy(string activatedBy)
        {
            if (Status != PolicyStatus.Draft)
                throw new InvalidOperationDomainException(
                    "ActivatePolicy",
                    "Only draft policies can be activated");

            if (StartDate > DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyActivation",
                    "Cannot activate a policy with a future start date");

            Status = PolicyStatus.Active;
            UpdateAuditInfo(activatedBy);
            AddDomainEvent(new PolicyCreatedEvent(this, activatedBy));
        }

        public void CancelPolicy(string cancelledBy, string? reason = null)
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "CancelPolicy",
                    "Only active or suspended policies can be cancelled");

            Status = PolicyStatus.Cancelled;
            UpdateAuditInfo(cancelledBy);
            AddDomainEvent(new PolicyCancelledEvent(this, cancelledBy, reason));
        }

        public void SuspendPolicy(string suspendedBy, string? reason = null)
        {
            if (Status != PolicyStatus.Active)
                throw new InvalidOperationDomainException(
                    "SuspendPolicy",
                    "Only active policies can be suspended");

            Status = PolicyStatus.Suspended;
            UpdateAuditInfo(suspendedBy);
            AddDomainEvent(new PolicySuspendedEvent(this, suspendedBy, reason));
        }

        public void ReactivatePolicy(string reactivatedBy)
        {
            if (Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "ReactivatePolicy",
                    "Only suspended policies can be reactivated");

            if (EndDate < DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyReactivation",
                    "Cannot reactivate an expired policy");

            Status = PolicyStatus.Active;
            UpdateAuditInfo(reactivatedBy);
            AddDomainEvent(new PolicyReactivatedEvent(this, reactivatedBy));
        }

        public void ExpirePolicy()
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Suspended)
                throw new InvalidOperationDomainException(
                    "ExpirePolicy",
                    "Only active or suspended policies can be expired");

            if (EndDate > DateTime.Today)
                throw new BusinessRuleViolationException(
                    "PolicyExpiry",
                    "Cannot expire a policy before its end date");

            Status = PolicyStatus.Expired;
            AddDomainEvent(new PolicyExpiredEvent(this));
        }

        public Policy CreateRenewal(DateTime newStartDate, DateTime newEndDate,
            decimal newPremiumAmount, string renewedBy)
        {
            if (Status != PolicyStatus.Active && Status != PolicyStatus.Expired)
                throw new InvalidOperationDomainException(
                    "CreateRenewal",
                    "Only active or expired policies can be renewed");

            if (newStartDate >= newEndDate)
                throw new BusinessRuleViolationException(
                    "PolicyRenewal",
                    "Renewal start date must be before end date");

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
                ParentPolicyId = this.Id, // Link to parent policy for renewal tracking
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
                    "Cannot update deleted policy");

            if (Status == PolicyStatus.Cancelled)
                throw new InvalidOperationDomainException(
                    "UpdateDates",
                    "Cannot update dates of cancelled policy");

            if (startDate >= endDate)
                throw new BusinessRuleViolationException(
                    "PolicyDates",
                    "Start date must be before end date");

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
                    "Premium amount must be greater than zero");

            if (Status == PolicyStatus.Cancelled)
                throw new InvalidOperationDomainException(
                    "UpdatePremium",
                    "Cannot update premium of cancelled policy");

            var oldPremium = PremiumAmount;
            PremiumAmount = newPremiumAmount;
            CalculateCommission();
            UpdateAuditInfo(updatedBy);

            AddDomainEvent(new PolicyPremiumUpdatedEvent(this, oldPremium, newPremiumAmount, updatedBy));
        }

        /// <summary>
        /// Calculates commission based on current premium and rate.
        /// NOTE: This is a legacy method. For new code, use ICommissionCalculator service
        /// which looks up rates from the database based on policy type and company.
        /// </summary>
        [Obsolete("Use ICommissionCalculator service instead for database-driven commission calculation")]
        public void CalculateCommission()
        {
            CommissionAmount = PremiumAmount * (CommissionRate / 100);
        }

        public void UpdateCommissionRate(decimal newCommissionRate, string updatedBy)
        {
            if (newCommissionRate < 0 || newCommissionRate > 100)
                throw new BusinessRuleViolationException(
                    "CommissionRateUpdate",
                    "Commission rate must be between 0 and 100");

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

            if (CurrencyId < 0)
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

        public void SetCurrencyAndConvert(int currencyId, decimal exchangeRate)
        {
            CurrencyId = currencyId;
            ExchangeRateToBase = exchangeRate;
            PremiumAmountInBaseCurrency = PremiumAmount * exchangeRate;
        }

        public decimal GetPremiumInCurrency(int targetCurrencyId, decimal exchangeRate)
        {
            if (targetCurrencyId == CurrencyId)
                return PremiumAmount;

            // Convert to base first, then to target
            var inBase = PremiumAmountInBaseCurrency ?? (PremiumAmount * (ExchangeRateToBase ?? 1));
            return inBase / exchangeRate;
        }
    }
}