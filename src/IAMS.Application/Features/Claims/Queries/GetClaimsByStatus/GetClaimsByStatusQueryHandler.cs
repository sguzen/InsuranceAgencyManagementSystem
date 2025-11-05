using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsByStatus
{
    public class GetClaimsByStatusQueryHandler : IRequestHandler<GetClaimsByStatusQuery, List<PolicyClaimDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClaimsByStatusQueryHandler> _logger;

        public GetClaimsByStatusQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetClaimsByStatusQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyClaimDto>> Handle(GetClaimsByStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var claims = await _unitOfWork.PolicyClaims.GetClaimsByStatusAsync(request.Status);
                return _mapper.Map<List<PolicyClaimDto>>(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims by status: {Status}", request.Status);
                throw;
            }
        }
    }
}
