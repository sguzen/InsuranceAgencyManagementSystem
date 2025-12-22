using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.ImportPoliciesWithMapping
{
    /// <summary>
    /// Command to import policies with customer mappings
    /// Takes preview data with customer mappings and saves policies to database
    /// </summary>
    public class ImportPoliciesWithMappingCommand : IRequest<Result<PolicyImportResultDto>>
    {
        public List<PolicyImportPreviewDto> MappedPolicies { get; set; }
        public string UserId { get; set; }
        public int InsuranceCompanyId { get; set; }

        public ImportPoliciesWithMappingCommand(
            List<PolicyImportPreviewDto> mappedPolicies,
            string userId,
            int insuranceCompanyId)
        {
            MappedPolicies = mappedPolicies;
            UserId = userId;
            InsuranceCompanyId = insuranceCompanyId;
        }
    }
}
