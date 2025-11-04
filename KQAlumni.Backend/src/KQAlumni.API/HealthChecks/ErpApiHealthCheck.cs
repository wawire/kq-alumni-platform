using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KQAlumni.API.HealthChecks;

/// <summary>
/// Health check for ERP API connectivity
/// </summary>
public class ErpApiHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ErpApiHealthCheck> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ErpApiHealthCheck(
        IConfiguration configuration,
        ILogger<ErpApiHealthCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _configuration["ErpApi:BaseUrl"];
            var enableMockMode = _configuration.GetValue<bool>("ErpApi:EnableMockMode", false);

            if (enableMockMode)
            {
                return HealthCheckResult.Healthy(
                    "ERP API using mock mode",
                    data: new Dictionary<string, object>
                    {
                        ["mockMode"] = true,
                        ["baseUrl"] = baseUrl ?? "not configured"
                    });
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                return HealthCheckResult.Unhealthy(
                    "ERP API not configured",
                    data: new Dictionary<string, object>
                    {
                        ["configured"] = false
                    });
            }

            // Connectivity check with timing
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var response = await httpClient.GetAsync(baseUrl, cancellationToken);
                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    ["baseUrl"] = baseUrl,
                    ["mockMode"] = false,
                    ["statusCode"] = (int)response.StatusCode,
                    ["responseTime"] = $"{stopwatch.ElapsedMilliseconds}ms"
                };

                // Warn if response time is slow
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    _logger.LogWarning(
                        "ERP API is responding slowly: {ResponseTime}ms",
                        stopwatch.ElapsedMilliseconds);

                    return HealthCheckResult.Degraded(
                        $"ERP API is responding slowly ({stopwatch.ElapsedMilliseconds}ms)",
                        data: data);
                }

                return HealthCheckResult.Healthy(
                    $"ERP API is reachable (responded in {stopwatch.ElapsedMilliseconds}ms)",
                    data: data);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("ERP API connection timeout: {BaseUrl}", baseUrl);
                return HealthCheckResult.Degraded(
                    "ERP API connection timeout (>5s)",
                    data: new Dictionary<string, object>
                    {
                        ["baseUrl"] = baseUrl,
                        ["error"] = "Connection timeout",
                        ["timeout"] = "5000ms"
                    });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "ERP API connection failed: {Message}", ex.Message);
                return HealthCheckResult.Unhealthy(
                    $"ERP API connection failed: {ex.Message}",
                    ex,
                    data: new Dictionary<string, object>
                    {
                        ["baseUrl"] = baseUrl,
                        ["error"] = ex.Message
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERP API health check failed");
            return HealthCheckResult.Unhealthy(
                "ERP API check failed",
                exception: ex);
        }
    }
}
