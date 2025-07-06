using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer
{
    public class GetPoliciesByCustomerQueryHandler : IRequestHandler<GetPoliciesByCustomerQuery, Result<List<PolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetPoliciesByCustomerQueryHandler> _logger;

        public GetPoliciesByCustomerQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetPoliciesByCustomerQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetPoliciesByCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<List<PolicyDto>>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.CustomerId);
                var policyDtos = _mapper.Map<List<PolicyDto>>(policies);

                return Result<List<PolicyDto>>.Success(policyDtos, $"Müşteriye ait {policyDtos.Count} poliçe getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policies for customer {CustomerId}", request.CustomerId);
                return Result<List<PolicyDto>>.InternalError("Müşteri poliçeleri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
