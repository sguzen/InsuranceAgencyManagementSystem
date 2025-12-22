using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IAMS.Application.Features.Policies.Queries.ParsePolicyImport
{
    /// <summary>
    /// Query to parse policy import file and return preview data WITHOUT saving to database
    /// This allows review and customer mapping before actual import
    /// </summary>
    public class ParsePolicyImportQuery : IRequest<Result<List<PolicyImportPreviewDto>>>
    {
        public IFormFile File { get; set; }
        public int InsuranceCompanyId { get; set; }

        public ParsePolicyImportQuery(IFormFile file, int insuranceCompanyId)
        {
            File = file;
            InsuranceCompanyId = insuranceCompanyId;
        }
    }
}
