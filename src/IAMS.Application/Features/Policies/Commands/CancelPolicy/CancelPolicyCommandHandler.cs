using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.CancelPolicy
{
    public class CancelPolicyCommandHandler : IRequestHandler<CancelPolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CancelPolicyCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CancelPolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CancelPolicyCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PolicyDto>> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(request.Id);
                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound("Poliçe bulunamadı");
                }

                var currentUserName = _currentUserService.UserName ?? "System";
                policy.CancelPolicy(currentUserName, request.CancellationReason);

                _unitOfWork.Policies.Update(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var policyDto = _mapper.Map<PolicyDto>(policy);
                _logger.LogInformation("Policy cancelled successfully with ID: {PolicyId}", request.Id);

                return Result<PolicyDto>.Success(policyDto, "Poliçe başarıyla iptal edildi");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error cancelling policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.ValidationFailure("İş kuralı ihlali", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.InternalError("Poliçe iptal edilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}