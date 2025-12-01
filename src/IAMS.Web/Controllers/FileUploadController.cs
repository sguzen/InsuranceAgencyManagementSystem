using Microsoft.AspNetCore.Mvc;

namespace IAMS.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
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
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Create multipart form content
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            // Forward to API
            var response = await _httpClient.PostAsync("/api/policies/import", content);

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
