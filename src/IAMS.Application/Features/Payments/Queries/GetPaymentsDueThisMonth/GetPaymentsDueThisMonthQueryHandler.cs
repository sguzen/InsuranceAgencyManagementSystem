using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsDueThisMonth
{
    public class GetPaymentsDueThisMonthQueryHandler : IRequestHandler<GetPaymentsDueThisMonthQuery, List<PolicyPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentsDueThisMonthQueryHandler> _logger;

        public GetPaymentsDueThisMonthQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPaymentsDueThisMonthQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> Handle(GetPaymentsDueThisMonthQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Use optimized DTO projection method - projects directly in database
                var payments = await _unitOfWork.PolicyPayments.GetPaymentsDueThisMonthDtoAsync();
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
