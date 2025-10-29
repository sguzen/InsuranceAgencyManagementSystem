using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Data
{
    /// <summary>
    /// Seed data for KKTC (Turkish Republic of Northern Cyprus) parametric tables
    /// </summary>
    public static class ParametricDataSeeder
    {
        public static void SeedParametricData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Countries
            SeedCountries(modelBuilder, seedDate);

            // Seed Occupations
            SeedOccupations(modelBuilder, seedDate);

            // Seed KKTC Cities
            SeedCities(modelBuilder, seedDate);

            // Seed Districts (Mahalleler)
            SeedDistricts(modelBuilder, seedDate);

            // Additional subdistricts and villages can be added as needed
        }

        private static void SeedCountries(ModelBuilder modelBuilder, DateTime seedDate)
        {
            modelBuilder.Entity<Country>().HasData(
                new Country
                {
                    Id = 1,
                    Code = "601",
                    NameTr = "Kuzey Kıbrıs Türk Cumhuriyeti",
                    NameEn = "Turkish Republic of Northern Cyprus",
                    PhoneCode = "+392",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new Country
                {
                    Id = 2,
                    Code = "052",
                    NameTr = "Türkiye",
                    NameEn = "Turkey",
                    PhoneCode = "+90",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new Country
                {
                    Id = 3,
                    Code = "044",
                    NameTr = "Birleşik Krallık",
                    NameEn = "United Kingdom",
                    PhoneCode = "+44",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new Country
                {
                    Id = 4,
                    Code = "001",
                    NameTr = "Amerika Birleşik Devletleri",
                    NameEn = "United States",
                    PhoneCode = "+1",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new Country
                {
                    Id = 5,
                    Code = "030",
                    NameTr = "Yunanistan",
                    NameEn = "Greece",
                    PhoneCode = "+30",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                }
            );
        }

        private static void SeedOccupations(ModelBuilder modelBuilder, DateTime seedDate)
        {
            modelBuilder.Entity<Occupation>().HasData(
                new Occupation { Id = 1, Code = "001", NameTr = "Doktor", NameEn = "Doctor", IsActive = true, DisplayOrder = 1, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 2, Code = "002", NameTr = "Mühendis", NameEn = "Engineer", IsActive = true, DisplayOrder = 2, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 3, Code = "003", NameTr = "Öğretmen", NameEn = "Teacher", IsActive = true, DisplayOrder = 3, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 4, Code = "004", NameTr = "Avukat", NameEn = "Lawyer", IsActive = true, DisplayOrder = 4, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 5, Code = "005", NameTr = "Muhasebeci", NameEn = "Accountant", IsActive = true, DisplayOrder = 5, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 6, Code = "006", NameTr = "İşletme Sahibi", NameEn = "Business Owner", IsActive = true, DisplayOrder = 6, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 7, Code = "007", NameTr = "Memur", NameEn = "Civil Servant", IsActive = true, DisplayOrder = 7, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 8, Code = "008", NameTr = "Emekli", NameEn = "Retired", IsActive = true, DisplayOrder = 8, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 9, Code = "009", NameTr = "Öğrenci", NameEn = "Student", IsActive = true, DisplayOrder = 9, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 10, Code = "010", NameTr = "Ev Hanımı", NameEn = "Homemaker", IsActive = true, DisplayOrder = 10, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 11, Code = "011", NameTr = "Serbest Meslek", NameEn = "Freelancer", IsActive = true, DisplayOrder = 11, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 12, Code = "012", NameTr = "Teknisyen", NameEn = "Technician", IsActive = true, DisplayOrder = 12, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 13, Code = "013", NameTr = "Satış Danışmanı", NameEn = "Sales Consultant", IsActive = true, DisplayOrder = 13, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 14, Code = "014", NameTr = "Bankacı", NameEn = "Banker", IsActive = true, DisplayOrder = 14, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 15, Code = "015", NameTr = "Hemşire", NameEn = "Nurse", IsActive = true, DisplayOrder = 15, CreatedOn = seedDate, CreatedBy = "System" },
                new Occupation { Id = 16, Code = "999", NameTr = "Diğer", NameEn = "Other", IsActive = true, DisplayOrder = 99, CreatedOn = seedDate, CreatedBy = "System" }
            );
        }

        private static void SeedCities(ModelBuilder modelBuilder, DateTime seedDate)
        {
            // Major cities of KKTC (Turkish Republic of Northern Cyprus)
            modelBuilder.Entity<City>().HasData(
                new City
                {
                    Id = 1,
                    Code = "01",
                    NameTr = "Lefkoşa",
                    NameEn = "Nicosia",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new City
                {
                    Id = 2,
                    Code = "02",
                    NameTr = "Gazimağusa",
                    NameEn = "Famagusta",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new City
                {
                    Id = 3,
                    Code = "03",
                    NameTr = "Girne",
                    NameEn = "Kyrenia",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new City
                {
                    Id = 4,
                    Code = "04",
                    NameTr = "Güzelyurt",
                    NameEn = "Morphou",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new City
                {
                    Id = 5,
                    Code = "05",
                    NameTr = "Lefke",
                    NameEn = "Lefka",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new City
                {
                    Id = 6,
                    Code = "06",
                    NameTr = "İskele",
                    NameEn = "Trikomo",
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                }
            );
        }

        private static void SeedDistricts(ModelBuilder modelBuilder, DateTime seedDate)
        {
            // Sample districts for Lefkoşa (Nicosia)
            modelBuilder.Entity<District>().HasData(
                new District
                {
                    Id = 1,
                    Code = "0101",
                    NameTr = "Köşklüçiftlik",
                    NameEn = "Köşklüçiftlik",
                    CityId = 1, // Lefkoşa
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 2,
                    Code = "0102",
                    NameTr = "Ortaköy",
                    NameEn = "Ortaköy",
                    CityId = 1, // Lefkoşa
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 3,
                    Code = "0103",
                    NameTr = "Gönyeli",
                    NameEn = "Gönyeli",
                    CityId = 1, // Lefkoşa
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 4,
                    Code = "0104",
                    NameTr = "Hamitköy",
                    NameEn = "Hamitköy",
                    CityId = 1, // Lefkoşa
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                // Sample districts for Gazimağusa (Famagusta)
                new District
                {
                    Id = 5,
                    Code = "0201",
                    NameTr = "Tuzla",
                    NameEn = "Tuzla",
                    CityId = 2, // Gazimağusa
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 6,
                    Code = "0202",
                    NameTr = "Salamis",
                    NameEn = "Salamis",
                    CityId = 2, // Gazimağusa
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                // Sample districts for Girne (Kyrenia)
                new District
                {
                    Id = 7,
                    Code = "0301",
                    NameTr = "Çatalköy",
                    NameEn = "Çatalköy",
                    CityId = 3, // Girne
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 8,
                    Code = "0302",
                    NameTr = "Lapta",
                    NameEn = "Lapta",
                    CityId = 3, // Girne
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                },
                new District
                {
                    Id = 9,
                    Code = "0303",
                    NameTr = "Alsancak",
                    NameEn = "Alsancak",
                    CityId = 3, // Girne
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedOn = seedDate,
                    CreatedBy = "System"
                }
            );

            // Note: Subdistricts and Villages can be added in separate migrations or as needed
            // by the tenant, as the complete hierarchical data can be quite extensive
        }
    }
}