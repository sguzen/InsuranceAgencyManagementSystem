using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces;

namespace IAMS.Application.Features.Customers.Queries.GetRecentCustomers
{
    public class GetRecentCustomersQueryHandler : IRequestHandler<GetRecentCustomersQuery, Result<List<CustomerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetRecentCustomersQueryHandler> _logger;

        public GetRecentCustomersQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetRecentCustomersQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<List<CustomerDto>>> Handle(GetRecentCustomersQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var customers = await _unitOfWork.Customers.GetRecentCustomersAsync(request.Count);
                var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

                _logger.LogDebug("Retrieved {Count} recent customers", customerDtos.Count);

                return Result<List<CustomerDto>>.Success(customerDtos, $"Son {customerDtos.Count} müşteri getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent customers");
                return Result<List<CustomerDto>>.InternalError("Son müşteriler alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}