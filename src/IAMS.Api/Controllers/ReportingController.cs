using IAMS.Infrastructure.Interfaces;
using IAMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportingController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        /// <summary>
        /// Get available report templates
        /// </summary>
        [HttpGet("templates")]
        public async Task<ActionResult<Result<List<ReportTemplate>>>> GetAvailableReports()
        {
            try
            {
                var templates = await _reportingService.GetAvailableReportsAsync();
                return Ok(Result<List<ReportTemplate>>.Success(templates));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<List<ReportTemplate>>.Failure($"Error retrieving report templates: {ex.Message}", ex.ToString()));
            }
        }

        /// <summary>
        /// Generate a report
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<Result<ReportResult>>> GenerateReport([FromBody] GenerateReportRequest request)
        {
            try
            {
                var result = await _reportingService.GenerateReportAsync(request.ReportType, request.Parameters);
                return Ok(Result<ReportResult>.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<ReportResult>.Failure($"Error generating report: {ex.Message}", ex.ToString()));
            }
        }

        /// <summary>
        /// Export a report to a specific format
        /// </summary>
        [HttpPost("export")]
        public async Task<ActionResult> ExportReport([FromBody] ExportReportRequest request)
        {
            try
            {
                var fileBytes = await _reportingService.ExportReportAsync(request.ReportType, request.Parameters, request.Format);
                var contentType = request.Format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/octet-stream"
                };

                return File(fileBytes, contentType, $"report.{request.Format}");
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Failure($"Error exporting report: {ex.Message}", ex.ToString()));
            }
        }

        /// <summary>
        /// Get scheduled reports
        /// </summary>
        [HttpGet("scheduled")]
        public async Task<ActionResult<Result<List<ScheduledReport>>>> GetScheduledReports()
        {
            try
            {
                var reports = await _reportingService.GetScheduledReportsAsync();
                return Ok(Result<List<ScheduledReport>>.Success(reports));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<List<ScheduledReport>>.Failure($"Error retrieving scheduled reports: {ex.Message}", ex.ToString()));
            }
        }

        /// <summary>
        /// Schedule a report
        /// </summary>
        [HttpPost("scheduled")]
        public async Task<ActionResult<Result<bool>>> ScheduleReport([FromBody] ScheduledReport report)
        {
            try
            {
                var success = await _reportingService.ScheduleReportAsync(report);
                return Ok(Result<bool>.Success(success));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<bool>.Failure($"Error scheduling report: {ex.Message}", ex.ToString()));
            }
        }

        /// <summary>
        /// Cancel a scheduled report
        /// </summary>
        [HttpDelete("scheduled/{reportId}")]
        public async Task<ActionResult<Result<bool>>> CancelScheduledReport(int reportId)
        {
            try
            {
                var success = await _reportingService.CancelScheduledReportAsync(reportId);
                return Ok(Result<bool>.Success(success));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<bool>.Failure($"Error canceling scheduled report: {ex.Message}", ex.ToString()));
            }
        }
    }

    // Request models
    public class GenerateReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public ReportParameters Parameters { get; set; } = new();
    }

    public class ExportReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public ReportParameters Parameters { get; set; } = new();
        public string Format { get; set; } = "pdf";
    }
}
