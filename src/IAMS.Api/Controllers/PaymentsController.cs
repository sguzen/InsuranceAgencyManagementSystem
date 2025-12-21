using IAMS.Shared.DTOs.Payment;
using IAMS.Application.Features.Payments.Commands.CreatePayment;
using IAMS.Application.Features.Payments.Commands.DeletePayment;
using IAMS.Application.Features.Payments.Commands.UpdatePaymentStatus;
using IAMS.Application.Features.Payments.Queries.GetOverduePayments;
using IAMS.Application.Features.Payments.Queries.GetPaymentById;
using IAMS.Application.Features.Payments.Queries.GetPaymentsByPolicyId;
using IAMS.Application.Features.Payments.Queries.GetPaymentsPaged;
using IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId;
using IAMS.Application.Features.Payments.Queries.GetCustomersWithOutstandingBalance;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all payments with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<PolicyPaymentDto>>> GetPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetPaymentsPagedQuery(pageNumber, pageSize, searchTerm);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PolicyPaymentDto>> GetPayment(int id)
        {
            var query = new GetPaymentByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get all payments for a specific policy
        /// </summary>
        [HttpGet("policy/{policyId}")]
        public async Task<ActionResult<Result<List<PolicyPaymentDto>>>> GetPaymentsByPolicy(int policyId)
        {
            var query = new GetPaymentsByPolicyIdQuery(policyId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get total payments amount for a specific policy
        /// </summary>
        [HttpGet("policy/{policyId}/total")]
        public async Task<ActionResult<Result<decimal>>> GetTotalPaymentsByPolicy(int policyId)
        {
            var query = new GetTotalPaymentsByPolicyIdQuery(policyId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all overdue payments
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<List<PolicyPaymentDto>>> GetOverduePayments()
        {
            var query = new GetOverduePaymentsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get customers with outstanding balances
        /// </summary>
        [HttpGet("outstanding-balances")]
        public async Task<ActionResult<Result<List<CustomerOutstandingBalanceDto>>>> GetCustomersWithOutstandingBalance([FromQuery] int? limit = null)
        {
            var query = new GetCustomersWithOutstandingBalanceQuery(limit);
            var result = await _mediator.Send(query);
            return Ok(Result<List<CustomerOutstandingBalanceDto>>.Success(result));
        }

        /// <summary>
        /// Create a new payment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<PolicyPaymentDto>>> CreatePayment([FromBody] CreatePolicyPaymentDto paymentDto)
        {
            var command = new CreatePaymentCommand(paymentDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetPayment), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<Result>> UpdatePaymentStatus(
            int id,
            [FromBody] UpdatePaymentStatusRequest request)
        {
            var command = new UpdatePaymentStatusCommand(id, request.Status);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a payment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> DeletePayment(int id)
        {
            var command = new DeletePaymentCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }

    /// <summary>
    /// Request model for updating payment status
    /// </summary>
    public class UpdatePaymentStatusRequest
    {
        public PaymentStatus Status { get; set; }
    }
}
