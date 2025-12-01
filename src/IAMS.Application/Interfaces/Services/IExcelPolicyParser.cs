using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Parses Excel files to extract policy data
    /// </summary>
    public interface IExcelPolicyParser
    {
        /// <summary>
        /// Parses an Excel file from a stream
        /// </summary>
        Task<List<ImportPolicyDto>> ParseFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parses an Excel file from a file path
        /// </summary>
        Task<List<ImportPolicyDto>> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
