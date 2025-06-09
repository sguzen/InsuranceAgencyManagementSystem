using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PagedResult<CustomerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomersQueryHandler> _logger;

        public GetCustomersQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCustomersQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedResult = await _unitOfWork.Customers.GetCustomersPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

                var customerDtos = new PagedResult<CustomerDto>
                {
                    Items = _mapper.Map<List<CustomerDto>>(pagedResult.Items),
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                return Result<PagedResult<CustomerDto>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return Result<PagedResult<CustomerDto>>.Failure("An error occurred while retrieving customers");
            }
        }
    }
}