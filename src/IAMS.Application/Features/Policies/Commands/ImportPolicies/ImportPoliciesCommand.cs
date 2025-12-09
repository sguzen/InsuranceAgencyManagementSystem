using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Commands.ImportPolicies
{
    public class ImportPoliciesCommand : IRequest<Result<PolicyImportResultDto>>
    {
        public IFormFile File { get; set; }
        public string UserId { get; set; }
        public int InsuranceCompanyId { get; set; }

        public ImportPoliciesCommand(IFormFile file, string userId, int insuranceCompanyId)
        {
            File = file;
            UserId = userId;
            InsuranceCompanyId = insuranceCompanyId;
        }
    }
}
