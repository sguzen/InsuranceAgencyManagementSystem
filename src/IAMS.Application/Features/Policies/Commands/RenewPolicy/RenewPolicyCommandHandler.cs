using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.RenewPolicy
{
    public class RenewPolicyCommandHandler : IRequestHandler<RenewPolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RenewPolicyCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public RenewPolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RenewPolicyCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PolicyDto>> Handle(RenewPolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var originalPolicy = await _unitOfWork.Policies.GetByIdAsync(request.OriginalPolicyId);
                if (originalPolicy == null)
                {
                    return Result<PolicyDto>.NotFound("Orijinal poliçe bulunamadı");
                }

                var currentUserName = _currentUserService.UserName ?? "System";
                var renewalPolicy = originalPolicy.CreateRenewal(
                    request.NewStartDate,
                    request.NewEndDate,
                    request.NewPremiumAmount,
                    currentUserName
                );

                await _unitOfWork.Policies.AddAsync(renewalPolicy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var renewalPolicyDto = _mapper.Map<PolicyDto>(renewalPolicy);
                _logger.LogInformation("Policy renewed successfully. Original ID: {OriginalPolicyId}, Renewal ID: {RenewalPolicyId}",
                    request.OriginalPolicyId, renewalPolicy.Id);

                return Result<PolicyDto>.Success(renewalPolicyDto, "Poliçe başarıyla yenilendi");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error renewing policy with ID: {PolicyId}", request.OriginalPolicyId);
                return Result<PolicyDto>.ValidationFailure("İş kuralı ihlali", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing policy with ID: {PolicyId}", request.OriginalPolicyId);
                return Result<PolicyDto>.InternalError("Poliçe yenilenirken beklenmeyen bir hata oluştu");
            }
        }
    }
}