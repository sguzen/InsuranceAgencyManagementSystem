using MediatR;
using IAMS.Application.DTOs.InsuranceCompany;
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