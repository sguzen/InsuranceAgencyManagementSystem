using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanyByName
{
    public record GetInsuranceCompanyByNameQuery(string Name) : IRequest<Result<InsuranceCompanyDto>>;
}
