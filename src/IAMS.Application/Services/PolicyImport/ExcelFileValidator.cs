using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using Microsoft.AspNetCore.Http;

namespace IAMS.Application.Services.PolicyImport
{
    /// <summary>
    /// Validates Excel files for policy import
    /// </summary>
    public class ExcelFileValidator : IExcelFileValidator
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

        public Result ValidateFile(IFormFile file)
        {
            // Check if file exists
            if (file == null || file.Length == 0)
            {
                return Result.Failure("No file was uploaded", (List<string>?)null);
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return Result.Failure($"Invalid file format. Only Excel files ({string.Join(", ", AllowedExtensions)}) are supported", (List<string>?)null);
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                return Result.Failure($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB", (List<string>?)null);
            }

            return Result.Success();
        }
    }
}
