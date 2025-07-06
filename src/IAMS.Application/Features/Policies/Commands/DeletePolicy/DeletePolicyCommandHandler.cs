using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.DeletePolicy
{
    public class DeletePolicyCommandHandler : IRequestHandler<DeletePolicyCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<DeletePolicyCommandHandler> _logger;

        public DeletePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<DeletePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var policy = await _unitOfWork.Policies.GetByIdAsync(request.Id);
                if (policy == null)
                {
                    return Result.NotFound("Poliçe bulunamadı");
                }

                // Check if policy can be deleted
                if (policy.Status == Domain.Enums.PolicyStatus.Active)
                {
                    return Result.InternalError("Aktif poliçe silinemez. Önce iptal edin.");
                }

                // Check if there are related claims
                var hasClaims = await _unitOfWork.PolicyClaims.ExistsByPolicyIdAsync(request.Id);
                if (hasClaims)
                {
                    return Result.InternalError("Hasar kaydı bulunan poliçe silinemez");
                }

                // Soft delete
                policy.IsDeleted = true;
                policy.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Policies.Update(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Policy deleted successfully with ID: {PolicyId}", request.Id);
                return Result.Success("Poliçe başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy with ID: {PolicyId}", request.Id);
                return Result.InternalError("Poliçe silinirken beklenmeyen bir hata oluştu");
            }
        }
    }
}