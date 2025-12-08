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
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Queries.GetExpiringPolicies
{
    public class GetExpiringPoliciesQueryHandler : IRequestHandler<GetExpiringPoliciesQuery, Result<List<PolicyDto>>>
    {
        private readonly IPolicyQueryService _policyQueryService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetExpiringPoliciesQueryHandler> _logger;

        public GetExpiringPoliciesQueryHandler(
            IPolicyQueryService policyQueryService,
            IMapper mapper,
            ILogger<GetExpiringPoliciesQueryHandler> logger)
        {
            _policyQueryService = policyQueryService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetExpiringPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var expiringPolicies = await _policyQueryService.GetExpiringPoliciesAsync(request.DaysAhead);
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
