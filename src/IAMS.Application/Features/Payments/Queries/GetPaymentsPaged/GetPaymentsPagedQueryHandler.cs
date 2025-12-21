using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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
                // OPTIMIZED: Use IQueryable + ProjectTo pattern
                // 1. Get count before pagination (need to build query without skip/take)
                var countQuery = _unitOfWork.PolicyPayments.AsQueryable().Where(pp => !pp.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.ToLower();
                    countQuery = countQuery.Where(pp =>
                        (pp.Reference != null && pp.Reference.ToLower().Contains(search)) ||
                        (pp.Notes != null && pp.Notes.ToLower().Contains(search)) ||
                        pp.Policy.PolicyNumber.ToLower().Contains(search) ||
                        (pp.Policy.Customer.FirstName + " " + pp.Policy.Customer.LastName).ToLower().Contains(search));
                }

                var totalCount = await countQuery.CountAsync(cancellationToken);

                // 2. Get paginated data with ProjectTo for efficient database-level projection
                var query = _unitOfWork.PolicyPayments.GetPagedPaymentsQuery(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

                var paymentDtos = await query
                    .ProjectTo<PolicyPaymentListDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Map to full DTO if needed (or return PolicyPaymentListDto directly)
                var fullDtos = _mapper.Map<List<PolicyPaymentDto>>(paymentDtos);

                return PagedResult<PolicyPaymentDto>.Success(fullDtos, totalCount, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged payments");
                return PagedResult<PolicyPaymentDto>.InternalError("Error retrieving payments", new List<string> { ex.Message }, request.PageNumber, request.PageSize);
            }
        }
    }
}
