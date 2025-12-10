using AutoMapper;
using AutoMapper.QueryableExtensions;
using IAMS.Application.DTOs.Claim;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsPaged
{
    public class GetClaimsPagedQueryHandler : IRequestHandler<GetClaimsPagedQuery, PagedResult<PolicyClaimDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClaimsPagedQueryHandler> _logger;

        public GetClaimsPagedQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetClaimsPagedQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<PolicyClaimDto>> Handle(GetClaimsPagedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // OPTIMIZED: Use IQueryable + ProjectTo pattern
                // 1. Get count before pagination (need to build query without skip/take)
                var countQuery = _unitOfWork.PolicyClaims.AsQueryable().Where(pc => !pc.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.ToLower();
                    countQuery = countQuery.Where(pc =>
                        pc.ClaimNumber.ToLower().Contains(search) ||
                        pc.Description.ToLower().Contains(search) ||
                        (pc.Notes != null && pc.Notes.ToLower().Contains(search)));
                }

                var totalCount = await countQuery.CountAsync(cancellationToken);

                // 2. Get paginated data with ProjectTo for efficient database-level projection
                var query = _unitOfWork.PolicyClaims.GetPagedClaimsQuery(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

                var claimDtos = await query
                    .ProjectTo<PolicyClaimDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return PagedResult<PolicyClaimDto>.Success(claimDtos, totalCount, request.PageNumber, request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged claims");
                return PagedResult<PolicyClaimDto>.InternalError("Error retrieving claims", new List<string> { ex.Message }, request.PageNumber, request.PageSize);
            }
        }
    }
}
