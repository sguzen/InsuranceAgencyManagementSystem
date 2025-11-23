namespace IAMS.Application.Configuration
{
    /// <summary>
    /// Configuration settings for premium calculation
    /// </summary>
    public class PremiumCalculationSettings
    {
        /// <summary>
        /// Policy types that should use external calculation service
        /// </summary>
        public List<string> ExternalCalculationPolicyTypes { get; set; } = new();

        /// <summary>
        /// Whether to fallback to internal calculator if external service fails
        /// </summary>
        public bool FallbackToInternalOnFailure { get; set; } = true;

        /// <summary>
        /// External service configuration
        /// </summary>
        public ExternalServiceSettings ExternalService { get; set; } = new();
    }

    /// <summary>
    /// External calculation service configuration
    /// </summary>
    public class ExternalServiceSettings
    {
        /// <summary>
        /// Base URL of the external calculation service
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API endpoint for premium calculation
        /// </summary>
        public string CalculationEndpoint { get; set; } = "/api/premium/calculate";

        /// <summary>
        /// Health check endpoint
        /// </summary>
        public string HealthCheckEndpoint { get; set; } = "/api/health";

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// API key for authentication (if required)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Maximum retry attempts for failed requests
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Whether the external service is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
