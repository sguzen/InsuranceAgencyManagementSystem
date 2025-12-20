using IAMS.Infrastructure.Interfaces;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IReportingApiClient
    {
        Task<Result<List<ReportTemplate>>> GetAvailableReportsAsync();
        Task<Result<ReportResult>> GenerateReportAsync(string reportType, ReportParameters parameters);
        Task<byte[]?> ExportReportAsync(string reportType, ReportParameters parameters, string format = "pdf");
        Task<Result<List<ScheduledReport>>> GetScheduledReportsAsync();
        Task<Result<bool>> ScheduleReportAsync(ScheduledReport report);
        Task<Result<bool>> CancelScheduledReportAsync(int reportId);
    }

    public class ReportingApiClient : BaseApiClient, IReportingApiClient
    {
        public ReportingApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<ReportTemplate>>> GetAvailableReportsAsync()
        {
            return await GetAsync<List<ReportTemplate>>("api/reporting/templates");
        }

        public async Task<Result<ReportResult>> GenerateReportAsync(string reportType, ReportParameters parameters)
        {
            return await PostAsync<ReportResult>("api/reporting/generate", new
            {
                ReportType = reportType,
                Parameters = parameters
            });
        }

        public async Task<byte[]?> ExportReportAsync(string reportType, ReportParameters parameters, string format = "pdf")
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/reporting/export", new
                {
                    ReportType = reportType,
                    Parameters = parameters,
                    Format = format
                }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Result<List<ScheduledReport>>> GetScheduledReportsAsync()
        {
            return await GetAsync<List<ScheduledReport>>("api/reporting/scheduled");
        }

        public async Task<Result<bool>> ScheduleReportAsync(ScheduledReport report)
        {
            return await PostAsync<bool>("api/reporting/scheduled", report);
        }

        public async Task<Result<bool>> CancelScheduledReportAsync(int reportId)
        {
            return await base.DeleteAsync($"api/reporting/scheduled/{reportId}")
                .ContinueWith(t => t.Result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(t.Result.Message));
        }
    }
}
