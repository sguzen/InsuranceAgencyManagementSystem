using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Queries.GetClaimById
{
    public class GetClaimByIdQueryHandler : IRequestHandler<GetClaimByIdQuery, PolicyClaimDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClaimByIdQueryHandler> _logger;

        public GetClaimByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetClaimByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PolicyClaimDto?> Handle(GetClaimByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(request.Id);
                return claim != null ? _mapper.Map<PolicyClaimDto>(claim) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claim with ID: {ClaimId}", request.Id);
                throw;
            }
        }
    }
}
