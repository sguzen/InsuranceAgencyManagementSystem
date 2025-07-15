using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Policies.Commands.ActivatePolicy;
using IAMS.Application.Features.Policies.Commands.CancelPolicy;
using IAMS.Application.Features.Policies.Commands.CreatePolicy;
using IAMS.Application.Features.Policies.Commands.DeletePolicy;
using IAMS.Application.Features.Policies.Commands.ReactivatePolicy;
using IAMS.Application.Features.Policies.Commands.RenewPolicy;
using IAMS.Application.Features.Policies.Commands.SuspendPolicy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetExpiringPolicies;
using IAMS.Application.Features.Policies.Queries.GetPolicies;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer;
using IAMS.Application.Features.Policies.Queries.GetPolicyByNumber;
using IAMS.Application.Features.Policies.Queries.GetPolicyStatistics;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            var query = new PolicyQueryParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CustomerId = customerId,
                InsuranceCompanyId = insuranceCompanyId,
                PolicyTypeId = policyTypeId,
                Status = Enum.Parse<PolicyStatus>(status)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get policy by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<PolicyDto>>> GetPolicy(int id)
        {
            var query = new GetPolicyByNumberQuery(id.ToString()); // huh?
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
            var command = new CancelPolicyCommand
            {
                Id = id,
                CancellationReason = request.Reason
            };
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
            var command = new SuspendPolicyCommand
            {
                Id = id,
                SuspensionReason = request.Reason
            };
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
            var command = new RenewPolicyCommand
            {
                OriginalPolicyId = id,
                NewStartDate = request.StartDate,
                NewEndDate = request.EndDate,
                NewPremiumAmount = request.PremiumAmount
            };
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
    }

    // Request models
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