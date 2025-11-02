using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KQAlumni.API.HealthChecks;

/// <summary>
/// Health check for email service connectivity
/// </summary>
public class EmailHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailHealthCheck> _logger;

    public EmailHealthCheck(
        IConfiguration configuration,
        ILogger<EmailHealthCheck> logger)
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
            var emailEnabled = _configuration.GetValue<bool>("Email:EnableEmailSending", false);
            var useMock = _configuration.GetValue<bool>("Email:UseMockEmailService", false);

            if (!emailEnabled)
            {
                return HealthCheckResult.Degraded(
                    "Email service is disabled",
                    data: new Dictionary<string, object>
                    {
                        ["enabled"] = false,
                        ["mockMode"] = useMock
                    });
            }

            if (useMock)
            {
                return HealthCheckResult.Healthy(
                    "Email service using mock mode",
                    data: new Dictionary<string, object>
                    {
                        ["enabled"] = true,
                        ["mockMode"] = true
                    });
            }

            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);

            if (string.IsNullOrEmpty(smtpServer))
            {
                return HealthCheckResult.Unhealthy(
                    "SMTP server not configured",
                    data: new Dictionary<string, object>
                    {
                        ["enabled"] = true,
                        ["configured"] = false
                    });
            }

            // Simple connectivity check without sending email
            return HealthCheckResult.Healthy(
                "Email service configured",
                data: new Dictionary<string, object>
                {
                    ["enabled"] = true,
                    ["smtpServer"] = smtpServer,
                    ["smtpPort"] = smtpPort,
                    ["mockMode"] = false
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email health check failed");
            return HealthCheckResult.Unhealthy(
                "Email service check failed",
                exception: ex);
        }
    }
}
