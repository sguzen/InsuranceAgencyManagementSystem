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

        // Customer Payment Management
        ICustomerPaymentRepository CustomerPayments { get; }

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

        /// <summary>
        /// Detach all tracked entities from the change tracker to reset dirty state
        /// </summary>
        void DetachAllEntities();

        /// <summary>
        /// Attach an entity back to the change tracker as Modified
        /// </summary>
        void AttachEntity<T>(T entity) where T : class;
    }
}