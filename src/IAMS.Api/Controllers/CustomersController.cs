using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Payment;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.CustomerPayments.Commands.CreateCustomerPayment;
using IAMS.Application.Features.CustomerPayments.Queries.GetCustomerBalance;
using IAMS.Application.Features.CustomerPayments.Queries.GetCustomerPayments;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Commands.UpdateCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo;
using IAMS.Application.Features.Customers.Queries.GetCustomerByPhone;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Features.Customers.Queries.GetCustomerStatement;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
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
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all customers with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<CustomerDto>>>> GetCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            var queryParams = new CustomerQueryParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Status = string.IsNullOrEmpty(status) ? CustomerStatus.Active : Enum.Parse<CustomerStatus>(status)
            };

            var query = new GetCustomersQuery(queryParams);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<CustomerDto>>> GetCustomer(int id)
        {
            var query = new GetCustomerQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<CustomerDto>>> CreateCustomer([FromBody] CreateOrUpdateCustomerDto customerDto)
        {
            var command = new CreateCustomerCommand(customerDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetCustomer), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<CustomerDto>>> UpdateCustomer(int id, [FromBody] CreateOrUpdateCustomerDto customerDto)
        {
            var command = new UpdateCustomerCommand(id, customerDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> DeleteCustomer(int id)
        {
            var command = new DeleteCustomerCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get customer policies
        /// </summary>
        [HttpGet("{id}/policies")]
        public async Task<ActionResult<Result<List<PolicyDto>>>> GetCustomerPolicies(int id)
        {
            var query = new GetPoliciesByCustomerQuery(id );
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get customer statement (multi-currency account statement with all transactions)
        /// </summary>
        [HttpGet("{id}/statement")]
        public async Task<ActionResult<Result<CustomerMultiCurrencyStatementDto>>> GetCustomerStatement(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? currencyCode = null)
        {
            // Default to current month if no dates provided
            var start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var end = endDate ?? DateTime.Today;

            var query = new GetCustomerStatementQuery(id, start, end, currencyCode);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get customer balance (multi-currency)
        /// </summary>
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<Result<CustomerBalanceDto>>> GetCustomerBalance(int id)
        {
            var query = new GetCustomerBalanceQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all payments for a customer
        /// </summary>
        [HttpGet("{id}/payments")]
        public async Task<ActionResult<Result<List<CustomerPaymentDto>>>> GetCustomerPayments(int id)
        {
            var query = new GetCustomerPaymentsQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Create a customer payment (not tied to specific policy)
        /// </summary>
        [HttpPost("{id}/payments")]
        public async Task<ActionResult<Result<CustomerPaymentDto>>> CreateCustomerPayment(
            int id,
            [FromBody] CreateCustomerPaymentDto paymentDto)
        {
            // Ensure the customer ID matches the route
            paymentDto.CustomerId = id;

            var command = new CreateCustomerPaymentCommand(paymentDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Search customers by phone
        /// </summary>
        [HttpGet("search/phone/{phone}")]
        public async Task<ActionResult<Result<CustomerDto>>> GetCustomerByPhone(string phone)
        {
            var query = new GetCustomerByPhoneQuery { Phone = phone };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Search customers by KKTC number
        /// </summary>
        [HttpGet("search/kktc/{IdentificationNo}")]
        public async Task<ActionResult<Result<CustomerDto>>> GetCustomerByIdentificationNo(string IdentificationNo)
        {
            var query = new GetCustomerByIdentificationNoQuery(IdentificationNo );
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}