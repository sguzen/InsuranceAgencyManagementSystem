using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetAllBrands
{
    public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, Result<List<VehicleBrandDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllBrandsQueryHandler> _logger;

        public GetAllBrandsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetAllBrandsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleBrandDto>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var brands = await _unitOfWork.VehicleBrands.GetAllAsync();
                var brandDtos = _mapper.Map<List<VehicleBrandDto>>(brands);

                return Result<List<VehicleBrandDto>>.Success(brandDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vehicle brands");
                return Result<List<VehicleBrandDto>>.InternalError("Error retrieving vehicle brands", new List<string> { ex.Message });
            }
        }
    }
}
