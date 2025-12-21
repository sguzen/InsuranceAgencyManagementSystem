using AutoMapper;
using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetActiveModelsByBrandId
{
    public class GetActiveModelsByBrandIdQueryHandler : IRequestHandler<GetActiveModelsByBrandIdQuery, Result<List<VehicleModelDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActiveModelsByBrandIdQueryHandler> _logger;

        public GetActiveModelsByBrandIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetActiveModelsByBrandIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleModelDto>>> Handle(GetActiveModelsByBrandIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var models = await _unitOfWork.VehicleModels.GetActiveByBrandIdAsync(request.BrandId);
                var modelDtos = _mapper.Map<List<VehicleModelDto>>(models);

                return Result<List<VehicleModelDto>>.Success(modelDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active vehicle models for brand ID: {BrandId}", request.BrandId);
                return Result<List<VehicleModelDto>>.InternalError("Error retrieving active vehicle models", new List<string> { ex.Message });
            }
        }
    }
}
