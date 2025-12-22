using IAMS.Shared.DTOs.Policy;
using IAMS.Application.Features.Policies.Commands.ActivatePolicy;
using IAMS.Application.Features.Policies.Commands.CancelPolicy;
using IAMS.Application.Features.Policies.Commands.CreatePolicy;
using IAMS.Application.Features.Policies.Commands.DeletePolicy;
using IAMS.Application.Features.Policies.Commands.ImportPolicies;
using IAMS.Application.Features.Policies.Commands.ImportPoliciesWithMapping;
using IAMS.Application.Features.Policies.Commands.ReactivatePolicy;
using IAMS.Application.Features.Policies.Queries.ParsePolicyImport;
using IAMS.Application.Features.Policies.Commands.RenewPolicy;
using IAMS.Application.Features.Policies.Commands.SuspendPolicy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetEndorsementsByPolicyId;
using IAMS.Application.Features.Policies.Queries.GetExpiringPolicies;
using IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenueByCurrency;
using IAMS.Application.Features.Policies.Queries.GetPolicies;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer;
using IAMS.Application.Features.Policies.Queries.GetPolicy;
using IAMS.Application.Features.Policies.Queries.GetPolicyByNumber;
using IAMS.Application.Features.Policies.Queries.GetPolicyStatistics;
using IAMS.Application.Features.Policies.Queries.GetTotalPoliciesCount;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class PoliciesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PoliciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all policies with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<PolicyDto>>>> GetPolicies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? insuranceCompanyId = null,
            [FromQuery] int? policyTypeId = null,
            [FromQuery] string? status = null)
        {
            var queryParams = new PolicyQueryParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CustomerId = customerId,
                InsuranceCompanyId = insuranceCompanyId,
                PolicyTypeId = policyTypeId,
                Status = string.IsNullOrEmpty(status) ? PolicyStatus.Active : Enum.Parse<PolicyStatus>(status)
            };

            var query = new GetPoliciesQuery(queryParams);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get policy by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<PolicyDto>>> GetPolicy(int id)
        {
            var query = new GetPolicyQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get policy by policy number
        /// </summary>
        [HttpGet("by-number/{policyNumber}")]
        public async Task<ActionResult<Result<PolicyDto>>> GetPolicyByNumber(string policyNumber)
        {
            var query = new GetPolicyByNumberQuery(policyNumber);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a new policy
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<PolicyDto>>> CreatePolicy([FromBody] CreatePolicyDto policyDto)
        {
            var command = new CreatePolicyCommand(policyDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetPolicy), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Update an existing policy
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<PolicyDto>>> UpdatePolicy(int id, [FromBody] UpdatePolicyDto policyDto)
        {
            var command = new UpdatePolicyCommand(id,policyDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a policy
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> DeletePolicy(int id)
        {
            var command = new DeletePolicyCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Activate a policy
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<Result<PolicyDto>>> ActivatePolicy(int id)
        {
            var command = new ActivatePolicyCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Cancel a policy
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<Result<PolicyDto>>> CancelPolicy(int id, [FromBody] CancelPolicyRequest request)
        {
            var command = new CancelPolicyCommand(id,request.Reason);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Suspend a policy
        /// </summary>
        [HttpPost("{id}/suspend")]
        public async Task<ActionResult<Result<PolicyDto>>> SuspendPolicy(int id, [FromBody] SuspendPolicyRequest request)
        {
            var command = new SuspendPolicyCommand(id,request.Reason);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Reactivate a suspended policy
        /// </summary>
        [HttpPost("{id}/reactivate")]
        public async Task<ActionResult<Result<PolicyDto>>> ReactivatePolicy(int id)
        {
            var command = new ReactivatePolicyCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Renew a policy
        /// </summary>
        [HttpPost("{id}/renew")]
        public async Task<ActionResult<Result<PolicyDto>>> RenewPolicy(int id, [FromBody] RenewPolicyRequest request)
        {
            var command = new RenewPolicyCommand(id,request.StartDate,request.EndDate,request.PremiumAmount);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get expiring policies
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<Result<List<PolicyDto>>>> GetExpiringPolicies([FromQuery] int daysAhead = 30)
        {
            var query = new GetExpiringPoliciesQuery (daysAhead);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get policies by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<Result<List<PolicyDto>>>> GetPoliciesByCustomer(int customerId)
        {
            var query = new GetPoliciesByCustomerQuery(customerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get policy statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<Result<PolicyStatisticsDto>>> GetStatistics()
        {
            var query = new GetPolicyStatisticsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get total policies count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<Result<int>>> GetTotalPoliciesCount()
        {
            var query = new GetTotalPoliciesCountQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get expiring policies count
        /// </summary>
        [HttpGet("expiring/count")]
        public async Task<ActionResult<Result<int>>> GetExpiringPoliciesCount([FromQuery] int daysAhead = 30)
        {
            var query = new GetExpiringPoliciesCountQuery(daysAhead);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get endorsements by policy ID
        /// </summary>
        [HttpGet("{policyId}/endorsements")]
        public async Task<ActionResult<Result<List<PolicyDto>>>> GetEndorsementsByPolicyId(int policyId)
        {
            var query = new GetEndorsementsByPolicyIdQuery(policyId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get monthly revenue
        /// </summary>
        [HttpGet("revenue/monthly")]
        public async Task<ActionResult<Result<decimal>>> GetMonthlyRevenue()
        {
            var query = new GetMonthlyRevenueQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get monthly revenue by currency
        /// </summary>
        [HttpGet("revenue/monthly-by-currency")]
        public async Task<ActionResult<Result<Dictionary<string, decimal>>>> GetMonthlyRevenueByCurrency()
        {
            var query = new GetMonthlyRevenueByCurrencyQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Parse and preview policies from Excel file WITHOUT saving
        /// Returns preview data for customer mapping before import
        /// </summary>
        [HttpPost("import/parse")]
        public async Task<ActionResult<Result<List<PolicyImportPreviewDto>>>> ParsePolicyImport(IFormFile file, [FromForm] int insuranceCompanyId)
        {
            if (insuranceCompanyId <= 0)
            {
                return BadRequest(Result<List<PolicyImportPreviewDto>>.Failure("Insurance company must be selected", (List<string>?)null));
            }

            var query = new ParsePolicyImportQuery(file, insuranceCompanyId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Import policies with customer mappings
        /// Used after ParsePolicyImport to save mapped policies
        /// </summary>
        [HttpPost("import/with-mapping")]
        public async Task<ActionResult<Result<PolicyImportResultDto>>> ImportPoliciesWithMapping(
            [FromBody] ImportPoliciesWithMappingRequest request)
        {
            if (request.InsuranceCompanyId <= 0)
            {
                return BadRequest(Result<PolicyImportResultDto>.Failure("Insurance company must be selected", (List<string>?)null));
            }

            if (request.MappedPolicies == null || !request.MappedPolicies.Any())
            {
                return BadRequest(Result<PolicyImportResultDto>.Failure("No policies to import", (List<string>?)null));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var command = new ImportPoliciesWithMappingCommand(
                request.MappedPolicies,
                userId,
                request.InsuranceCompanyId);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Import policies from Excel file (legacy - direct import)
        /// </summary>
        [HttpPost("import")]
        public async Task<ActionResult<Result<PolicyImportResultDto>>> ImportPolicies(IFormFile file, [FromForm] int insuranceCompanyId)
        {
            if (insuranceCompanyId <= 0)
            {
                return BadRequest(Result<PolicyImportResultDto>.Failure("Insurance company must be selected", (List<string>?)null));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var command = new ImportPoliciesCommand(file, userId, insuranceCompanyId);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }

    // Request models
    public class ImportPoliciesWithMappingRequest
    {
        public List<PolicyImportPreviewDto> MappedPolicies { get; set; } = new();
        public int InsuranceCompanyId { get; set; }
    }

    public class CancelPolicyRequest
    {
        public string? Reason { get; set; }
    }

    public class SuspendPolicyRequest
    {
        public string? Reason { get; set; }
    }

    public class RenewPolicyRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
    }
}