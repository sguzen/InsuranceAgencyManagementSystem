
using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Identity;
using IAMS.Application.Interfaces;
using IAMS.Application.Services.Customers;
using IAMS.Domain.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.Tests.Helpers
{
    public static class TenantTestHelper
    {
        public static Mock<ITenantContextAccessor> CreateMockTenantContextAccessor(MultiTenancy.Models.Tenant tenant = null)
        {
            tenant ??= CreateTestTenant();
            var tenantContext = new TenantContext(tenant);

            var mock = new Mock<ITenantContextAccessor>();
            mock.Setup(x => x.TenantContext).Returns(tenantContext);
            mock.Setup(x => x.CurrentTenant).Returns(tenant);
            mock.Setup(x => x.CurrentTenantId).Returns(tenant.Id);
            mock.Setup(x => x.HasTenantContext).Returns(true);
            mock.Setup(x => x.GetConnectionString()).Returns(tenant.ConnectionString);
            mock.Setup(x => x.IsModuleEnabled(It.IsAny<string>())).Returns((string moduleName) =>
                tenant.IsModuleEnabled(moduleName));

            return mock;
        }

        public static MultiTenancy.Models.Tenant CreateTestTenant(int id = 1, string identifier = "test")
        {
            return new MultiTenancy.Models.Tenant
            {
                Id = id,
                Name = $"Test Tenant {id}",
                Identifier = identifier,
                ConnectionString = "Server=localhost;Database=TestTenant;Integrated Security=true;",
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                SubscriptionPlan = "Standard",
                SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                MaxUsers = 50,
                ContactEmail = $"admin@{identifier}.com",
                TimeZone = "UTC",
                Currency = "USD",
                Language = "en",
                EnabledModules = new Dictionary<string, bool>
                {
                    { "Reporting", true },
                    { "Accounting", true },
                    { "Integration", false }
                },
                Settings = new Dictionary<string, object>
                {
                    { "MaxRecordsPerPage", 50 },
                    { "DefaultCurrency", "USD" },
                    { "EnableNotifications", true }
                }
            };
        }

        public static IServiceCollection AddTestTenantServices(
            this IServiceCollection services,
            MultiTenancy.Models.Tenant testTenant = null)
        {
            testTenant ??= CreateTestTenant();

            var mockTenantContextAccessor = CreateMockTenantContextAccessor(testTenant);
            services.AddSingleton(mockTenantContextAccessor.Object);

            var mockTenantService = new Mock<ITenantService>();
            mockTenantService.Setup(x => x.GetTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(testTenant);
            mockTenantService.Setup(x => x.GetTenantByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(testTenant);
            mockTenantService.Setup(x => x.GetCurrentTenant())
                .Returns(testTenant);
            mockTenantService.Setup(x => x.GetCurrentTenantId())
                .Returns(testTenant.Id);

            services.AddSingleton(mockTenantService.Object);

            return services;
        }
    }

    // Example test
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            // Set up tenant context
            var testTenant = TenantTestHelper.CreateTestTenant();
            _tenantContextAccessorMock = TenantTestHelper.CreateMockTenantContextAccessor(testTenant);

            //_customerService = new CustomerService(
            //    _customerRepositoryMock.Object,
            //    _unitOfWorkMock.Object,
            //    _mapperMock.Object,
            //    Mock.Of<ILogger<CustomerService>>());
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var createDto = new CreateCustomerDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                TenantId = 1
            };

            _mapperMock.Setup(x => x.Map<Customer>(createDto)).Returns(customer);
            _customerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync(customer);
            _mapperMock.Setup(x => x.Map<CustomerDto>(customer))
                .Returns(new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" });

            // Act
            var result = await _customerService.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}