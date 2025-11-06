using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany
{
    public record UpdateInsuranceCompanyCommand(int Id, UpdateInsuranceCompanyDto CompanyDto)
        : IRequest<Result<InsuranceCompanyDto>>;
}
