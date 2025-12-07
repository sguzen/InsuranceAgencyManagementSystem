using AutoMapper;
using IAMS.Application.DTOs.Parametric;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Parametric.Queries.GetCities
{
    public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, Result<List<CityDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCitiesQueryHandler> _logger;

        public GetCitiesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCitiesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CityDto>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cities = await _unitOfWork.Cities.GetAllAsync();
                var dtos = _mapper.Map<List<CityDto>>(cities
                    .OrderBy(c => c.DisplayOrder)
                    .ToList());

                return Result<List<CityDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities");
                return Result<List<CityDto>>.InternalError("Şehirler yüklenirken hata oluştu");
            }
        }
    }

    public class GetCityByIdQueryHandler : IRequestHandler<GetCityByIdQuery, Result<CityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCityByIdQueryHandler> _logger;

        public GetCityByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCityByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CityDto>> Handle(GetCityByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var city = await _unitOfWork.Cities.GetByIdAsync(request.Id);
                if (city == null)
                    return Result<CityDto>.NotFound("Şehir bulunamadı");

                var dto = _mapper.Map<CityDto>(city);
                return Result<CityDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city with ID {CityId}", request.Id);
                return Result<CityDto>.InternalError("Şehir bilgisi yüklenirken hata oluştu");
            }
        }
    }
}






