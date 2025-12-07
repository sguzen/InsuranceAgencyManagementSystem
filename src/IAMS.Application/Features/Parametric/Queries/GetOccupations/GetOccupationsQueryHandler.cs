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

namespace IAMS.Application.Features.Parametric.Queries.GetOccupations
{
    public class GetOccupationsQueryHandler : IRequestHandler<GetOccupationsQuery, Result<List<OccupationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOccupationsQueryHandler> _logger;

        public GetOccupationsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOccupationsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<OccupationDto>>> Handle(GetOccupationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var occupations = await _unitOfWork.Occupations.GetAllAsync();
                var dtos = _mapper.Map<List<OccupationDto>>(occupations
                    .OrderBy(o => o.DisplayOrder)
                    .ToList());

                return Result<List<OccupationDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving occupations");
                return Result<List<OccupationDto>>.InternalError("Meslekler yüklenirken hata oluştu");
            }
        }
    }

    public class GetOccupationByIdQueryHandler : IRequestHandler<GetOccupationByIdQuery, Result<OccupationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOccupationByIdQueryHandler> _logger;

        public GetOccupationByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOccupationByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<OccupationDto>> Handle(GetOccupationByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var occupation = await _unitOfWork.Occupations.GetByIdAsync(request.Id);
                if (occupation == null)
                    return Result<OccupationDto>.NotFound("Meslek bulunamadı");

                var dto = _mapper.Map<OccupationDto>(occupation);
                return Result<OccupationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving occupation with ID {OccupationId}", request.Id);
                return Result<OccupationDto>.InternalError("Meslek bilgisi yüklenirken hata oluştu");
            }
        }
    }
}







