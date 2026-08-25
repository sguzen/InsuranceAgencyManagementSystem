using FluentAssertions;
using IAMS.Infrastructure.Security;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Infrastructure
{
    public class AgencyCredentialServiceTests
    {
        private const int AgencyId = 1;
        private const int InsuranceCompanyId = 10;

        private static TenantDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TenantDbContext(options);
        }

        private static AgencyCredentialService CreateService(TenantDbContext context)
        {
            var encryption = new Mock<ICredentialEncryptionService>();
            encryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s);
            return new AgencyCredentialService(context, encryption.Object, Mock.Of<ILogger<AgencyCredentialService>>());
        }

        private static async Task SeedAsync(TenantDbContext context, string? linkAgencyCode, string? tenantExternalId,
            string dbServer = "db.example.com:3306")
        {
            context.Tenants.Add(new TenantEntity
            {
                Id = AgencyId,
                Name = "Test Agency",
                Identifier = "test-agency",
                ConnectionString = "Server=.;Database=Tenant1",
                ExternalId = tenantExternalId
            });
            context.AgencyInsuranceCompanies.Add(new AgencyInsuranceCompany
            {
                Id = 100,
                AgencyId = AgencyId,
                InsuranceCompanyId = InsuranceCompanyId,
                InsuranceCompanyName = "Test Insurer",
                InsuranceCompanyCode = "INS",
                AgencyCode = linkAgencyCode,
                DbServer = dbServer,
                DbName = "policies",
                DbUsername = "reader",
                DbPassword = "secret",
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetCredentialDetails_UsesInsurerSpecificAgencyCode_WhenSet()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: "0157", tenantExternalId: "A022");

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details.Should().NotBeNull();
            details!.AgencyCode.Should().Be("0157", "the insurer's own code must win over the tenant-level ExternalId");
        }

        [Fact]
        public async Task GetCredentialDetails_TrimsInsurerSpecificAgencyCode()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: " 0157 ", tenantExternalId: null);

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details!.AgencyCode.Should().Be("0157");
        }

        [Fact]
        public async Task GetCredentialDetails_FallsBackToTenantExternalId_WhenLinkHasNoCode()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: null, tenantExternalId: "A022");

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details!.AgencyCode.Should().Be("A022", "legacy links created before per-insurer codes must keep working");
        }

        [Fact]
        public async Task GetCredentialDetails_ReturnsNullAgencyCode_WhenNeitherIsSet()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: "", tenantExternalId: "");

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details!.AgencyCode.Should().BeNull();
        }

        [Fact]
        public async Task GetCredentialDetails_DetectsMySqlFromPort_AndDecryptsPassword()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: "0157", tenantExternalId: null, dbServer: "db.example.com:23306");

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details!.IsMySql.Should().BeTrue();
            details.Host.Should().Be("db.example.com");
            details.Port.Should().Be(23306);
            details.DatabaseName.Should().Be("policies");
            details.Username.Should().Be("reader");
            details.Password.Should().Be("secret");
        }

        [Fact]
        public async Task GetCredentialDetails_TreatsServerWithoutMySqlPortAsSqlServer()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: "0157", tenantExternalId: null, dbServer: "sqlhost");

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details!.IsMySql.Should().BeFalse();
            details.Port.Should().Be(1433);
        }

        [Fact]
        public async Task GetCredentialDetails_ReturnsNull_WhenLinkIsInactiveOrMissing()
        {
            await using var context = CreateContext();
            await SeedAsync(context, linkAgencyCode: "0157", tenantExternalId: null);
            var link = await context.AgencyInsuranceCompanies.SingleAsync();
            link.IsActive = false;
            await context.SaveChangesAsync();

            var details = await CreateService(context).GetCredentialDetailsAsync(AgencyId, InsuranceCompanyId);

            details.Should().BeNull();
        }
    }
}
