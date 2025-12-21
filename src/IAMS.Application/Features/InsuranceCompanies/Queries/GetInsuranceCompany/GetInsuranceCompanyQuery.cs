using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompany
{
    public class GetInsuranceCompanyQuery : IRequest<Result<InsuranceCompanyDto>>
    {
        public int Id { get; set; }

        public GetInsuranceCompanyQuery(int id)
        {
            Id = id;
        }
    }
}