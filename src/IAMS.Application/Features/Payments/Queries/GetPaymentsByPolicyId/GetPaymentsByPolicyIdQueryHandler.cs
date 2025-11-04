using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsByPolicyId
{
    public class GetPaymentsByPolicyIdQueryHandler : IRequestHandler<GetPaymentsByPolicyIdQuery, List<PolicyPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentsByPolicyIdQueryHandler> _logger;

        public GetPaymentsByPolicyIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPaymentsByPolicyIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> Handle(GetPaymentsByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payments = await _unitOfWork.PolicyPayments.GetPaymentsByPolicyIdAsync(request.PolicyId);
                return _mapper.Map<List<PolicyPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for policy ID: {PolicyId}", request.PolicyId);
                throw;
            }
        }
    }
}
