using MediatR;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;
using IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompany;

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
            // This would need a GetInsuranceCompaniesQuery implementation
            return Result<PagedResult<InsuranceCompanyDto>>.Success(PagedResult<InsuranceCompanyDto>.Empty());
        }

        public async Task<Result<InsuranceCompanyDto>> CreateInsuranceCompanyAsync(CreateInsuranceCompanyDto createCompanyDto)
        {
            return await _mediator.Send(new CreateInsuranceCompanyCommand(createCompanyDto));
        }

        public async Task<Result<InsuranceCompanyDto>> UpdateInsuranceCompanyAsync(int id, UpdateInsuranceCompanyDto updateCompanyDto)
        {
            // This would need an UpdateInsuranceCompanyCommand implementation
            return Result<InsuranceCompanyDto>.NotFound("Update not implemented yet");
        }

        public async Task<Result> DeleteInsuranceCompanyAsync(int id)
        {
            // This would need a DeleteInsuranceCompanyCommand implementation
            return Result.Success("Delete not implemented yet");
        }

        public async Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByNameAsync(string name)
        {
            // This would need a specific query implementation
            return Result<InsuranceCompanyDto>.NotFound("Company not found");
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
