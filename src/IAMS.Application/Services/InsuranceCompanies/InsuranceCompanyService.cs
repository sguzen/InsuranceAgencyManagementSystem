using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanyByName;
using IAMS.Shared.DTOs.InsuranceCompany;

namespace IAMS.Application.Services.InsuranceCompanies
{
    public class InsuranceCompanyService : IInsuranceCompanyService
    {
        private readonly IMediator _mediator;

        public InsuranceCompanyService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByIdAsync(int id)
        {
            return await _mediator.Send(new GetInsuranceCompanyQuery(id));
        }

        public async Task<Result<PagedResult<InsuranceCompanyDto>>> GetInsuranceCompaniesAsync(InsuranceCompanyQueryParams queryParams)
        {
            return await _mediator.Send(new GetInsuranceCompaniesQuery(queryParams));
        }

        public async Task<Result<InsuranceCompanyDto>> CreateInsuranceCompanyAsync(CreateInsuranceCompanyDto createCompanyDto)
        {
            return await _mediator.Send(new CreateInsuranceCompanyCommand(createCompanyDto));
        }

        public async Task<Result<InsuranceCompanyDto>> UpdateInsuranceCompanyAsync(int id, UpdateInsuranceCompanyDto updateCompanyDto)
        {
            return await _mediator.Send(new UpdateInsuranceCompanyCommand(id, updateCompanyDto));
        }

        public async Task<Result> DeleteInsuranceCompanyAsync(int id)
        {
            return await _mediator.Send(new DeleteInsuranceCompanyCommand(id));
        }

        public async Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByNameAsync(string name)
        {
            return await _mediator.Send(new GetInsuranceCompanyByNameQuery(name));
        }

        public async Task<Result<List<InsuranceCompanyDto>>> GetActiveInsuranceCompaniesAsync()
        {
            var queryParams = new InsuranceCompanyQueryParams { IsActive = true, PageSize = 1000 };
            var result = await GetInsuranceCompaniesAsync(queryParams);

            return result.IsSuccess
                ? Result<List<InsuranceCompanyDto>>.Success(result.Data?.Items ?? new List<InsuranceCompanyDto>())
                : Result<List<InsuranceCompanyDto>>.Failure(result.Message ?? "Failed to retrieve companies", result.Errors);
        }
    }
}
