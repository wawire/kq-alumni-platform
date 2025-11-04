using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using KQAlumni.Core.Entities;

namespace KQAlumni.API.HealthChecks;

/// <summary>
/// SMTP server connectivity health check
/// Tests if SMTP server is reachable and accepting connections
/// </summary>
public class SmtpHealthCheck : IHealthCheck
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpHealthCheck> _logger;

    public SmtpHealthCheck(
        IOptions<EmailSettings> emailSettings,
        ILogger<SmtpHealthCheck> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If mock mode is enabled, return degraded status
            if (_emailSettings.UseMockEmailService)
            {
                return HealthCheckResult.Degraded(
                    "SMTP Health Check: Mock mode enabled (emails will be logged, not sent)",
                    data: new Dictionary<string, object>
                    {
                        ["mockMode"] = true,
                        ["smtpServer"] = _emailSettings.SmtpServer ?? "Not configured",
                        ["smtpPort"] = _emailSettings.SmtpPort
                    });
            }

            // If email sending is disabled, return degraded status
            if (!_emailSettings.EnableEmailSending)
            {
                return HealthCheckResult.Degraded(
                    "SMTP Health Check: Email sending is disabled",
                    data: new Dictionary<string, object>
                    {
                        ["enabled"] = false,
                        ["smtpServer"] = _emailSettings.SmtpServer ?? "Not configured"
                    });
            }

            // Check if SMTP server is configured
            if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
            {
                return HealthCheckResult.Unhealthy(
                    "SMTP server is not configured",
                    data: new Dictionary<string, object>
                    {
                        ["configured"] = false
                    });
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Test TCP connection to SMTP server
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                var timeoutTask = Task.Delay(5000, cancellationToken); // 5 second timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger.LogWarning(
                        "SMTP server connection timeout: {Server}:{Port}",
                        _emailSettings.SmtpServer,
                        _emailSettings.SmtpPort);

                    return HealthCheckResult.Degraded(
                        $"SMTP server connection timeout ({_emailSettings.SmtpServer}:{_emailSettings.SmtpPort})",
                        data: new Dictionary<string, object>
                        {
                            ["smtpServer"] = _emailSettings.SmtpServer,
                            ["smtpPort"] = _emailSettings.SmtpPort,
                            ["timeout"] = "5000ms",
                            ["status"] = "Timeout"
                        });
                }

                await connectTask; // Ensure we await the actual connection
                stopwatch.Stop();

                return HealthCheckResult.Healthy(
                    $"SMTP server is reachable (connected in {stopwatch.ElapsedMilliseconds}ms)",
                    data: new Dictionary<string, object>
                    {
                        ["smtpServer"] = _emailSettings.SmtpServer,
                        ["smtpPort"] = _emailSettings.SmtpPort,
                        ["sslEnabled"] = _emailSettings.EnableSsl,
                        ["responseTime"] = $"{stopwatch.ElapsedMilliseconds}ms",
                        ["status"] = "Connected"
                    });
            }
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex,
                "SMTP server connection failed: {Server}:{Port} - {Message}",
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                ex.Message);

            return HealthCheckResult.Unhealthy(
                $"SMTP server connection failed: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    ["smtpServer"] = _emailSettings.SmtpServer ?? "Unknown",
                    ["smtpPort"] = _emailSettings.SmtpPort,
                    ["errorCode"] = ex.ErrorCode,
                    ["socketError"] = ex.SocketErrorCode.ToString()
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SMTP health check: {Message}", ex.Message);

            return HealthCheckResult.Unhealthy(
                $"Unexpected error: {ex.Message}",
                ex,
                data: new Dictionary<string, object>
                {
                    ["smtpServer"] = _emailSettings.SmtpServer ?? "Unknown",
                    ["smtpPort"] = _emailSettings.SmtpPort
                });
        }
    }
}
