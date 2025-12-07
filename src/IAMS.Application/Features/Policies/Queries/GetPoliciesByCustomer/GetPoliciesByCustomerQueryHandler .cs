using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer
{
    public class GetPoliciesByCustomerQueryHandler : IRequestHandler<GetPoliciesByCustomerQuery, Result<List<PolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPoliciesByCustomerQueryHandler> _logger;

        public GetPoliciesByCustomerQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPoliciesByCustomerQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetPoliciesByCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.CustomerId);
                var policyDtos = _mapper.Map<List<PolicyDto>>(policies);

                return Result<List<PolicyDto>>.Success(policyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies for customer: {CustomerId}", request.CustomerId);
                return Result<List<PolicyDto>>.InternalError("Müşteri poliçeleri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}