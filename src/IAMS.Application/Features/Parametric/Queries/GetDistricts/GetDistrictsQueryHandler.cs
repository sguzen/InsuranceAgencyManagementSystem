using AutoMapper;
using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Parametric.Queries.GetDistricts
{
    public class GetDistrictsQueryHandler : IRequestHandler<GetDistrictsQuery, Result<List<DistrictDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDistrictsQueryHandler> _logger;

        public GetDistrictsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetDistrictsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<DistrictDto>>> Handle(GetDistrictsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var districts = await _unitOfWork.Districts.GetAllAsync();
                var dtos = _mapper.Map<List<DistrictDto>>(districts
                    .OrderBy(d => d.DisplayOrder)
                    .ToList());

                return Result<List<DistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving districts");
                return Result<List<DistrictDto>>.InternalError("Mahalleler yüklenirken hata oluştu");
            }
        }
    }

    public class GetDistrictsByCityIdQueryHandler : IRequestHandler<GetDistrictsByCityIdQuery, Result<List<DistrictDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDistrictsByCityIdQueryHandler> _logger;

        public GetDistrictsByCityIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetDistrictsByCityIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<DistrictDto>>> Handle(GetDistrictsByCityIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var districts = await _unitOfWork.Districts.GetByCityIdAsync(request.CityId);
                var dtos = _mapper.Map<List<DistrictDto>>(districts
                    .OrderBy(d => d.DisplayOrder)
                    .ToList());

                return Result<List<DistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving districts for city {CityId}", request.CityId);
                return Result<List<DistrictDto>>.InternalError("Mahalleler yüklenirken hata oluştu");
            }
        }
    }

    public class GetDistrictByIdQueryHandler : IRequestHandler<GetDistrictByIdQuery, Result<DistrictDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDistrictByIdQueryHandler> _logger;

        public GetDistrictByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetDistrictByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<DistrictDto>> Handle(GetDistrictByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var district = await _unitOfWork.Districts.GetByIdAsync(request.Id);
                if (district == null)
                    return Result<DistrictDto>.NotFound("Mahalle bulunamadı");

                var dto = _mapper.Map<DistrictDto>(district);
                return Result<DistrictDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving district with ID {DistrictId}", request.Id);
                return Result<DistrictDto>.InternalError("Mahalle bilgisi yüklenirken hata oluştu");
            }
        }
    }
}






