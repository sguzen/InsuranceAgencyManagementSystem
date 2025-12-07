using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetTopPoliciesByPremium
{
    public class GetTopPoliciesByPremiumQueryHandler : IRequestHandler<GetTopPoliciesByPremiumQuery, Result<List<PolicyDto>>>
    {
        private readonly IPolicyQueryService _policyQueryService;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetTopPoliciesByPremiumQueryHandler> _logger;

        public GetTopPoliciesByPremiumQueryHandler(
            IPolicyQueryService policyQueryService,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetTopPoliciesByPremiumQueryHandler> logger)
        {
            _policyQueryService = policyQueryService;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetTopPoliciesByPremiumQuery request, CancellationToken cancellationToken)
        {
            try
            {


                var topPolicies = await _policyQueryService.GetTopPoliciesByPremiumAsync(request.Count);
                var policyDtos = _mapper.Map<List<PolicyDto>>(topPolicies);

                return Result<List<PolicyDto>>.Success(policyDtos, $"En yüksek primli {request.Count} poliçe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top policies by premium");
                return Result<List<PolicyDto>>.InternalError("En yüksek primli poliçeler getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
