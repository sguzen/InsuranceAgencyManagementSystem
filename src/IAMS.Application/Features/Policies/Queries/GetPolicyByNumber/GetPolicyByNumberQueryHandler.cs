using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyByNumber
{
    public class GetPolicyByNumberQueryHandler : IRequestHandler<GetPolicyByNumberQuery, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetPolicyByNumberQueryHandler> _logger;

        public GetPolicyByNumberQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetPolicyByNumberQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(GetPolicyByNumberQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<PolicyDto>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var policy = await _unitOfWork.Policies.GetByPolicyNumberAsync(request.PolicyNumber);
                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound($"Poliçe numarası '{request.PolicyNumber}' bulunamadı");
                }

                var policyDto = _mapper.Map<PolicyDto>(policy);
                return Result<PolicyDto>.Success(policyDto, "Poliçe başarıyla getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy by number {PolicyNumber}", request.PolicyNumber);
                return Result<PolicyDto>.InternalError("Poliçe getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}