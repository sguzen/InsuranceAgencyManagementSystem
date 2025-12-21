using AutoMapper;
using IAMS.Shared.DTOs.Claim;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsByPolicyId
{
    public class GetClaimsByPolicyIdQueryHandler : IRequestHandler<GetClaimsByPolicyIdQuery, List<PolicyClaimDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClaimsByPolicyIdQueryHandler> _logger;

        public GetClaimsByPolicyIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetClaimsByPolicyIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyClaimDto>> Handle(GetClaimsByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var claims = await _unitOfWork.PolicyClaims.GetClaimsByPolicyIdAsync(request.PolicyId);
                return _mapper.Map<List<PolicyClaimDto>>(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims for policy ID: {PolicyId}", request.PolicyId);
                throw;
            }
        }
    }
}
