using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromMySql
{
    public class SyncPoliciesFromMySqlCommand : IRequest<Result<PolicyImportResultDto>>
    {
        /// <summary>
        /// Start date for the import range
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the import range
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Insurance company ID — used to look up the ImportConfiguration
        /// which contains MySQL connection details and agency code
        /// </summary>
        public int InsuranceCompanyId { get; set; }

        /// <summary>
        /// User performing the import
        /// </summary>
        public string UserId { get; set; }

        public SyncPoliciesFromMySqlCommand(
            DateTime startDate,
            DateTime endDate,
            int insuranceCompanyId,
            string userId)
        {
            StartDate = startDate;
            EndDate = endDate;
            InsuranceCompanyId = insuranceCompanyId;
            UserId = userId;
        }
    }
}
