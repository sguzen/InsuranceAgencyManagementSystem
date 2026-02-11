using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Policy;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Service for importing policies from an external MySQL database.
    /// Uses raw ADO.NET (MySqlConnector) for read-only access — no EF Core dependency.
    /// Connection details come from ImportConfiguration per insurance company.
    ///
    /// ImportConfiguration field mapping for SourceType = DatabaseImport:
    ///   ApiBaseUrl      → MySQL Host (e.g., "77.92.6.20")
    ///   ApiUsername      → MySQL Username
    ///   ApiPassword      → MySQL Password
    ///   PoliciesEndpoint → MySQL Database name (e.g., "birim2025")
    ///   ApiKey           → Agency Code (e.g., "A022")
    ///   AdditionalSettings → JSON: { "Port": 3306, "UseSsl": false, "ConnectionTimeoutSeconds": 30, "CommandTimeoutSeconds": 120 }
    /// </summary>
    public class MySqlPolicyImportService : IMySqlPolicyImportService
    {
        private readonly ILogger<MySqlPolicyImportService> _logger;

        // Validates agency code format to prevent injection (alphanumeric only, e.g., "A022")
        private static readonly Regex AgencyCodeRegex = new(@"^[A-Za-z0-9]{1,10}$", RegexOptions.Compiled);

        public MySqlPolicyImportService(ILogger<MySqlPolicyImportService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> TestConnectionAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = BuildConnectionString(configuration);
                _logger.LogInformation("Testing MySQL connection to {Host}/{Database}",
                    configuration.ApiBaseUrl, configuration.PoliciesEndpoint);

                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 10;
                await command.ExecuteScalarAsync(cancellationToken);

                _logger.LogInformation("MySQL connection test successful for config: {ConfigName}", configuration.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MySQL connection test failed for config: {ConfigName} ({Host}/{Database})",
                    configuration.Name, configuration.ApiBaseUrl, configuration.PoliciesEndpoint);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<ImportPolicyDto>> FetchPoliciesAsync(
            ImportConfiguration configuration,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var agencyCode = configuration.ApiKey?.Trim()
                ?? throw new InvalidOperationException("Agency code (ApiKey) not configured for this import configuration");

            // Validate agency code format to prevent SQL injection
            if (!AgencyCodeRegex.IsMatch(agencyCode))
                throw new ArgumentException($"Invalid agency code format: {agencyCode}");

            var connectionString = BuildConnectionString(configuration);
            var additionalSettings = ParseAdditionalSettings(configuration.AdditionalSettings);

            _logger.LogInformation(
                "Fetching policies from MySQL: Config={ConfigName}, Agency={AgencyCode}, DateRange={StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                configuration.Name, agencyCode, startDate, endDate);

            var policies = new List<ImportPolicyDto>();

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = BuildPolicyQuery();
                command.CommandTimeout = additionalSettings.CommandTimeoutSeconds;

                // All dynamic values are parameterized
                command.Parameters.AddWithValue("@agencyCodeStart", agencyCode);
                command.Parameters.AddWithValue("@agencyCodeEnd", agencyCode);
                command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                var rowNumber = 0;
                while (await reader.ReadAsync(cancellationToken))
                {
                    rowNumber++;
                    try
                    {
                        var dto = MapRowToImportPolicyDto(reader, rowNumber);
                        policies.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to map row {RowNumber} from MySQL result", rowNumber);
                    }
                }

                _logger.LogInformation("Successfully fetched {Count} policies from MySQL (config: {ConfigName})",
                    policies.Count, configuration.Name);
                return policies;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error while fetching policies. Config={ConfigName}, ErrorCode={ErrorCode}",
                    configuration.Name, ex.ErrorCode);
                throw new InvalidOperationException($"Failed to fetch policies from MySQL database: {ex.Message}", ex);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error while fetching policies from MySQL (config: {ConfigName})",
                    configuration.Name);
                throw new InvalidOperationException($"Unexpected error during MySQL import: {ex.Message}", ex);
            }
        }

        #region Connection Builder

        /// <summary>
        /// Builds a secure MySQL connection string from ImportConfiguration fields.
        /// </summary>
        private static string BuildConnectionString(ImportConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.ApiBaseUrl))
                throw new InvalidOperationException($"MySQL host (ApiBaseUrl) not configured for '{configuration.Name}'");
            if (string.IsNullOrWhiteSpace(configuration.PoliciesEndpoint))
                throw new InvalidOperationException($"MySQL database (PoliciesEndpoint) not configured for '{configuration.Name}'");
            if (string.IsNullOrWhiteSpace(configuration.ApiUsername))
                throw new InvalidOperationException($"MySQL username (ApiUsername) not configured for '{configuration.Name}'");

            var additionalSettings = ParseAdditionalSettings(configuration.AdditionalSettings);

            var builder = new MySqlConnectionStringBuilder
            {
                Server = configuration.ApiBaseUrl.Trim(),
                Port = (uint)additionalSettings.Port,
                Database = configuration.PoliciesEndpoint.Trim(),
                UserID = configuration.ApiUsername.Trim(),
                Password = configuration.ApiPassword ?? string.Empty,
                ConnectionTimeout = (uint)additionalSettings.ConnectionTimeoutSeconds,
                DefaultCommandTimeout = (uint)additionalSettings.CommandTimeoutSeconds,
                // Security settings
                SslMode = additionalSettings.UseSsl ? MySqlSslMode.Required : MySqlSslMode.Preferred,
                AllowUserVariables = false,
                AllowLoadLocalInfile = false,
                // Performance settings
                ConnectionReset = true,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 5,
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Parses the AdditionalSettings JSON field for MySQL-specific options.
        /// Returns defaults if the field is empty or invalid.
        /// </summary>
        private static MySqlAdditionalSettings ParseAdditionalSettings(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new MySqlAdditionalSettings();

            try
            {
                return JsonSerializer.Deserialize<MySqlAdditionalSettings>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new MySqlAdditionalSettings();
            }
            catch
            {
                return new MySqlAdditionalSettings();
            }
        }

        private class MySqlAdditionalSettings
        {
            public int Port { get; set; } = 3306;
            public bool UseSsl { get; set; } = false;
            public int ConnectionTimeoutSeconds { get; set; } = 30;
            public int CommandTimeoutSeconds { get; set; } = 120;
        }

        #endregion

        #region Row Mapping

        /// <summary>
        /// Maps a single MySQL data row to an ImportPolicyDto
        /// </summary>
        private static ImportPolicyDto MapRowToImportPolicyDto(MySqlDataReader reader, int rowNumber)
        {
            var zytip = GetString(reader, "zytip");
            var dvcns = GetString(reader, "dvcns");

            return new ImportPolicyDto
            {
                RowNumber = rowNumber,

                // Policy identification
                BranchCode = GetString(reader, "brkod"),
                PolicyNumber = GetString(reader, "polno"),
                TecditNumber = GetString(reader, "tecdt"),
                InnerCode = GetString(reader, "zeyno") ?? "000",
                StateType = ParseStateType(zytip),
                PolicyTypeCode = GetString(reader, "brkod"),

                // Insurance company
                InsuranceCompanyName = GetString(reader, "kisaAD"),
                InsuranceCompanyCode = GetString(reader, "ackod"),

                // Customer information
                CustomerName = GetString(reader, "sigor"),
                CustomerCountryCode = GetString(reader, "ulkod"),
                CustomerIdType = GetString(reader, "kmtur"),
                CustomerIdentifier = GetString(reader, "kimno"),

                // Dates
                StartDate = GetNullableDateTime(reader, "bstar"),
                EndDate = GetNullableDateTime(reader, "bttar"),

                // Financial - use yekun (total) as PremiumAmount, matching Excel "Toplam" mapping
                PremiumAmount = GetDecimal(reader, "yekun"),
                CommissionAmount = GetNullableDecimal(reader, "xkoms"),
                CurrencyCode = MapCurrencyCode(dvcns),

                // Driver information
                DriverAge = GetNullableInt(reader, "sgyas"),
                DriverTypeText = GetString(reader, "suren"),

                // Marketer
                Marketer = GetString(reader, "pazADI"),

                // Vehicle information
                PlateNumber = GetString(reader, "plaka"),
                VehicleBrand = GetString(reader, "marka"),
                VehicleModel = GetString(reader, "model"),
                VehicleYear = GetNullableInt(reader, "imyil"),

                // Build notes from additional fields
                Notes = BuildNotes(
                    GetString(reader, "rehin"),
                    GetString(reader, "menfaat"),
                    GetString(reader, "subADI"),
                    GetNullableDecimal(reader, "xprim"),
                    GetNullableDecimal(reader, "xharc"),
                    GetNullableDecimal(reader, "xasis"),
                    GetNullableDecimal(reader, "xbsiv"),
                    GetNullableDecimal(reader, "xshrc"),
                    GetNullableDecimal(reader, "xgfon"),
                    GetNullableDecimal(reader, "xdpul"),
                    GetNullableDecimal(reader, "xkymet"),
                    GetNullableInt(reader, "xadths"),
                    GetNullableDecimal(reader, "xmuahs"),
                    GetNullableDecimal(reader, "xodnhs")),

                // Default status
                Status = PolicyStatus.Active
            };
        }

        private static StateType ParseStateType(string? zytip)
        {
            return zytip?.Trim().ToUpperInvariant() switch
            {
                "P" => StateType.YeniPolice,
                "T" => StateType.Tecdit,
                "V" => StateType.Ilave,
                "R" => StateType.Iade,
                "X" => StateType.Iptal,
                "Y" => StateType.Yeniden,
                _ => StateType.YeniPolice
            };
        }

        private static string MapCurrencyCode(string? dvcns)
        {
            if (string.IsNullOrWhiteSpace(dvcns))
                return "TRY";

            return dvcns.Trim().ToUpperInvariant() switch
            {
                "TL" or "TRY" => "TRY",
                "USD" or "US" or "$" => "USD",
                "EUR" or "EU" => "EUR",
                "GBP" or "STG" => "GBP",
                _ => dvcns.Trim()
            };
        }

        private static string? BuildNotes(
            string? rehin, string? menfaat, string? subAdi,
            decimal? netPrim, decimal? harc, decimal? asis, decimal? bsiv,
            decimal? shrc, decimal? gfon, decimal? dpul,
            decimal? kymet, int? adths, decimal? muahs, decimal? odnhs)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(rehin))
                parts.Add($"Rehin: {rehin}");
            if (!string.IsNullOrWhiteSpace(menfaat))
                parts.Add($"Menfaat: {menfaat}");
            if (!string.IsNullOrWhiteSpace(subAdi))
                parts.Add($"Alt Kod: {subAdi}");
            if (kymet.HasValue && kymet.Value != 0)
                parts.Add($"Bedel: {kymet.Value:N2}");
            if (netPrim.HasValue && netPrim.Value != 0)
                parts.Add($"Net Prim: {netPrim.Value:N2}");
            if (harc.HasValue && harc.Value != 0)
                parts.Add($"Harç: {harc.Value:N2}");
            if (asis.HasValue && asis.Value != 0)
                parts.Add($"Asistans: {asis.Value:N2}");
            if (bsiv.HasValue && bsiv.Value != 0)
                parts.Add($"BSMV: {bsiv.Value:N2}");
            if (shrc.HasValue && shrc.Value != 0)
                parts.Add($"SSH: {shrc.Value:N2}");
            if (gfon.HasValue && gfon.Value != 0)
                parts.Add($"GF: {gfon.Value:N2}");
            if (dpul.HasValue && dpul.Value != 0)
                parts.Add($"DP: {dpul.Value:N2}");
            if (adths.HasValue && adths.Value > 0)
                parts.Add($"Hasar Adedi: {adths.Value}");
            if (muahs.HasValue && muahs.Value != 0)
                parts.Add($"Muallak Hasar: {muahs.Value:N2}");
            if (odnhs.HasValue && odnhs.Value != 0)
                parts.Add($"Ödenen Hasar: {odnhs.Value:N2}");

            return parts.Count > 0 ? string.Join(" | ", parts) : null;
        }

        #endregion

        #region Data Reader Helpers

        private static string? GetString(MySqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal)?.Trim();
            }
            catch { return null; }
        }

        private static decimal GetDecimal(MySqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
            }
            catch { return 0m; }
        }

        private static decimal? GetNullableDecimal(MySqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
            }
            catch { return null; }
        }

        private static int? GetNullableInt(MySqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch { return null; }
        }

        private static DateTime? GetNullableDateTime(MySqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return null;

                var value = reader.GetDateTime(ordinal);
                // Filter out MySQL zero dates
                return value.Year < 1900 ? null : value;
            }
            catch { return null; }
        }

        #endregion

        #region Query Builder

        /// <summary>
        /// Builds the parameterized policy query across all 5 policy tables.
        /// Parameters: @agencyCodeStart, @agencyCodeEnd, @startDate, @endDate
        /// </summary>
        private static string BuildPolicyQuery()
        {
            return @"
SELECT kisaAD, a.brkod, a.ackod, polno, tecdt, zytip, zeyno, tarih, bstar, bttar, sigor, dvcns, dvkur, a.sbkod, a.pzkod, subkod.adi AS subADI, pzadi AS pazADI,
       a.ulkod, a.kmtur, a.kimno, rehin, IF((k.d_tarihi IS NULL) OR (a.kmtur NOT IN ('KN','PN')), 0, YEAR(a.bstar)-YEAR(k.d_tarihi)) AS sgyas,
       '' AS arkod, '' AS plaka, '' AS marka, '' AS model, 0 AS imyil, 0 AS atguc, '' AS suren,
       IF(a.brkod='01', binbd+esybd+dembd+makbd+emtbd+hvzbd, IF(a.brkod='02', binbd+dembd+makbd+elkbd+emtbd+desbd+hvzbd, binbd+esybd+desbd+elkbd+makbd+cevbd+hvzbd)) AS xkymet,
       (yanpr+ektpr+deppr)*IF(zytip IN ('R','X'),-1,1) AS xprim,
       idhrc*IF(zytip IN ('R','X'),-1,1) AS xharc,
       asspr*IF(zytip IN ('R','X'),-1,1) AS xasis,
       Round((yanpr+ektpr+deppr+idhrc+asspr)*bsiyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xbsiv,
       0 AS xshrc,
       0 AS xgfon,
       0 AS xdpul,
      (yanpr+ektpr+deppr+idhrc+asspr+Round((yanpr+ektpr+deppr+idhrc+asspr)*bsiyz/100,2))*IF(zytip IN ('R','X'),-1,1) AS yekun,
      (ykmmk+ekmmk+dkmmk+hkmmk+akmmk)*IF(zytip IN ('R','X'),-1,1) AS xkoms,
       IFNULL((SELECT COUNT(*) FROM hasgen d WHERE d.brkod=a.brkod AND d.ackod=a.ackod AND d.polno=a.polno AND d.tecdt=a.tecdt AND a.zeyno='000'), 0) AS xadths,
       GREATEST(0, IFNULL((SELECT SUM(thmin-IFNULL((SELECT SUM(d.moden) FROM odnhas d WHERE d.brkod=h.brkod AND d.dosno=h.dosno AND d.tlpno=hastlp.tlpno),0))
                           FROM hasgen h LEFT OUTER JOIN hastlp ON h.brkod=hastlp.brkod AND h.dosno=hastlp.dosno
                           WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt AND (hastlp.kptar=@endDate OR (hastlp.kptar IS NULL))), 0) ) AS xmuahs,
       IFNULL((SELECT SUM(moden) FROM hasgen h LEFT OUTER JOIN odnhas ON h.brkod=odnhas.brkod AND h.dosno=odnhas.dosno
               WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt),0) AS xodnhs,
       '' AS menfaat
FROM yanpol a
LEFT OUTER JOIN pargen p ON p.brkod=a.brkod
LEFT OUTER JOIN kimlik k ON k.ulkod=a.ulkod AND k.kmtur=a.kmtur AND k.kimno=a.kimno
LEFT OUTER JOIN subkod ON subkod.kod=a.sbkod
LEFT OUTER JOIN pazkod ON pazkod.ackod=a.ackod AND pazkod.pzkod=a.pzkod
WHERE a.onayOK=1 AND a.ackod >= @agencyCodeStart AND a.ackod <= @agencyCodeEnd AND tarih >= @startDate AND tarih <= @endDate

UNION

SELECT kisaAD, a.brkod, a.ackod, polno, tecdt, zytip, zeyno, tarih, bstar, bttar, sigor, dvcns, dvkur, a.sbkod, a.pzkod, subkod.adi AS subADI, pzadi AS pazADI,
       a.ulkod, a.kmtur, a.kimno, rehin, IF((k.d_tarihi IS NULL) OR (a.kmtur NOT IN ('KN','PN')), 0, YEAR(a.bstar)-YEAR(k.d_tarihi)) AS sgyas,
       '' AS arkod, '' AS plaka, '' AS marka, '' AS model, 0 AS imyil, 0 AS atguc, '' AS suren,
       kymet AS xkymet, netpr*IF(zytip IN ('R','X'),-1,1) AS xprim, idhrc*IF(zytip IN ('R','X'),-1,1) AS xharc, asspr*IF(zytip IN ('R','X'),-1,1) AS xasis,
       Round((netpr+idhrc+asspr)*bsiyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xbsiv, 0 AS xshrc, 0 AS xgfon, 0 AS xdpul,
      (netpr+idhrc+asspr+Round((netpr+idhrc+asspr)*bsiyz/100,2))*IF(zytip IN ('R','X'),-1,1) AS yekun,
      (pkmmk+hkmmk+akmmk)*IF(zytip IN ('R','X'),-1,1) AS xkoms,
       IFNULL((SELECT COUNT(*) FROM hasgen d WHERE d.brkod=a.brkod AND d.ackod=a.ackod AND d.polno=a.polno AND d.tecdt=a.tecdt AND a.zeyno='000'), 0) AS xadths,
       GREATEST(0, IFNULL((SELECT SUM(thmin-IFNULL((SELECT SUM(d.moden) FROM odnhas d WHERE d.brkod=h.brkod AND d.dosno=h.dosno AND d.tlpno=hastlp.tlpno),0))
                           FROM hasgen h LEFT OUTER JOIN hastlp ON h.brkod=hastlp.brkod AND h.dosno=hastlp.dosno
                           WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt AND (hastlp.kptar=@endDate OR (hastlp.kptar IS NULL))), 0) ) AS xmuahs,
       IFNULL((SELECT SUM(moden) FROM hasgen h LEFT OUTER JOIN odnhas ON h.brkod=odnhas.brkod AND h.dosno=odnhas.dosno
               WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt),0) AS xodnhs,
       '' AS menfaat
FROM nakpol a
LEFT OUTER JOIN pargen p ON p.brkod=a.brkod
LEFT OUTER JOIN kimlik k ON k.ulkod=a.ulkod AND k.kmtur=a.kmtur AND k.kimno=a.kimno
LEFT OUTER JOIN subkod ON subkod.kod=a.sbkod
LEFT OUTER JOIN pazkod ON pazkod.ackod=a.ackod AND pazkod.pzkod=a.pzkod
WHERE a.onayOK=1 AND a.brkod<>'NC' AND a.ackod >= @agencyCodeStart AND a.ackod <= @agencyCodeEnd AND tarih >= @startDate AND tarih <= @endDate

UNION

SELECT kisaAD, a.brkod, a.ackod, polno, tecdt, zytip, zeyno, tarih, bstar, bttar, sigor, dvcns, dvkur, a.sbkod, a.pzkod, subkod.adi AS subADI, pzadi AS pazADI,
       a.ulkod, a.kmtur, a.kimno, rehin, IF((k.d_tarihi IS NULL) OR (a.kmtur NOT IN ('KN','PN')), 0, YEAR(a.bstar)-YEAR(k.d_tarihi)) AS sgyas,
       arkod, plaka, marka.adi AS marka, model.adi AS model, imyil, atguc, IF(bazxx=2,'NAME','ANY') AS suren,
       (kasbd+emtbd+radbd+jntbd) AS xkymet,
       (zprim+kprim+dprim+fprim+sprim)*IF(zytip IN ('R','X'),-1,1) AS xprim, idhrc*IF(zytip IN ('R','X'),-1,1) AS xharc, aprim*IF(zytip IN ('R','X'),-1,1) AS xasis,
       Round((zprim+kprim+dprim+fprim+sprim+aprim+idhrc)*bsiyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xbsiv,
       Round((zprim+kprim+dprim+fprim+sprim+aprim+idhrc)*sshyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xshrc,
       gfnmk*IF(zytip IN ('R','X'),-1,1) AS xgfon, dmpul*IF(zytip IN ('R','X'),-1,1) AS xdpul,
      (zprim+kprim+dprim+fprim+sprim+aprim+idhrc+gfnmk+dmpul+Round((zprim+kprim+dprim+fprim+sprim+aprim+idhrc)*bsiyz/100,2)+Round((zprim+kprim+dprim+fprim+sprim+aprim+idhrc)*sshyz/100,2))*IF(zytip IN ('R','X'),-1,1) AS yekun,
      (zkmmk+kkmmk+dkmmk+fkmmk+skmmk+akmmk+hkmmk)*IF(zytip IN ('R','X'),-1,1) AS xkoms,
       IFNULL((SELECT COUNT(*) FROM hasgen d WHERE d.brkod=a.brkod AND d.ackod=a.ackod AND d.polno=a.polno AND d.tecdt=a.tecdt AND a.zeyno='000'), 0) AS xadths,
       GREATEST(0, IFNULL((SELECT SUM(thmin-IFNULL((SELECT SUM(d.moden) FROM odnhas d WHERE d.brkod=h.brkod AND d.dosno=h.dosno AND d.tlpno=hastlp.tlpno),0))
                           FROM hasgen h LEFT OUTER JOIN hastlp ON h.brkod=hastlp.brkod AND h.dosno=hastlp.dosno
                           WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt AND (hastlp.kptar=@endDate OR (hastlp.kptar IS NULL))), 0) ) AS xmuahs,
       IFNULL((SELECT SUM(moden) FROM hasgen h LEFT OUTER JOIN odnhas ON h.brkod=odnhas.brkod AND h.dosno=odnhas.dosno
               WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt),0) AS xodnhs,
       '' AS menfaat
FROM motpol a
LEFT OUTER JOIN pargen p ON p.brkod=a.brkod
LEFT OUTER JOIN kimlik k ON k.ulkod=a.ulkod AND k.kmtur=a.kmtur AND k.kimno=a.kimno
LEFT OUTER JOIN subkod ON subkod.kod=a.sbkod
LEFT OUTER JOIN pazkod ON pazkod.ackod=a.ackod AND pazkod.pzkod=a.pzkod
LEFT OUTER JOIN model ON model.kod=a.modelID
LEFT OUTER JOIN marka ON marka.kod=model.markaKOD
WHERE a.onayOK=1 AND a.brkod<>'TC' AND a.ackod >= @agencyCodeStart AND a.ackod <= @agencyCodeEnd AND tarih >= @startDate AND tarih <= @endDate

UNION

SELECT kisaAD, a.brkod, a.ackod, polno, tecdt, zytip, zeyno, tarih, bstar, bttar, sigor, dvcns, dvkur, a.sbkod, a.pzkod, subkod.adi AS subADI, pzadi AS pazADI,
       a.ulkod, a.kmtur, a.kimno, '' AS rehin, IF((k.d_tarihi IS NULL) OR (a.kmtur NOT IN ('KN','PN')), 0, YEAR(a.bstar)-YEAR(k.d_tarihi)) AS sgyas,
       '' AS arkod, '' AS plaka, '' AS marka, '' AS model, 0 AS imyil, 0 AS atguc, '' AS suren,
       IF(a.brkod='19', bed02, IF(a.brkod='35', bed03+bed04, bed01)) AS xkymet,
       (netpr+sagpr)*IF(zytip IN ('R','X'),-1,1) AS xprim, idhrc*IF(zytip IN ('R','X'),-1,1) AS xharc, asspr*IF(zytip IN ('R','X'),-1,1) AS xasis,
       Round((netpr+sagpr+idhrc+asspr)*bsiyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xbsiv, 0 AS xshrc, 0 AS xgfon, 0 AS xdpul,
      (netpr+sagpr+idhrc+asspr+Round((netpr+sagpr+idhrc+asspr)*bsiyz/100,2))*IF(zytip IN ('R','X'),-1,1) AS yekun,
      (pkmmk+skmmk+hkmmk+akmmk)*IF(zytip IN ('R','X'),-1,1) AS xkoms,
       IFNULL((SELECT COUNT(*) FROM hasgen d WHERE d.brkod=a.brkod AND d.ackod=a.ackod AND d.polno=a.polno AND d.tecdt=a.tecdt AND a.zeyno='000'), 0) AS xadths,
       GREATEST(0, IFNULL((SELECT SUM(thmin-IFNULL((SELECT SUM(d.moden) FROM odnhas d WHERE d.brkod=h.brkod AND d.dosno=h.dosno AND d.tlpno=hastlp.tlpno),0))
                           FROM hasgen h LEFT OUTER JOIN hastlp ON h.brkod=hastlp.brkod AND h.dosno=hastlp.dosno
                           WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt AND (hastlp.kptar=@endDate OR (hastlp.kptar IS NULL))), 0) ) AS xmuahs,
       IFNULL((SELECT SUM(moden) FROM hasgen h LEFT OUTER JOIN odnhas ON h.brkod=odnhas.brkod AND h.dosno=odnhas.dosno
               WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt),0) AS xodnhs,
       IF(a.brkod IN ('19','36','45','48'), CONCAT(ack01,' ',ack02), '') AS menfaat
FROM dgrpol a
LEFT OUTER JOIN pargen p ON p.brkod=a.brkod
LEFT OUTER JOIN kimlik k ON k.ulkod=a.ulkod AND k.kmtur=a.kmtur AND k.kimno=a.kimno
LEFT OUTER JOIN subkod ON subkod.kod=a.sbkod
LEFT OUTER JOIN pazkod ON pazkod.ackod=a.ackod AND pazkod.pzkod=a.pzkod
WHERE a.onayOK=1 AND a.ackod >= @agencyCodeStart AND a.ackod <= @agencyCodeEnd AND tarih >= @startDate AND tarih <= @endDate

UNION

SELECT kisaAD, a.brkod, a.ackod, polno, tecdt, zytip, zeyno, tarih, bstar, bttar, sigor, dvcns, dvkur, a.sbkod, a.pzkod, subkod.adi AS subADI, pzadi AS pazADI,
       a.ulkod, a.kmtur, a.kimno, '' AS rehin, IF((k.d_tarihi IS NULL) OR (a.kmtur NOT IN ('KN','PN')), 0, YEAR(a.bstar)-YEAR(k.d_tarihi)) AS sgyas,
       '' AS arkod, '' AS plaka, '' AS marka, '' AS model, 0 AS imyil, 0 AS atguc, '' AS suren,
       kymet AS xkymet, (netpr+deppr)*IF(zytip IN ('R','X'),-1,1) AS xprim, idhrc*IF(zytip IN ('R','X'),-1,1) AS xharc, asspr*IF(zytip IN ('R','X'),-1,1) AS xasis,
       Round((netpr+deppr+idhrc+asspr)*bsiyz/100,2)*IF(zytip IN ('R','X'),-1,1) AS xbsiv, 0 AS xshrc, 0 AS xgfon, 0 AS xdpul,
      (netpr+deppr+idhrc+asspr+Round((netpr+deppr+idhrc+asspr)*bsiyz/100,2))*IF(zytip IN ('R','X'),-1,1) AS yekun,
      (pkmmk+dkmmk+hkmmk)*IF(zytip IN ('R','X'),-1,1) AS xkoms,
       IFNULL((SELECT COUNT(*) FROM hasgen d WHERE d.brkod=a.brkod AND d.ackod=a.ackod AND d.polno=a.polno AND d.tecdt=a.tecdt AND a.zeyno='000'), 0) AS xadths,
       GREATEST(0, IFNULL((SELECT SUM(thmin-IFNULL((SELECT SUM(d.moden) FROM odnhas d WHERE d.brkod=h.brkod AND d.dosno=h.dosno AND d.tlpno=hastlp.tlpno),0))
                           FROM hasgen h LEFT OUTER JOIN hastlp ON h.brkod=hastlp.brkod AND h.dosno=hastlp.dosno
                           WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt AND (hastlp.kptar=@endDate OR (hastlp.kptar IS NULL))), 0) ) AS xmuahs,
       IFNULL((SELECT SUM(moden) FROM hasgen h LEFT OUTER JOIN odnhas ON h.brkod=odnhas.brkod AND h.dosno=odnhas.dosno
               WHERE h.brkod=a.brkod AND h.ackod=a.ackod AND h.polno=a.polno AND h.tecdt=a.tecdt),0) AS xodnhs,
       '' AS menfaat
FROM makpol a
LEFT OUTER JOIN pargen p ON p.brkod=a.brkod
LEFT OUTER JOIN kimlik k ON k.ulkod=a.ulkod AND k.kmtur=a.kmtur AND k.kimno=a.kimno
LEFT OUTER JOIN subkod ON subkod.kod=a.sbkod
LEFT OUTER JOIN pazkod ON pazkod.ackod=a.ackod AND pazkod.pzkod=a.pzkod
WHERE a.onayOK=1 AND a.ackod >= @agencyCodeStart AND a.ackod <= @agencyCodeEnd AND tarih >= @startDate AND tarih <= @endDate

ORDER BY ackod,brkod,zytip,polno,tecdt,zeyno";
        }

        #endregion
    }
}
