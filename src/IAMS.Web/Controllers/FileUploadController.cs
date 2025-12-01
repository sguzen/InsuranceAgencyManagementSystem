using Microsoft.AspNetCore.Mvc;

namespace IAMS.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[IgnoreAntiforgeryToken]
public class FileUploadController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(HttpClient httpClient, ILogger<FileUploadController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpPost("policies/import")]
    public async Task<IActionResult> ImportPolicies(IFormFile file)
    {
        _logger.LogInformation("FileUploadController.ImportPolicies called");
        _logger.LogInformation("HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);

        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded");
                return BadRequest("No file uploaded");
            }

            _logger.LogInformation("Received file: {FileName}, Size: {Size} bytes", file.FileName, file.Length);

            // Create multipart form content
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            _logger.LogInformation("Forwarding to API: {Url}", $"{_httpClient.BaseAddress}api/policies/import");

            // Forward to API
            var response = await _httpClient.PostAsync("api/policies/import", content);

            _logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the import page on success
                return Redirect("/policies/import?success=true");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError("API returned error: {Error}", errorMessage);
                return Redirect($"/policies/import?error={Uri.EscapeDataString(errorMessage)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return Redirect($"/policies/import?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}
