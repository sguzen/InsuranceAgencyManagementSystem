using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Persistence.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsDueThisMonth
{
    public class GetPaymentsDueThisMonthQueryHandler : IRequestHandler<GetPaymentsDueThisMonthQuery, List<PolicyPaymentDto>>
    {
        private readonly PolicyPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentsDueThisMonthQueryHandler> _logger;

        public GetPaymentsDueThisMonthQueryHandler(
            PolicyPaymentRepository repository,
            IMapper mapper,
            ILogger<GetPaymentsDueThisMonthQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> Handle(GetPaymentsDueThisMonthQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Use optimized DTO projection method - projects directly in database
                var payments = await _repository.GetPaymentsDueThisMonthDtoAsync();
                return _mapper.Map<List<PolicyPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments due this month");
                throw;
            }
        }
    }
}
