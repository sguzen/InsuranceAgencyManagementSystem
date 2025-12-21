using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanyByName;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.InsuranceCompany;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class InsuranceCompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InsuranceCompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all insurance companies with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<InsuranceCompanyDto>>>> GetInsuranceCompanies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var queryParams = new InsuranceCompanyQueryParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                IsActive = isActive
            };

            var query = new GetInsuranceCompaniesQuery(queryParams);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get insurance company by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<InsuranceCompanyDto>>> GetInsuranceCompany(int id)
        {
            var query = new GetInsuranceCompanyQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get insurance company by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<Result<InsuranceCompanyDto>>> GetInsuranceCompanyByName(string name)
        {
            var query = new GetInsuranceCompanyByNameQuery(name);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a new insurance company
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<InsuranceCompanyDto>>> CreateInsuranceCompany(
            [FromBody] CreateInsuranceCompanyDto companyDto)
        {
            var command = new CreateInsuranceCompanyCommand(companyDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetInsuranceCompany), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Update an existing insurance company
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<InsuranceCompanyDto>>> UpdateInsuranceCompany(
            int id,
            [FromBody] UpdateInsuranceCompanyDto companyDto)
        {
            var command = new UpdateInsuranceCompanyCommand(id, companyDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete an insurance company
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> DeleteInsuranceCompany(int id)
        {
            var command = new DeleteInsuranceCompanyCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
