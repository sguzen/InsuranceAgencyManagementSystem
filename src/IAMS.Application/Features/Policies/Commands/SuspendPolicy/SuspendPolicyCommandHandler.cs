using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.SuspendPolicy
{
    public class SuspendPolicyCommandHandler : IRequestHandler<SuspendPolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SuspendPolicyCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public SuspendPolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SuspendPolicyCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PolicyDto>> Handle(SuspendPolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(request.Id);
                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound("Poliçe bulunamadı");
                }

                var currentUserName = _currentUserService.UserName ?? "System";
                policy.SuspendPolicy(currentUserName, request.SuspensionReason);

                _unitOfWork.Policies.Update(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var policyDto = _mapper.Map<PolicyDto>(policy);
                _logger.LogInformation("Policy suspended successfully with ID: {PolicyId}", request.Id);

                return Result<PolicyDto>.Success(policyDto, "Poliçe başarıyla askıya alındı");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error suspending policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.ValidationFailure("İş kuralı ihlali", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.InternalError("Poliçe askıya alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}