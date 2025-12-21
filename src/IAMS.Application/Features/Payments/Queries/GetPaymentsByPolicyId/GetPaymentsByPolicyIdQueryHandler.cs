using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
                // Use IQueryable + ProjectTo for optimized database projection
                var query = _unitOfWork.PolicyPayments.GetPaymentsByPolicyIdQuery(request.PolicyId);
                var payments = await query
                    .ProjectTo<PolicyPaymentListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

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
