using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies
{
    public record GetInsuranceCompaniesQuery(InsuranceCompanyQueryParams QueryParams)
        : IRequest<Result<PagedResult<InsuranceCompanyDto>>>;
}
