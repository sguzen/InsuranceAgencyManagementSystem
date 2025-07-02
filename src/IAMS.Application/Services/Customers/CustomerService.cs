using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Models;
using IAMS.Application.Services.Customers;
using MediatR;

    public class CustomerService : ICustomerService
    {
        private readonly IMediator _mediator;

        public CustomerService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            return await _mediator.Send(new GetCustomerQuery(id));
        }

        public async Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerQueryParams queryParams)
        {
            return Result<PagedResult<CustomerDto>>.Success(PagedResult<CustomerDto>.Empty());
        }

        public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateOrUpdateCustomerDto createCustomerDto)
        {
            return await _mediator.Send(new CreateCustomerCommand(createCustomerDto));
        }

        public async Task<Result<CustomerDto>> UpdateCustomerAsync(int id, CreateOrUpdateCustomerDto updateCustomerDto)
        {
            return Result<CustomerDto>.NotFound("Update not implemented yet");
        }

        public async Task<Result> DeleteCustomerAsync(int id)
        {
            return await _mediator.Send(new DeleteCustomerCommand(id));
        }

        public async Task<Result<CustomerDto>> GetCustomerByTcNoAsync(string tcNo)
        {
            return Result<CustomerDto>.NotFound("Customer not found");
        }

        public async Task<Result<CustomerDto>> GetCustomerByEmailAsync(string email)
        {
            return Result<CustomerDto>.NotFound("Customer not found");
        }

        public async Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode)
        {
            return Result<CustomerDto>.NotFound("Customer not found");
        }

        public async Task<Result<List<CustomerDto>>> GetCustomersWithActivePoliciesAsync()
        {
            return Result<List<CustomerDto>>.Success(new List<CustomerDto>());
        }

        public async Task<Result<bool>> ValidateTcNoAsync(string tcNo, int? excludeCustomerId = null)
        {
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ValidateEmailAsync(string email, int? excludeCustomerId = null)
        {
            return Result<bool>.Success(true);
        }
    }
