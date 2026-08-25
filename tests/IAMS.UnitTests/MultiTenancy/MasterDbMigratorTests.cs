using FluentAssertions;
using IAMS.MultiTenancy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.MultiTenancy
{
    public class MasterDbMigratorTests
    {
        private static TenantDbContext CreateInMemoryContext() =>
            new(new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public void Migrate_SkipsWhenProviderIsNotSqlServer()
        {
            using var context = CreateInMemoryContext();
            var migrator = new MasterDbMigrator(context, new MasterDbMigrationOptions(), Mock.Of<ILogger<MasterDbMigrator>>());

            var act = () => migrator.Migrate();

            act.Should().NotThrow("test hosts use the in-memory provider and must not try to reach SQL Server");
        }

        [Fact]
        public void Migrate_SkipsWhenAutoMigrateIsDisabled()
        {
            using var context = CreateInMemoryContext();
            var options = new MasterDbMigrationOptions { AutoMigrate = false };
            var migrator = new MasterDbMigrator(context, options, Mock.Of<ILogger<MasterDbMigrator>>());

            var act = () => migrator.Migrate();

            act.Should().NotThrow();
        }

        [Fact]
        public void MigrationScripts_AreEmbeddedAndNumbered()
        {
            var names = typeof(MasterDbMigrator).Assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith("IAMS.MultiTenancy.Data.Migrations.", StringComparison.Ordinal))
                .ToList();

            names.Should().NotBeEmpty("scripts under Data/Migrations must be embedded for the migrator to find them");
            names.Should().OnlyContain(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));
            names.Should().Contain(n => n.Contains("0005_AddAgencyCodeToAgencyInsuranceCompanies"));
            names.Select(n => n.Substring("IAMS.MultiTenancy.Data.Migrations.".Length))
                .Should().OnlyContain(n => n.Length > 5 && n.Substring(0, 4).All(char.IsDigit) && n[4] == '_',
                    "scripts are applied in name order, so they must be prefixed NNNN_");
        }
    }
}
