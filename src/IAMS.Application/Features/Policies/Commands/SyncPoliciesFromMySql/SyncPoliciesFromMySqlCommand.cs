using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromMySql
{
    public class SyncPoliciesFromMySqlCommand : IRequest<Result<PolicyImportResultDto>>
    {
        /// <summary>
        /// Agency code to filter policies (e.g., "A022")
        /// </summary>
        public string AgencyCode { get; set; }

        /// <summary>
        /// Start date for the import range
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the import range
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Insurance company ID in the local system to associate imported policies with
        /// </summary>
        public int InsuranceCompanyId { get; set; }

        /// <summary>
        /// User performing the import
        /// </summary>
        public string UserId { get; set; }

        public SyncPoliciesFromMySqlCommand(
            string agencyCode,
            DateTime startDate,
            DateTime endDate,
            int insuranceCompanyId,
            string userId)
        {
            AgencyCode = agencyCode;
            StartDate = startDate;
            EndDate = endDate;
            InsuranceCompanyId = insuranceCompanyId;
            UserId = userId;
        }
    }
}
