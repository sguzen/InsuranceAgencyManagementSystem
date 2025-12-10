using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Persistence.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetOverduePayments
{
    public class GetOverduePaymentsQueryHandler : IRequestHandler<GetOverduePaymentsQuery, List<PolicyPaymentDto>>
    {
        private readonly PolicyPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOverduePaymentsQueryHandler> _logger;

        public GetOverduePaymentsQueryHandler(
            PolicyPaymentRepository repository,
            IMapper mapper,
            ILogger<GetOverduePaymentsQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> Handle(GetOverduePaymentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Use optimized DTO projection method - projects directly in database
                var payments = await _repository.GetOverduePaymentsDtoAsync();
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
