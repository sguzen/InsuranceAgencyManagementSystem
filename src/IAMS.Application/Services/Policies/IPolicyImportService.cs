using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for importing policies and endorsements from Excel files
    /// </summary>
    public interface IPolicyImportService
    {
        /// <summary>
        /// Imports policies and endorsements from an Excel file
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <param name="userId">User performing the import</param>
        /// <returns>Import result with success/failure counts and errors</returns>
        Task<PolicyImportResultDto> ImportFromExcelAsync(string filePath, string userId);

        /// <summary>
        /// Imports policies and endorsements from a stream (for file uploads)
        /// </summary>
        /// <param name="stream">Stream containing Excel data</param>
        /// <param name="userId">User performing the import</param>
        /// <returns>Import result with success/failure counts and errors</returns>
        Task<PolicyImportResultDto> ImportFromStreamAsync(Stream stream, string userId);

        /// <summary>
        /// Validates Excel data without importing
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <returns>Validation result</returns>
        Task<PolicyImportResultDto> ValidateExcelAsync(string filePath);

        /// <summary>
        /// Parses Excel file and returns list of DTOs for preview
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <returns>List of parsed policies</returns>
        Task<List<ImportPolicyDto>> ParseExcelAsync(string filePath);
    }
}
