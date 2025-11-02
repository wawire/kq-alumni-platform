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

            // Simple connectivity check
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var response = await httpClient.GetAsync(baseUrl, cancellationToken);

                return HealthCheckResult.Healthy(
                    "ERP API is reachable",
                    data: new Dictionary<string, object>
                    {
                        ["baseUrl"] = baseUrl,
                        ["mockMode"] = false,
                        ["statusCode"] = (int)response.StatusCode
                    });
            }
            catch (TaskCanceledException)
            {
                return HealthCheckResult.Degraded(
                    "ERP API timeout",
                    data: new Dictionary<string, object>
                    {
                        ["baseUrl"] = baseUrl,
                        ["error"] = "Connection timeout"
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
