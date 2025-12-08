using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Commands.UpdateClaimStatus
{
    public class UpdateClaimStatusCommandHandler : IRequestHandler<UpdateClaimStatusCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClaimStatusCommandHandler> _logger;

        public UpdateClaimStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateClaimStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateClaimStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(request.Id);
                if (claim == null)
                {
                    return Result.NotFound($"Claim with ID {request.Id} not found");
                }

                claim.Status = request.Status;
                claim.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyClaims.Update(claim);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Claim status updated successfully for ID: {ClaimId}", request.Id);
                return Result.Success("Claim status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating claim status for ID: {ClaimId}", request.Id);
                return Result.InternalError("Error updating claim status", new List<string> { ex.Message });
            }
        }
    }
}
