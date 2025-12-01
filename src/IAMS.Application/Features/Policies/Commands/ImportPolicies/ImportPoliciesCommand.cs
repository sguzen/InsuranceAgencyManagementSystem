using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IAMS.Application.Features.Policies.Commands.ImportPolicies
{
    public class ImportPoliciesCommand : IRequest<Result<PolicyImportResultDto>>
    {
        public IFormFile File { get; set; }
        public string UserId { get; set; }

        public ImportPoliciesCommand(IFormFile file, string userId)
        {
            File = file;
            UserId = userId;
        }
    }
}
