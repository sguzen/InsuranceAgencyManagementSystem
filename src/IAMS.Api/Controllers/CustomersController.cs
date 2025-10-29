using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Commands.UpdateCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo;
using IAMS.Application.Features.Customers.Queries.GetCustomerByPhone;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer;
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
            CustomerStatus _status;
            var query = new CustomerQueryParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Status = Enum.Parse<CustomerStatus>(status)
            };

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
            var query = new GetCustomerByIdentificationNoQuery(IdentificationNo, 44 );
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}