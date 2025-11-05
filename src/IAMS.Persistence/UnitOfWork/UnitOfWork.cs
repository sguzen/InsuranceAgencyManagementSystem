using IAMS.Application.Interfaces.Repositories;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IAMS.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private ICustomerRepository? _customers;
        private IPolicyRepository? _policies;
        private IPolicyTypeRepository? _policyTypes;
        private IInsuranceCompanyRepository? _insuranceCompanies;
        private ICustomerInsuranceCompanyRepository? _customerInsuranceCompanies;
        private IPolicyPaymentRepository? _policyPayments;
        private IPolicyClaimRepository? _policyClaims;
        private ICommissionRateRepository? _commissionRates;

        private ICountryRepository? _countries;
        private IOccupationRepository? _occupations;
        private ICityRepository? _cities;
        private IDistrictRepository? _districts;
        private ISubdistrictRepository? _subdistricts;
        private IVillageRepository? _villages;
        private IVehicleRepository? _vehicles;
        private IVehicleBrandRepository? _vehicleBrands;
        private IVehicleModelRepository? _vehicleModels;
        private ICurrencyRepository? _currencies;
        private ICurrencyExchangeRateRepository? _currencyExchangeRates;


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICustomerRepository Customers =>
            _customers ??= new CustomerRepository(_context);

        public IPolicyRepository Policies =>
            _policies ??= new PolicyRepository(_context);

        public IPolicyTypeRepository PolicyTypes =>
            _policyTypes ??= new PolicyTypeRepository(_context);

        public IInsuranceCompanyRepository InsuranceCompanies =>
            _insuranceCompanies ??= new InsuranceCompanyRepository(_context);

        public ICustomerInsuranceCompanyRepository CustomerInsuranceCompanies =>
            _customerInsuranceCompanies ??= new CustomerInsuranceCompanyRepository(_context);

        public IPolicyPaymentRepository PolicyPayments =>
            _policyPayments ??= new PolicyPaymentRepository(_context);

        public IPolicyClaimRepository PolicyClaims =>
            _policyClaims ??= new PolicyClaimRepository(_context);

        public ICommissionRateRepository CommissionRates =>
            _commissionRates ??= new CommissionRateRepository(_context);

        public ICountryRepository Countries =>
            _countries ??= new CountryRepository(_context);

        public IOccupationRepository Occupations =>
            _occupations ??= new OccupationRepository(_context);

        public ICityRepository Cities =>
            _cities ??= new CityRepository(_context);

        public IDistrictRepository Districts =>
            _districts ??= new DistrictRepository(_context);

        public ISubdistrictRepository Subdistricts =>
            _subdistricts ??= new SubdistrictRepository(_context);

        public IVillageRepository Villages =>
            _villages ??= new VillageRepository(_context);

        public IVehicleRepository Vehicles => 
            _vehicles ??= new VehicleRepository(_context);

        public IVehicleBrandRepository VehicleBrands => 
            _vehicleBrands ??= new VehicleBrandRepository(_context);

        public IVehicleModelRepository VehicleModels => 
            _vehicleModels ??= new VehicleModelRepository(_context);

        public ICurrencyRepository Currencies => 
            _currencies ??= new CurrencyRepository(_context);

        public ICurrencyExchangeRateRepository CurrencyExchangeRates 
            => _currencyExchangeRates ??= new CurrencyExchangeRateRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new InvalidOperationException("A concurrency conflict occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}