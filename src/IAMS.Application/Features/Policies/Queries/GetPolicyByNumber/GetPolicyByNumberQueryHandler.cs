using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyByNumber
{
    public class GetPolicyByNumberQueryHandler : IRequestHandler<GetPolicyByNumberQuery, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPolicyByNumberQueryHandler> _logger;

        public GetPolicyByNumberQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPolicyByNumberQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(GetPolicyByNumberQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByPolicyNumberAsync(request.PolicyNumber);
                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound($"Poliçe numarası '{request.PolicyNumber}' bulunamadı");
                }

                var policyDto = _mapper.Map<PolicyDto>(policy);
                return Result<PolicyDto>.Success(policyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy by number: {PolicyNumber}", request.PolicyNumber);
                return Result<PolicyDto>.InternalError("Poliçe getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}