using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies
{
    public record GetInsuranceCompaniesQuery(InsuranceCompanyQueryParams QueryParams)
        : IRequest<Result<PagedResult<InsuranceCompanyDto>>>;
}
