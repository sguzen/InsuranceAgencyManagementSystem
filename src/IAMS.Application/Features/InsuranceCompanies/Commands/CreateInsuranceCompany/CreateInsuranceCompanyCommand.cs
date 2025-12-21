using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany
{
    public class CreateInsuranceCompanyCommand : IRequest<Result<InsuranceCompanyDto>>
    {
        public CreateInsuranceCompanyDto CompanyDto { get; set; }

        public CreateInsuranceCompanyCommand(CreateInsuranceCompanyDto companyDto)
        {
            CompanyDto = companyDto;
        }
    }
}