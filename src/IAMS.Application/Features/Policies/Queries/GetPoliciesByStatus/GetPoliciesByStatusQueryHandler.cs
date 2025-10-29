using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Policies.Queries.GetPolicies;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus
{
    public class GetPoliciesByStatusQueryHandler : IRequestHandler<GetPoliciesByStatusQuery, Result<Dictionary<PolicyStatus, int>>>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPoliciesByStatusQueryHandler> _logger;

        public GetPoliciesByStatusQueryHandler(
            IPolicyRepository policyRepository,
            IMapper mapper,
            ILogger<GetPoliciesByStatusQueryHandler> logger)
        {
            _policyRepository = policyRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Result<Dictionary<PolicyStatus, int>>> Handle(GetPoliciesByStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var policy = await _policyRepository.GetPoliciesByStatusAsync(request.PolicyStatus);
                if (policy == null)
                {
                    return Result<Dictionary<PolicyStatus, int>>.NotFound($"'{request.PolicyStatus}' poliçeleri bulunamadı");
                }

                var result = _mapper.Map<PolicyDto>(policy);
                return Result<Dictionary<PolicyStatus, int>>.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by statud: {PolicyStatus}", request.PolicyStatus);
                return Result<Dictionary<PolicyStatus, int>>.InternalError("Poliçe getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
