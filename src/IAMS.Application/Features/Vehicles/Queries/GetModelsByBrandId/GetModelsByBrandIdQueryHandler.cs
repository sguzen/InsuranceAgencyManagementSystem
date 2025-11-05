using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetModelsByBrandId
{
    public class GetModelsByBrandIdQueryHandler : IRequestHandler<GetModelsByBrandIdQuery, Result<List<VehicleModelDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetModelsByBrandIdQueryHandler> _logger;

        public GetModelsByBrandIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetModelsByBrandIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleModelDto>>> Handle(GetModelsByBrandIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var models = await _unitOfWork.VehicleModels.GetByBrandIdAsync(request.BrandId);
                var modelDtos = _mapper.Map<List<VehicleModelDto>>(models);

                return Result<List<VehicleModelDto>>.Success(modelDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle models for brand ID: {BrandId}", request.BrandId);
                return Result<List<VehicleModelDto>>.InternalError("Error retrieving vehicle models", new List<string> { ex.Message });
            }
        }
    }
}
