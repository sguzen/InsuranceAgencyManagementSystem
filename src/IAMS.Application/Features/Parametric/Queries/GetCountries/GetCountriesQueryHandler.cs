using AutoMapper;
using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Features.Parametric.Queries;
using IAMS.Application.Features.Parametric.Queries.GetCountries;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Parametric.Queries.GetCountries;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<List<CountryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountriesQueryHandler> _logger;

    public GetCountriesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCountriesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var countries = await _unitOfWork.Countries.GetAllAsync();
            var dtos = _mapper.Map<List<CountryDto>>(countries
                .OrderBy(c => c.DisplayOrder)
                .ToList());

            return Result<List<CountryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving countries");
            return Result<List<CountryDto>>.InternalError("Ülkeler yüklenirken hata oluştu");
        }
    }
}

public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountryByIdQueryHandler> _logger;

    public GetCountryByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCountryByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var country = await _unitOfWork.Countries.GetByIdAsync(request.Id);
            if (country == null)
                return Result<CountryDto>.NotFound("Ülke bulunamadı");

            var dto = _mapper.Map<CountryDto>(country);
            return Result<CountryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving country with ID {CountryId}", request.Id);
            return Result<CountryDto>.InternalError("Ülke bilgisi yüklenirken hata oluştu");
        }
    }
}

public class GetCountryByCodeQueryHandler : IRequestHandler<GetCountryByCodeQuery, Result<CountryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountryByCodeQueryHandler> _logger;

    public GetCountryByCodeQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCountryByCodeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CountryDto>> Handle(GetCountryByCodeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var country = await _unitOfWork.Countries.GetByCodeAsync(request.Code);
            if (country == null)
                return Result<CountryDto>.NotFound("Ülke bulunamadı");

            var dto = _mapper.Map<CountryDto>(country);
            return Result<CountryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving country with code {CountryCode}", request.Code);
            return Result<CountryDto>.InternalError("Ülke bilgisi yüklenirken hata oluştu");
        }
    }
}