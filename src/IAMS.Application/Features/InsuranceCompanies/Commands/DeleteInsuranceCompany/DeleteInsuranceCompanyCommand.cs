using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany
{
    public record DeleteInsuranceCompanyCommand(int Id) : IRequest<Result>;
}
