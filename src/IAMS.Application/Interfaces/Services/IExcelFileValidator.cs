using IAMS.Application.Models;
using Microsoft.AspNetCore.Http;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Validates Excel files for policy import
    /// </summary>
    public interface IExcelFileValidator
    {
        /// <summary>
        /// Validates an uploaded Excel file
        /// </summary>
        Result ValidateFile(IFormFile file);
    }
}
