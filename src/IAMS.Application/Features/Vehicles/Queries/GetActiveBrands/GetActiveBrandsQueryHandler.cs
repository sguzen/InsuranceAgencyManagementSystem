using AutoMapper;
using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetActiveBrands
{
    public class GetActiveBrandsQueryHandler : IRequestHandler<GetActiveBrandsQuery, Result<List<VehicleBrandDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActiveBrandsQueryHandler> _logger;

        public GetActiveBrandsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetActiveBrandsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleBrandDto>>> Handle(GetActiveBrandsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var brands = await _unitOfWork.VehicleBrands.GetActiveBrandsAsync();
                var brandDtos = _mapper.Map<List<VehicleBrandDto>>(brands);

                return Result<List<VehicleBrandDto>>.Success(brandDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active vehicle brands");
                return Result<List<VehicleBrandDto>>.InternalError("Error retrieving active vehicle brands", new List<string> { ex.Message });
            }
        }
    }
}
