using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsPaged
{
    public class GetPaymentsPagedQueryHandler : IRequestHandler<GetPaymentsPagedQuery, PagedResult<PolicyPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPaymentsPagedQueryHandler> _logger;

        public GetPaymentsPagedQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPaymentsPagedQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<PolicyPaymentDto>> Handle(GetPaymentsPagedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = (await _unitOfWork.PolicyPayments.GetAllAsync()).AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(p =>
                        (p.Reference != null && p.Reference.ToLower().Contains(searchTerm)) ||
                        (p.Notes != null && p.Notes.ToLower().Contains(searchTerm)));
                }

                var totalCount = query.Count();
                var payments = query
                    .OrderByDescending(p => p.CreatedOn)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var paymentDtos = _mapper.Map<List<PolicyPaymentDto>>(payments);

                return PagedResult<PolicyPaymentDto>.Success(paymentDtos, totalCount, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged payments");
                return PagedResult<PolicyPaymentDto>.InternalError("Error retrieving payments", new List<string> { ex.Message }, request.PageNumber, request.PageSize);
            }
        }
    }
}
