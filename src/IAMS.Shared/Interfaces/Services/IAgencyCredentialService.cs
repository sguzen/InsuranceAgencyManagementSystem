namespace IAMS.Shared.Interfaces.Services
{
    /// <summary>
    /// Securely manages agency-insurance company credentials.
    /// Encrypts passwords at rest and provides secure access for import operations.
    /// </summary>
    public interface IAgencyCredentialService
    {
        /// <summary>
        /// Saves credentials with encryption.
        /// </summary>
        Task SaveCredentialsAsync(int agencyInsuranceCompanyId, string? dbServer, string? dbName,
            string? dbUsername, string? dbPassword, string? connectionString);

        /// <summary>
        /// Gets decrypted connection string for import operations.
        /// Only call this from secure background services!
        /// </summary>
        Task<string?> GetConnectionStringAsync(int agencyId, int insuranceCompanyId);

        /// <summary>
        /// Tests connection using stored credentials.
        /// </summary>
        Task<(bool Success, string Message)> TestConnectionAsync(int agencyId, int insuranceCompanyId);
    }
}
