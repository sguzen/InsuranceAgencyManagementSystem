using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Commands.DeleteClaim
{
    public class DeleteClaimCommandHandler : IRequestHandler<DeleteClaimCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteClaimCommandHandler> _logger;

        public DeleteClaimCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteClaimCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(request.Id);
                if (claim == null)
                {
                    return Result.NotFound($"Claim with ID {request.Id} not found");
                }

                _unitOfWork.PolicyClaims.Remove(claim);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Claim deleted successfully with ID: {ClaimId}", request.Id);
                return Result.Success("Claim deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim with ID: {ClaimId}", request.Id);
                return Result.InternalError("Error deleting claim", new List<string> { ex.Message });
            }
        }
    }
}
