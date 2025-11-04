using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KQAlumni.API.HealthChecks;

/// <summary>
/// Comprehensive SQL Server health check with connection testing and query execution
/// </summary>
public class SqlServerHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlServerHealthCheck> _logger;

    public SqlServerHealthCheck(
        IConfiguration configuration,
        ILogger<SqlServerHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy(
                    "Database connection string is not configured",
                    data: new Dictionary<string, object>
                    {
                        ["configured"] = false
                    });
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                // Test query to verify database is responsive
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    command.CommandTimeout = 5; // 5 second timeout
                    await command.ExecuteScalarAsync(cancellationToken);
                }

                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    ["server"] = ExtractServerFromConnectionString(connectionString),
                    ["database"] = ExtractDatabaseFromConnectionString(connectionString),
                    ["responseTime"] = $"{stopwatch.ElapsedMilliseconds}ms",
                    ["status"] = "Connected"
                };

                // Warn if response time is slow
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "SQL Server is responding slowly: {ResponseTime}ms",
                        stopwatch.ElapsedMilliseconds);

                    return HealthCheckResult.Degraded(
                        $"Database is responding slowly ({stopwatch.ElapsedMilliseconds}ms)",
                        data: data);
                }

                return HealthCheckResult.Healthy(
                    $"Database is healthy (responded in {stopwatch.ElapsedMilliseconds}ms)",
                    data: data);
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Server health check failed: {Message}", ex.Message);

            return HealthCheckResult.Unhealthy(
                $"Database connection failed: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    ["errorNumber"] = ex.Number,
                    ["errorClass"] = ex.Class,
                    ["server"] = ex.Server ?? "Unknown"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SQL Server health check: {Message}", ex.Message);

            return HealthCheckResult.Unhealthy(
                $"Unexpected error: {ex.Message}",
                ex);
        }
    }

    private string ExtractServerFromConnectionString(string connString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connString, @"Server=([^;]+)");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }

    private string ExtractDatabaseFromConnectionString(string connString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connString, @"Database=([^;]+)");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }
}
