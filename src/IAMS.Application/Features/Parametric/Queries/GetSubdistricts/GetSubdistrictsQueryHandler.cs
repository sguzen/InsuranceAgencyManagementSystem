using AutoMapper;
using IAMS.Application.DTOs.Parametric;
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

namespace IAMS.Application.Features.Parametric.Queries.GetSubdistricts
{
    public class GetSubdistrictsQueryHandler : IRequestHandler<GetSubdistrictsQuery, Result<List<SubdistrictDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSubdistrictsQueryHandler> _logger;

        public GetSubdistrictsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetSubdistrictsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<SubdistrictDto>>> Handle(GetSubdistrictsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var subdistricts = await _unitOfWork.Subdistricts.GetAllAsync();
                var dtos = _mapper.Map<List<SubdistrictDto>>(subdistricts
                    .OrderBy(s => s.DisplayOrder)
                    .ToList());

                return Result<List<SubdistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdistricts");
                return Result<List<SubdistrictDto>>.InternalError("Bucaklar yüklenirken hata oluştu");
            }
        }
    }

    public class GetSubdistrictsByDistrictIdQueryHandler : IRequestHandler<GetSubdistrictsByDistrictIdQuery, Result<List<SubdistrictDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSubdistrictsByDistrictIdQueryHandler> _logger;

        public GetSubdistrictsByDistrictIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetSubdistrictsByDistrictIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<SubdistrictDto>>> Handle(GetSubdistrictsByDistrictIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var subdistricts = await _unitOfWork.Subdistricts.GetByDistrictIdAsync(request.DistrictId);
                var dtos = _mapper.Map<List<SubdistrictDto>>(subdistricts
                    .OrderBy(s => s.DisplayOrder)
                    .ToList());

                return Result<List<SubdistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdistricts for district {DistrictId}", request.DistrictId);
                return Result<List<SubdistrictDto>>.InternalError("Bucaklar yüklenirken hata oluştu");
            }
        }
    }

    public class GetSubdistrictByIdQueryHandler : IRequestHandler<GetSubdistrictByIdQuery, Result<SubdistrictDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSubdistrictByIdQueryHandler> _logger;

        public GetSubdistrictByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetSubdistrictByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<SubdistrictDto>> Handle(GetSubdistrictByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var subdistrict = await _unitOfWork.Subdistricts.GetByIdAsync(request.Id);
                if (subdistrict == null)
                    return Result<SubdistrictDto>.NotFound("Bucak bulunamadı");

                var dto = _mapper.Map<SubdistrictDto>(subdistrict);
                return Result<SubdistrictDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdistrict with ID {SubdistrictId}", request.Id);
                return Result<SubdistrictDto>.InternalError("Bucak bilgisi yüklenirken hata oluştu");
            }
        }
    }
}






