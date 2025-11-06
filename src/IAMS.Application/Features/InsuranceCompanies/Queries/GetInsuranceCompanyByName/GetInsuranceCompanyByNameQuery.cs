using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanyByName
{
    public record GetInsuranceCompanyByNameQuery(string Name) : IRequest<Result<InsuranceCompanyDto>>;
}
