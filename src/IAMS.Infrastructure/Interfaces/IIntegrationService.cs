using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Interfaces
{
    public interface IIntegrationService
    {
        Task<IntegrationResult> SyncCustomerDataAsync(int customerId);
        Task<IntegrationResult> SyncPolicyDataAsync(int policyId);
        Task<IntegrationResult> SubmitClaimAsync(int claimId);
        Task<List<IntegrationLog>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> TestConnectionAsync(string providerName);
        Task<List<IntegrationProvider>> GetAvailableProvidersAsync();
    }

    public class IntegrationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class IntegrationLog
    {
        public int Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
    }

    public class IntegrationProvider
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastSync { get; set; }
        public Dictionary<string, string> Settings { get; set; } = new();
    }
}
