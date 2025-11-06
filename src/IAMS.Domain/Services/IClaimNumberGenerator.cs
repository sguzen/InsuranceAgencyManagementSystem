namespace IAMS.Domain.Services
{
    public interface IClaimNumberGenerator
    {
        string GenerateClaimNumber(int policyId);
        Task<string> GenerateClaimNumberAsync(int policyId);
        bool IsValidClaimNumber(string claimNumber);
    }
}