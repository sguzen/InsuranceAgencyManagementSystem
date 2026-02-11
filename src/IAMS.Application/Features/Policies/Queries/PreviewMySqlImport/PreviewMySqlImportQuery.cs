using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.PreviewMySqlImport
{
    /// <summary>
    /// Query to fetch policies from external MySQL database and return preview data
    /// WITHOUT saving to database. This allows Cari Kart mapping before actual import.
    /// </summary>
    public class PreviewMySqlImportQuery : IRequest<Result<MySqlImportPreviewResult>>
    {
        public int AgencyId { get; set; }
        public int MasterInsuranceCompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public PreviewMySqlImportQuery(int agencyId, int masterInsuranceCompanyId, DateTime? startDate, DateTime? endDate)
        {
            AgencyId = agencyId;
            MasterInsuranceCompanyId = masterInsuranceCompanyId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
