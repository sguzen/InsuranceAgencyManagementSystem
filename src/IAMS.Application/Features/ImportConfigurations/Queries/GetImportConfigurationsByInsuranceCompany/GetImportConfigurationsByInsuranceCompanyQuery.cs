using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurationsByInsuranceCompany
{
    public class GetImportConfigurationsByInsuranceCompanyQuery : IRequest<Result<List<ImportConfigurationDto>>>
    {
        public int InsuranceCompanyId { get; set; }

        public GetImportConfigurationsByInsuranceCompanyQuery(int insuranceCompanyId)
        {
            InsuranceCompanyId = insuranceCompanyId;
        }
    }
}
