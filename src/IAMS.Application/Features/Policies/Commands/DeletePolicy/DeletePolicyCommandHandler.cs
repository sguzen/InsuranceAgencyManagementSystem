using MediatR;
using IAMS.Application.Models;
using IAMS.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.DeletePolicy
{
    public class DeletePolicyCommandHandler : IRequestHandler<DeletePolicyCommand, Result>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeletePolicyCommandHandler> _logger;

        public DeletePolicyCommandHandler(
            IPolicyRepository policyRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeletePolicyCommandHandler> logger)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting policy with ID: {PolicyId}", request.Id);

                var policy = await _policyRepository.GetByIdAsync(request.Id);
                if (policy == null)
                {
                    return Result.NotFound("Policy not found");
                }

                // Business rule: Cannot delete active policies, must cancel first
                if (policy.Status == Domain.Enums.PolicyStatus.Active)
                {
                    return Result.Failure("Cannot delete active policy. Cancel the policy first.");
                }

                // Soft delete
                policy.Delete("System"); // TODO: Get from current user context
                _policyRepository.Update(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Policy deleted successfully: {PolicyId}", request.Id);

                return Result.Success("Policy deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy with ID: {PolicyId}", request.Id);
                return Result.InternalError("An error occurred while deleting the policy");
            }
        }
    }
}