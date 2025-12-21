using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany
{
    public record UpdateInsuranceCompanyCommand(int Id, UpdateInsuranceCompanyDto CompanyDto)
        : IRequest<Result<InsuranceCompanyDto>>;
}
