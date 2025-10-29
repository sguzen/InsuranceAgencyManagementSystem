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

namespace IAMS.Application.Features.Policies.Queries.GetRecentPolicies
{
    public class GetRecentPoliciesQueryHandler : IRequestHandler<GetRecentPoliciesQuery, Result<List<PolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRecentPoliciesQueryHandler> _logger;

        public GetRecentPoliciesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetRecentPoliciesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetRecentPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
               
                var recentPolicies = await _unitOfWork.Policies.GetRecentPoliciesAsync(request.Count);
                var policyDtos = _mapper.Map<List<PolicyDto>>(recentPolicies);

                return Result<List<PolicyDto>>.Success(policyDtos, $"Son {request.Count} poliçe getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent policies");
                return Result<List<PolicyDto>>.InternalError("Son poliçeler getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}

