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

namespace IAMS.Application.Features.Parametric.Queries.GetVillages
{
    public class GetVillagesQueryHandler : IRequestHandler<GetVillagesQuery, Result<List<VillageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVillagesQueryHandler> _logger;

        public GetVillagesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVillagesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VillageDto>>> Handle(GetVillagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var villages = await _unitOfWork.Villages.GetAllAsync();
                var dtos = _mapper.Map<List<VillageDto>>(villages
                    .OrderBy(v => v.DisplayOrder)
                    .ToList());

                return Result<List<VillageDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving villages");
                return Result<List<VillageDto>>.InternalError("Köyler yüklenirken hata oluştu");
            }
        }
    }

    public class GetVillagesBySubdistrictIdQueryHandler : IRequestHandler<GetVillagesBySubdistrictIdQuery, Result<List<VillageDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVillagesBySubdistrictIdQueryHandler> _logger;

        public GetVillagesBySubdistrictIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVillagesBySubdistrictIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VillageDto>>> Handle(GetVillagesBySubdistrictIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var villages = await _unitOfWork.Villages.GetBySubdistrictIdAsync(request.SubdistrictId);
                var dtos = _mapper.Map<List<VillageDto>>(villages
                    .OrderBy(v => v.DisplayOrder)
                    .ToList());

                return Result<List<VillageDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving villages for subdistrict {SubdistrictId}", request.SubdistrictId);
                return Result<List<VillageDto>>.InternalError("Köyler yüklenirken hata oluştu");
            }
        }
    }

    public class GetVillageByIdQueryHandler : IRequestHandler<GetVillageByIdQuery, Result<VillageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVillageByIdQueryHandler> _logger;

        public GetVillageByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVillageByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<VillageDto>> Handle(GetVillageByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var village = await _unitOfWork.Villages.GetByIdAsync(request.Id);
                if (village == null)
                    return Result<VillageDto>.NotFound("Köy bulunamadı");

                var dto = _mapper.Map<VillageDto>(village);
                return Result<VillageDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving village with ID {VillageId}", request.Id);
                return Result<VillageDto>.InternalError("Köy bilgisi yüklenirken hata oluştu");
            }
        }
    }
}






