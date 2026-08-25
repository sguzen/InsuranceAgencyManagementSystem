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
        /// Gets the raw credential details (decrypted) for building import configurations.
        /// Returns null if no credentials are configured.
        /// </summary>
        Task<AgencyCredentialDetails?> GetCredentialDetailsAsync(int agencyId, int insuranceCompanyId);

        /// <summary>
        /// Tests connection using stored credentials.
        /// </summary>
        Task<(bool Success, string Message)> TestConnectionAsync(int agencyId, int insuranceCompanyId);
    }

    /// <summary>
    /// Raw credential details for an agency-insurance company link (with decrypted password).
    /// </summary>
    public class AgencyCredentialDetails
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 3306;
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsMySql { get; set; }

        /// <summary>
        /// Agency code assigned by this insurer (AgencyInsuranceCompany.AgencyCode),
        /// falling back to the tenant's ExternalId for legacy links. Null if neither is set.
        /// </summary>
        public string? AgencyCode { get; set; }
    }
}
