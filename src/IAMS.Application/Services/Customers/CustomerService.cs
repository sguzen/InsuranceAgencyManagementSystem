using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Commands.UpdateCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomerByCode;
using IAMS.Application.Features.Customers.Queries.GetCustomerByEmail;
using IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Features.Customers.Queries.GetCustomersByStatus;
using IAMS.Application.Features.Customers.Queries.GetCustomersCreatedByMonth;
using IAMS.Application.Features.Customers.Queries.GetCustomerStatistics;
using IAMS.Application.Features.Customers.Queries.GetCustomersWithActivePolicies;
using IAMS.Application.Features.Customers.Queries.GetRecentCustomers;
using IAMS.Application.Features.Customers.Queries.GetTopCustomersByPolicyCount;
using IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount;
using IAMS.Application.Features.Customers.Queries.ValidateEmail;
using IAMS.Application.Features.Customers.Queries.ValidateIdentificationNo;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Services.Customers
{
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
            try
            {
                var query = new GetCustomersQuery(queryParams);
                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                {
                    return Result<PagedResult<CustomerDto>>.Success(result.Data!, "Müşteriler başarıyla getirildi");
                }

                return Result<PagedResult<CustomerDto>>.Failure(result.Message ?? "Müşteriler getirilemedi", result.Errors);
            }
            catch (Exception ex)
            {
                return Result<PagedResult<CustomerDto>>.InternalError($"Müşteriler getirilirken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateOrUpdateCustomerDto createCustomerDto)
        {
            return await _mediator.Send(new CreateCustomerCommand(createCustomerDto));
        }

        public async Task<Result<CustomerDto>> UpdateCustomerAsync(int id, CreateOrUpdateCustomerDto updateCustomerDto)
        {
            return await _mediator.Send(new UpdateCustomerCommand(id, updateCustomerDto));
        }

        public async Task<Result> DeleteCustomerAsync(int id)
        {
            return await _mediator.Send(new DeleteCustomerCommand(id));
        }

        public async Task<Result<CustomerDto>> GetCustomerByIdentificationNoAsync(string IdentificationNo)
        {
            return await _mediator.Send(new GetCustomerByIdentificationNoQuery(IdentificationNo));
        }

        public async Task<Result<CustomerDto>> GetCustomerByEmailAsync(string email)
        {
            return await _mediator.Send(new GetCustomerByEmailQuery(email));
        }

        public async Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode)
        {
            return await _mediator.Send(new GetCustomerByCodeQuery(customerCode));
        }

        public async Task<Result<List<CustomerDto>>> GetCustomersWithActivePoliciesAsync()
        {
            return await _mediator.Send(new GetCustomersWithActivePoliciesQuery());
        }

        public async Task<Result<bool>> ValidateIdentificationNoAsync(string tcNo, int? excludeCustomerId = null)
        {
            return await _mediator.Send(new ValidateIdentificationNoQuery(tcNo, excludeCustomerId));
        }

        public async Task<Result<bool>> ValidateEmailAsync(string email, int? excludeCustomerId = null)
        {
            return await _mediator.Send(new ValidateEmailQuery(email, excludeCustomerId));
        }

        // Dashboard statistics methods
        public async Task<Result<int>> GetTotalCustomersCountAsync()
        {
            return await _mediator.Send(new GetTotalCustomersCountQuery());
        }

        public async Task<Result<List<CustomerDto>>> GetRecentCustomersAsync(int count = 5)
        {
            return await _mediator.Send(new GetRecentCustomersQuery(count));
        }

        public async Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync()
        {
            return await _mediator.Send(new GetCustomerStatisticsQuery());
        }

        public async Task<Result<List<CustomerDto>>> GetTopCustomersByPolicyCountAsync(int count = 10)
        {
            return await _mediator.Send(new GetTopCustomersByPolicyCountQuery(count));
        }

        public async Task<Result<Dictionary<CustomerStatus, int>>> GetCustomersByStatusAsync()
        {
            return await _mediator.Send(new GetCustomersByStatusQuery());
        }

        public async Task<Result<Dictionary<string, int>>> GetCustomersCreatedByMonthAsync(int months = 12)
        {
            return await _mediator.Send(new GetCustomersCreatedByMonthQuery(months));
        }
    }
}