namespace IAMS.Domain.Exceptions
{
    public class CommissionCalculationException : DomainException
    {
        public int InsuranceCompanyId { get; }
        public int PolicyTypeId { get; }
        public DateTime EffectiveDate { get; }

        public CommissionCalculationException(int insuranceCompanyId, int policyTypeId, DateTime effectiveDate, string message, int tenantId = 0)
            : base("COMMISSION_CALCULATION_ERROR", message, tenantId)
        {
            InsuranceCompanyId = insuranceCompanyId;
            PolicyTypeId = policyTypeId;
            EffectiveDate = effectiveDate;
        }
    }
}