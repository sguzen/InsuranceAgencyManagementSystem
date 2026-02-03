namespace IAMS.Shared.Constants
{
    /// <summary>
    /// Application-wide constants to avoid magic strings
    /// </summary>
    public static class ApplicationConstants
    {
        /// <summary>
        /// Connection string names
        /// </summary>
        public static class ConnectionStrings
        {
            public const string DefaultConnection = "DefaultConnection";
            public const string MasterConnection = "MasterConnection";
            public const string IntegrationConnection = "IntegrationConnection";
        }

        /// <summary>
        /// Configuration section names
        /// </summary>
        public static class ConfigurationSections
        {
            public const string JwtSettings = "JwtSettings";
            public const string EmailSettings = "EmailSettings";
            public const string FileStorageSettings = "FileStorageSettings";
            public const string MultiTenancy = "MultiTenancy";
            public const string TenantConnections = "TenantConnections";
            public const string MySqlImport = "MySqlImport";
        }

        /// <summary>
        /// JWT configuration keys
        /// </summary>
        public static class JwtConfiguration
        {
            public const string Secret = "JwtSettings:Secret";
            public const string Issuer = "JwtSettings:Issuer";
            public const string Audience = "JwtSettings:Audience";
            public const string ExpiryMinutes = "JwtSettings:ExpiryMinutes";
            public const string RefreshTokenExpiryDays = "JwtSettings:RefreshTokenExpiryDays";
        }

        /// <summary>
        /// HTTP header names
        /// </summary>
        public static class Headers
        {
            public const string TenantId = "X-Tenant-ID";
            public const string Authorization = "Authorization";
            public const string ContentType = "Content-Type";
        }

        /// <summary>
        /// Claim type names
        /// </summary>
        public static class ClaimTypes
        {
            public const string Permission = "permission";
            public const string TenantId = "tenant_id";
            public const string TenantName = "tenant_name";
        }

        /// <summary>
        /// Multi-tenancy configuration
        /// </summary>
        public static class MultiTenancy
        {
            public const string HeaderStrategy = "Header";
            public const string SubdomainStrategy = "Subdomain";
            public const string DatabaseStrategy = "Database";
            public const string DefaultTenant = "default";
        }

        /// <summary>
        /// Cache key prefixes
        /// </summary>
        public static class CacheKeys
        {
            public const string TenantPrefix = "tenant:";
            public const string UserPermissionsPrefix = "permissions:user:";
            public const string RolePermissionsPrefix = "permissions:role:";
            public const string SettingsPrefix = "settings:";
        }

        /// <summary>
        /// Default values
        /// </summary>
        public static class Defaults
        {
            public const int PageSize = 10;
            public const int MaxPageSize = 100;
            public const int DefaultCacheExpirationMinutes = 30;
            public const int CommandTimeoutSeconds = 30;
        }

        /// <summary>
        /// File storage constants
        /// </summary>
        public static class FileStorage
        {
            public const string LocalStorageType = "Local";
            public const string AzureStorageType = "Azure";
            public const string AwsStorageType = "AWS";
            public const string DefaultUploadPath = "wwwroot/uploads";
        }

        /// <summary>
        /// Email templates
        /// </summary>
        public static class EmailTemplates
        {
            public const string Welcome = "Welcome";
            public const string PasswordReset = "PasswordReset";
            public const string PolicyReminder = "PolicyReminder";
            public const string PolicyExpiring = "PolicyExpiring";
        }

        /// <summary>
        /// Module names
        /// </summary>
        public static class Modules
        {
            public const string Policies = "Policies";
            public const string Customers = "Customers";
            public const string Claims = "Claims";
            public const string Payments = "Payments";
            public const string Reports = "Reports";
            public const string Settings = "Settings";
            public const string UserManagement = "UserManagement";
        }

        /// <summary>
        /// Permission categories
        /// </summary>
        public static class PermissionCategories
        {
            public const string Policies = "Policies";
            public const string Customers = "Customers";
            public const string Claims = "Claims";
            public const string Payments = "Payments";
            public const string Reports = "Reports";
            public const string Administration = "Administration";
        }

        /// <summary>
        /// Validation messages
        /// </summary>
        public static class ValidationMessages
        {
            public const string Required = "{PropertyName} is required";
            public const string MaxLength = "{PropertyName} must not exceed {MaxLength} characters";
            public const string EmailFormat = "Invalid email format";
            public const string InvalidDate = "{PropertyName} must be a valid date";
            public const string MustBePositive = "{PropertyName} must be greater than zero";
        }

        /// <summary>
        /// Error messages
        /// </summary>
        public static class ErrorMessages
        {
            public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
            public const string NotFound = "The requested resource was not found.";
            public const string Unauthorized = "You are not authorized to perform this action.";
            public const string ConcurrencyConflict = "The record was modified by another user. Please refresh and try again.";
            public const string DatabaseError = "A database error occurred. Please try again.";
            public const string ValidationFailed = "Validation failed. Please check your input.";
        }
    }
}
