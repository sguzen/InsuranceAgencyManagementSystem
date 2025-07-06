using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Features.Customers.Queries.GetCustomerStatistics;
using IAMS.Application.Features.Customers.Queries.GetRecentCustomers;
using IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Application.Services.Customers;
using IAMS.Domain.Enums;
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
        //return Result<PagedResult<CustomerDto>>.Success(PagedResult<CustomerDto>.Empty());
        try
        {

            // Use MediatR to get customers
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

    public async Task<Result<int>> GetTotalCustomersCountAsync(int tenantId)
    {
        try
        {
            var query = new GetTotalCustomersCountQuery(tenantId);
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            return Result<int>.InternalError($"Müşteri sayısı alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<List<CustomerDto>>> GetRecentCustomersAsync(int count = 5)
    {
        try
        {
            var query = new GetRecentCustomersQuery(count);
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            return Result<List<CustomerDto>>.InternalError($"Son müşteriler alınırken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync()
    {
        try
        {
            var query = new GetCustomerStatisticsQuery();
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            return Result<CustomerStatisticsDto>.InternalError($"Müşteri istatistikleri alınırken hata oluştu: {ex.Message}");
        }
    }

    public Task<Result<List<CustomerDto>>> GetTopCustomersByPolicyCountAsync(int count = 10)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Dictionary<CustomerStatus, int>>> GetCustomersByStatusAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<Dictionary<string, int>>> GetCustomersCreatedByMonthAsync(int months = 12)
    {
        throw new NotImplementedException();
    }
}
