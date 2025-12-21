using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Queries.GetPolicy
{
    public class GetPolicyQueryHandler : IRequestHandler<GetPolicyQuery, Result<PolicyDto>>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPolicyQueryHandler> _logger;

        public GetPolicyQueryHandler(
            IPolicyRepository policyRepository,
            IMapper mapper,
            ILogger<GetPolicyQueryHandler> logger)
        {
            _policyRepository = policyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(GetPolicyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var policy = await _policyRepository.GetPolicyByIdWithDetailsAsync(request.Id);

                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound($"Policy with ID {request.Id} not found");
                }

                var policyDto = _mapper.Map<PolicyDto>(policy);
                return Result<PolicyDto>.Success(policyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading policy with ID {PolicyId}", request.Id);
                return Result<PolicyDto>.InternalError("Poliçe yüklenirken hata oluştu");
            }
        }
    }
}
