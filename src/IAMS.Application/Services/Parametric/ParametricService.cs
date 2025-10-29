using AutoMapper;
using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Parametric
{
    public class ParametricService : IParametricService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ParametricService> _logger;

        public ParametricService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ParametricService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Countries

        public async Task<Result<List<CountryDto>>> GetCountriesAsync()
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
                return Result<List<CountryDto>>.Failure("Ülkeler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<CountryDto>> GetCountryByIdAsync(int id)
        {
            try
            {
                var country = await _unitOfWork.Countries.GetByIdAsync(id);
                if (country == null)
                    return Result<CountryDto>.Failure("Ülke bulunamadı");

                var dto = _mapper.Map<CountryDto>(country);
                return Result<CountryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country with ID {CountryId}", id);
                return Result<CountryDto>.Failure("Ülke bilgisi yüklenirken hata oluştu");
            }
        }

        public async Task<Result<CountryDto>> GetCountryByCodeAsync(string code)
        {
            try
            {
                var country = await _unitOfWork.Countries.GetByCodeAsync(code);
                if (country == null)
                    return Result<CountryDto>.Failure("Ülke bulunamadı");

                var dto = _mapper.Map<CountryDto>(country);
                return Result<CountryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country with code {Code}", code);
                return Result<CountryDto>.Failure("Ülke bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion

        #region Occupations

        public async Task<Result<List<OccupationDto>>> GetOccupationsAsync()
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
                return Result<List<OccupationDto>>.Failure("Meslekler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<OccupationDto>> GetOccupationByIdAsync(int id)
        {
            try
            {
                var occupation = await _unitOfWork.Occupations.GetByIdAsync(id);
                if (occupation == null)
                    return Result<OccupationDto>.Failure("Meslek bulunamadı");

                var dto = _mapper.Map<OccupationDto>(occupation);
                return Result<OccupationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving occupation with ID {OccupationId}", id);
                return Result<OccupationDto>.Failure("Meslek bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion

        #region Cities

        public async Task<Result<List<CityDto>>> GetCitiesAsync()
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
                return Result<List<CityDto>>.Failure("Şehirler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<CityDto>> GetCityByIdAsync(int id)
        {
            try
            {
                var city = await _unitOfWork.Cities.GetByIdAsync(id);
                if (city == null)
                    return Result<CityDto>.Failure("Şehir bulunamadı");

                var dto = _mapper.Map<CityDto>(city);
                return Result<CityDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city with ID {CityId}", id);
                return Result<CityDto>.Failure("Şehir bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion

        #region Districts

        public async Task<Result<List<DistrictDto>>> GetDistrictsAsync()
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
                return Result<List<DistrictDto>>.Failure("Mahalleler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<List<DistrictDto>>> GetDistrictsByCityIdAsync(int cityId)
        {
            try
            {
                var districts = await _unitOfWork.Districts.GetByCityIdAsync(cityId);
                var dtos = _mapper.Map<List<DistrictDto>>(districts
                    .OrderBy(d => d.DisplayOrder)
                    .ToList());

                return Result<List<DistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving districts for city {CityId}", cityId);
                return Result<List<DistrictDto>>.Failure("Mahalleler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<DistrictDto>> GetDistrictByIdAsync(int id)
        {
            try
            {
                var district = await _unitOfWork.Districts.GetByIdAsync(id);
                if (district == null)
                    return Result<DistrictDto>.Failure("Mahalle bulunamadı");

                var dto = _mapper.Map<DistrictDto>(district);
                return Result<DistrictDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving district with ID {DistrictId}", id);
                return Result<DistrictDto>.Failure("Mahalle bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion

        #region Subdistricts

        public async Task<Result<List<SubdistrictDto>>> GetSubdistrictsAsync()
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
                return Result<List<SubdistrictDto>>.Failure("Bucaklar yüklenirken hata oluştu");
            }
        }

        public async Task<Result<List<SubdistrictDto>>> GetSubdistrictsByDistrictIdAsync(int districtId)
        {
            try
            {
                var subdistricts = await _unitOfWork.Subdistricts.GetByDistrictIdAsync(districtId);
                var dtos = _mapper.Map<List<SubdistrictDto>>(subdistricts
                    .OrderBy(s => s.DisplayOrder)
                    .ToList());

                return Result<List<SubdistrictDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdistricts for district {DistrictId}", districtId);
                return Result<List<SubdistrictDto>>.Failure("Bucaklar yüklenirken hata oluştu");
            }
        }

        public async Task<Result<SubdistrictDto>> GetSubdistrictByIdAsync(int id)
        {
            try
            {
                var subdistrict = await _unitOfWork.Subdistricts.GetByIdAsync(id);
                if (subdistrict == null)
                    return Result<SubdistrictDto>.Failure("Bucak bulunamadı");

                var dto = _mapper.Map<SubdistrictDto>(subdistrict);
                return Result<SubdistrictDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdistrict with ID {SubdistrictId}", id);
                return Result<SubdistrictDto>.Failure("Bucak bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion

        #region Villages

        public async Task<Result<List<VillageDto>>> GetVillagesAsync()
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
                return Result<List<VillageDto>>.Failure("Köyler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<List<VillageDto>>> GetVillagesBySubdistrictIdAsync(int subdistrictId)
        {
            try
            {
                var villages = await _unitOfWork.Villages.GetBySubdistrictIdAsync(subdistrictId);
                var dtos = _mapper.Map<List<VillageDto>>(villages
                    .OrderBy(v => v.DisplayOrder)
                    .ToList());

                return Result<List<VillageDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving villages for subdistrict {SubdistrictId}", subdistrictId);
                return Result<List<VillageDto>>.Failure("Köyler yüklenirken hata oluştu");
            }
        }

        public async Task<Result<VillageDto>> GetVillageByIdAsync(int id)
        {
            try
            {
                var village = await _unitOfWork.Villages.GetByIdAsync(id);
                if (village == null)
                    return Result<VillageDto>.Failure("Köy bulunamadı");

                var dto = _mapper.Map<VillageDto>(village);
                return Result<VillageDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving village with ID {VillageId}", id);
                return Result<VillageDto>.Failure("Köy bilgisi yüklenirken hata oluştu");
            }
        }

        #endregion
    }
}