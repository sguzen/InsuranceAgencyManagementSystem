using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies
{
    public record GetInsuranceCompaniesQuery(InsuranceCompanyQueryParams QueryParams)
        : IRequest<Result<PagedResult<InsuranceCompanyDto>>>;
}
