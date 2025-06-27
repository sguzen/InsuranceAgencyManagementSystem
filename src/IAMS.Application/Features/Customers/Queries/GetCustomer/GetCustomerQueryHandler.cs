using MediatR;
using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomer
{
    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerQueryHandler> _logger;

        public GetCustomerQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetCustomerQueryHandler> logger)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting customer with ID: {CustomerId}", request.Id);

                var customer = await _customerRepository.GetByIdWithDetailsAsync(request.Id);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound("Customer not found");
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);

                return Result<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer with ID: {CustomerId}", request.Id);
                return Result<CustomerDto>.InternalError("An error occurred while retrieving the customer");
            }
        }
    }
}