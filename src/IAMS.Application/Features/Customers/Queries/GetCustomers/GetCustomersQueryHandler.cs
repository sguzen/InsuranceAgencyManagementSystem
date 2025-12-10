using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
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

                // OPTIMIZED: Use IQueryable + ProjectTo pattern
                // 1. Get count before pagination
                var countQuery = _customerRepository.AsQueryable().Where(c => !c.IsDeleted);

                // Apply search filter for count
                if (!string.IsNullOrWhiteSpace(request.QueryParams.SearchTerm))
                {
                    var search = request.QueryParams.SearchTerm.ToLower();
                    countQuery = countQuery.Where(c =>
                        c.FirstName.ToLower().Contains(search) ||
                        c.LastName.ToLower().Contains(search) ||
                        c.Email.ToLower().Contains(search) ||
                        c.IdentificationNumber.Contains(search) ||
                        c.CustomerCode.Contains(search));
                }

                // Apply status filter for count
                if (request.QueryParams.Status.HasValue)
                {
                    countQuery = countQuery.Where(c => c.Status == request.QueryParams.Status.Value);
                }

                var totalCount = await countQuery.CountAsync(cancellationToken);

                // 2. Get paginated data with ProjectTo for efficient database-level projection
                // All aggregates (ActivePoliciesCount, TotalPremium, etc.) calculated in database
                var query = _customerRepository.GetPagedQuery(request.QueryParams);

                var customerListDtos = await query
                    .ProjectTo<CustomerListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Map lightweight DTO to full DTO
                var customerDtos = _mapper.Map<List<CustomerDto>>(customerListDtos);

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