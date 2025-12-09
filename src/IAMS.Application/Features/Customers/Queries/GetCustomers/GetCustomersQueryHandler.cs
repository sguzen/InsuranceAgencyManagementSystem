using MediatR;
using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PagedResult<CustomerDto>>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomersQueryHandler> _logger;

        public GetCustomersQueryHandler(
            ICustomerRepository customerRepository,
            IMapper mapper,
            ILogger<GetCustomersQueryHandler> logger)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting customers with parameters: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}",
                    request.QueryParams.SearchTerm, request.QueryParams.PageNumber, request.QueryParams.PageSize);

                var (customers, totalCount) = await _customerRepository.GetPagedAsync(request.QueryParams);

                var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

                // Add aggregated data for each customer
                foreach (var customerDto in customerDtos)
                {
                    var customer = customers.First(c => c.Id == customerDto.Id);
                    customerDto.ActivePoliciesCount = customer.GetActivePolicies().Count;
                    customerDto.TotalPremium = customer.GetTotalPremiums();
                    customerDto.TotalCommissions = customer.GetTotalCommissions();
                    customerDto.LastPolicyDate = customer.GetLastPolicyDate();
                }

                var pagedResult = PagedResult<CustomerDto>.Create(
                    customerDtos,
                    totalCount,
                    request.QueryParams.PageNumber,
                    request.QueryParams.PageSize);

                return Result<PagedResult<CustomerDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return Result<PagedResult<CustomerDto>>.InternalError("An error occurred while retrieving customers");
            }
        }
    }
}