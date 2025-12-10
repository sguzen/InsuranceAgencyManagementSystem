using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetOverduePayments
{
    public class GetOverduePaymentsQueryHandler : IRequestHandler<GetOverduePaymentsQuery, List<PolicyPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOverduePaymentsQueryHandler> _logger;

        public GetOverduePaymentsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOverduePaymentsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> Handle(GetOverduePaymentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Use IQueryable + ProjectTo for optimized database projection
                // AutoMapper generates SQL SELECT with only required fields
                var query = _unitOfWork.PolicyPayments.GetOverduePaymentsQuery();
                var payments = await query
                    .ProjectTo<PolicyPaymentListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return _mapper.Map<List<PolicyPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue payments");
                throw;
            }
        }
    }
}
