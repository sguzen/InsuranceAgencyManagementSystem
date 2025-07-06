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

namespace IAMS.Application.Features.Policies.Queries.GetExpiringPolicies
{
    public class GetExpiringPoliciesQueryHandler : IRequestHandler<GetExpiringPoliciesQuery, Result<List<PolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetExpiringPoliciesQueryHandler> _logger;

        public GetExpiringPoliciesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetExpiringPoliciesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetExpiringPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<List<PolicyDto>>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var tenantId = _currentTenantService.TenantId.Value;
                var expiringPolicies = await _unitOfWork.Policies.GetExpiringPoliciesAsync(request.DaysAhead, tenantId);
                var policyDtos = _mapper.Map<List<PolicyDto>>(expiringPolicies);

                return Result<List<PolicyDto>>.Success(policyDtos, $"{request.DaysAhead} gün içinde süresi dolacak {policyDtos.Count} poliçe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring policies for {DaysAhead} days", request.DaysAhead);
                return Result<List<PolicyDto>>.InternalError("Süresi dolacak poliçeler getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
