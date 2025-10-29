namespace IAMS.Application.DTOs.Parametric
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class OccupationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CityDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class DistrictDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class SubdistrictDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class VillageDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int SubdistrictId { get; set; }
        public string? SubdistrictName { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}