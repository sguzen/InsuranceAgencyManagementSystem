namespace IAMS.Domain.Services
{
    public interface IClaimNumberGenerator
    {
        string GenerateClaimNumber(int tenantId, int policyId);
        Task<string> GenerateClaimNumberAsync(int tenantId, int policyId);
        bool IsValidClaimNumber(string claimNumber);
    }
}