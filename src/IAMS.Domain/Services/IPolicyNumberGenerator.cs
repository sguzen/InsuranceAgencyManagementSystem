namespace IAMS.Domain.Services
{
    public interface IPolicyNumberGenerator
    {
        Task<string> GenerateAsync(int insuranceCompanyId, int policyTypeId);
        Task<string> GenerateRenewalNumberAsync(string originalPolicyNumber);
        bool IsValidFormat(string policyNumber, int insuranceCompanyId);
    }
}