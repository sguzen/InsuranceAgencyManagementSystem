using System.Reflection;
using DbUp;
using DbUp.Engine.Output;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.MultiTenancy.Data
{
    /// <summary>
    /// Applies pending schema scripts to the master database (TenantDb) at application startup.
    /// </summary>
    public interface IMasterDbMigrator
    {
        /// <summary>
        /// Runs every embedded script under <c>Data/Migrations</c> that has not been journaled yet,
        /// in file-name order, each in its own transaction. Throws if a script fails so the host
        /// does not start against a schema it does not understand.
        /// </summary>
        void Migrate();
    }

    public class MasterDbMigrationOptions
    {
        public const string SectionName = "MasterDb";

        /// <summary>Set <c>MasterDb:AutoMigrate=false</c> to run the scripts out-of-band instead.</summary>
        public bool AutoMigrate { get; set; } = true;
    }

    /// <summary>
    /// DbUp-based migrator for the master database. Scripts are SQL files embedded from
    /// <c>IAMS.MultiTenancy/Data/Migrations/NNNN_Description.sql</c> and journaled in
    /// <c>dbo.__MasterDbMigrations</c>, so each script runs exactly once per database.
    /// Scripts must still be idempotent (IF NOT EXISTS guards) so that a database that was
    /// patched by hand before this mechanism existed is handled gracefully.
    /// </summary>
    public class MasterDbMigrator : IMasterDbMigrator
    {
        public const string JournalSchema = "dbo";
        public const string JournalTable = "__MasterDbMigrations";
        private const string ScriptNamespacePrefix = "IAMS.MultiTenancy.Data.Migrations.";

        private readonly TenantDbContext _context;
        private readonly MasterDbMigrationOptions _options;
        private readonly ILogger<MasterDbMigrator> _logger;

        public MasterDbMigrator(
            TenantDbContext context,
            MasterDbMigrationOptions options,
            ILogger<MasterDbMigrator> logger)
        {
            _context = context;
            _options = options;
            _logger = logger;
        }

        public void Migrate()
        {
            if (!_options.AutoMigrate)
            {
                _logger.LogInformation("Master DB auto-migration is disabled (MasterDb:AutoMigrate=false); skipping");
                return;
            }

            if (!_context.Database.IsSqlServer())
            {
                // In-memory / test providers have no schema to migrate.
                _logger.LogInformation("Master DB provider is not SQL Server ({Provider}); skipping migration scripts",
                    _context.Database.ProviderName);
                return;
            }

            var connectionString = _context.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Master database connection string is not configured; cannot run migrations");

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    typeof(MasterDbMigrator).GetTypeInfo().Assembly,
                    name => name.StartsWith(ScriptNamespacePrefix, StringComparison.Ordinal)
                         && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .JournalToSqlTable(JournalSchema, JournalTable)
                .WithTransactionPerScript()
                .LogTo(new LoggerAdapter(_logger))
                .Build();

            var pending = upgrader.GetScriptsToExecute();
            if (pending.Count == 0)
            {
                _logger.LogInformation("Master DB schema is up to date");
                return;
            }

            _logger.LogInformation("Applying {Count} master DB migration script(s): {Scripts}",
                pending.Count, string.Join(", ", pending.Select(s => s.Name.Replace(ScriptNamespacePrefix, string.Empty))));

            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException(
                    $"Master DB migration failed at script '{result.ErrorScript?.Name}': {result.Error?.Message}",
                    result.Error);
            }

            _logger.LogInformation("Master DB migration completed: {Count} script(s) applied", result.Scripts.Count());
        }

        /// <summary>Routes DbUp's log output through Microsoft.Extensions.Logging.</summary>
        private sealed class LoggerAdapter : IUpgradeLog
        {
            private readonly ILogger _logger;
            public LoggerAdapter(ILogger logger) => _logger = logger;

#pragma warning disable CA2254 // DbUp supplies composite format strings, not structured templates
            public void WriteInformation(string format, params object[] args) => _logger.LogInformation(string.Format(format, args));
            public void WriteWarning(string format, params object[] args) => _logger.LogWarning(string.Format(format, args));
            public void WriteError(string format, params object[] args) => _logger.LogError(string.Format(format, args));
#pragma warning restore CA2254
        }
    }
}
