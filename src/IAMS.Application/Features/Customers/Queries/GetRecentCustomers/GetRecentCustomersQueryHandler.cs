using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

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
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<List<CustomerDto>>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var tenantId = _currentTenantService.TenantId.Value;
                var customers = await _unitOfWork.Customers.GetRecentCustomersAsync(request.Count);
                var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

                _logger.LogDebug("Retrieved {Count} recent customers for tenant {TenantId}", customerDtos.Count, tenantId);

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