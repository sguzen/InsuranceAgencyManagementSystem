using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Interfaces
{
    public interface IReportingService
    {
        Task<ReportResult> GenerateReportAsync(string reportType, ReportParameters parameters);
        Task<byte[]> ExportReportAsync(string reportType, ReportParameters parameters, string format = "pdf");
        Task<bool> ScheduleReportAsync(ScheduledReport report);
        Task<List<ScheduledReport>> GetScheduledReportsAsync();
        Task<bool> CancelScheduledReportAsync(int reportId);
        Task<List<ReportTemplate>> GetAvailableReportsAsync();
    }

    public class ReportParameters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int> CustomerIds { get; set; } = new();
        public List<string> PolicyTypes { get; set; } = new();
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    public class ReportResult
    {
        public string ReportType { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string? FilePath { get; set; }
    }

    public class ScheduledReport
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public ReportParameters Parameters { get; set; } = new();
        public string CronExpression { get; set; } = string.Empty;
        public string EmailRecipients { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
    }

    public class ReportTemplate
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> SupportedFormats { get; set; } = new();
        public bool RequiresDateRange { get; set; }
        public List<ReportParameter> Parameters { get; set; } = new();
    }

    public class ReportParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public object? DefaultValue { get; set; }
        public List<object> Options { get; set; } = new();
    }
}
