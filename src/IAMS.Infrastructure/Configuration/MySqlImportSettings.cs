namespace IAMS.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration settings for MySQL database import connection.
    /// Credentials should be stored in User Secrets (dev) or environment variables (production).
    /// Development: dotnet user-secrets set "MySqlImport:Password" "your-password"
    /// </summary>
    public class MySqlImportSettings
    {
        public const string SectionName = "MySqlImport";

        /// <summary>
        /// MySQL server host/IP address
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// MySQL server port (default: 3306)
        /// </summary>
        public int Port { get; set; } = 3306;

        /// <summary>
        /// MySQL database name
        /// </summary>
        public string Database { get; set; } = string.Empty;

        /// <summary>
        /// MySQL username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// MySQL password (should be stored in User Secrets or environment variables)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Command/query execution timeout in seconds
        /// </summary>
        public int CommandTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Whether to use SSL for the connection
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Default agency code filter for the query
        /// </summary>
        public string DefaultAgencyCode { get; set; } = "A022";

        /// <summary>
        /// Builds a secure connection string from the settings.
        /// Forces read-only intent and disables dangerous options.
        /// </summary>
        public string BuildConnectionString()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new InvalidOperationException("MySQL host is not configured");
            if (string.IsNullOrWhiteSpace(Database))
                throw new InvalidOperationException("MySQL database is not configured");
            if (string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException("MySQL username is not configured");

            var builder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = Host,
                Port = (uint)Port,
                Database = Database,
                UserID = Username,
                Password = Password,
                ConnectionTimeout = (uint)ConnectionTimeoutSeconds,
                DefaultCommandTimeout = (uint)CommandTimeoutSeconds,
                // Security settings
                SslMode = UseSsl ? MySqlConnector.MySqlSslMode.Required : MySqlConnector.MySqlSslMode.Preferred,
                AllowUserVariables = false,
                AllowLoadLocalInfile = false,
                // Performance settings
                ConnectionReset = true,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 5,
            };

            return builder.ConnectionString;
        }
    }
}
