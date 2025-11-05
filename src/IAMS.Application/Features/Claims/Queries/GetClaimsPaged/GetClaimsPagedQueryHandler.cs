using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
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
                var query = (await _unitOfWork.PolicyClaims.GetAllAsync()).AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.ClaimNumber.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm) ||
                        (c.Notes != null && c.Notes.ToLower().Contains(searchTerm)));
                }

                var totalCount = query.Count();
                var claims = query
                    .OrderByDescending(c => c.CreatedOn)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var claimDtos = _mapper.Map<List<PolicyClaimDto>>(claims);

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
