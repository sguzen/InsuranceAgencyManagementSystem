using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IPolicyTypeRepository PolicyTypes { get; }
        IInsuranceCompanyRepository InsuranceCompanies { get; }
        ICustomerInsuranceCompanyRepository CustomerInsuranceCompanies { get; }
        IPolicyPaymentRepository PolicyPayments { get; }
        IPolicyClaimRepository PolicyClaims { get; }
        ICommissionRateRepository CommissionRates { get; }
        IInvoiceRepository Invoices { get; }

        ICountryRepository Countries { get; }
        IOccupationRepository Occupations { get; }
        ICityRepository Cities { get; }
        IDistrictRepository Districts { get; }
        ISubdistrictRepository Subdistricts { get; }
        IVillageRepository Villages { get; }

        // Vehicle Management
        IVehicleRepository Vehicles { get; }
        IVehicleBrandRepository VehicleBrands { get; }
        IVehicleModelRepository VehicleModels { get; }

        // Currency Management
        ICurrencyRepository Currencies { get; }
        ICurrencyExchangeRateRepository CurrencyExchangeRates { get; }

        // Import Management
        IImportConfigurationRepository ImportConfigurations { get; }
        IImportHistoryRepository ImportHistories { get; }

        // Permission Management
        IPermissionRepository Permissions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
    }
}