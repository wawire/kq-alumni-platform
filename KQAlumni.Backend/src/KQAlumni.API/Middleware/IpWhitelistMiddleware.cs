using System.Net;

namespace KQAlumni.API.Middleware;

/// <summary>
/// Middleware to restrict admin routes to whitelisted IP addresses
/// </summary>
public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly HashSet<string> _whitelist;
    private readonly bool _enabled;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;

        // Check if IP whitelisting is enabled
        _enabled = configuration.GetValue<bool>("IpWhitelist:Enabled", false);

        // Load whitelisted IPs from configuration
        var whitelistIps = configuration.GetSection("IpWhitelist:AllowedIps").Get<string[]>() ?? Array.Empty<string>();
        _whitelist = new HashSet<string>(whitelistIps, StringComparer.OrdinalIgnoreCase);

        // Always allow localhost
        _whitelist.Add("127.0.0.1");
        _whitelist.Add("::1");
        _whitelist.Add("localhost");

        if (_enabled)
        {
            _logger.LogInformation("IP Whitelisting enabled with {Count} allowed IPs", _whitelist.Count);
        }
        else
        {
            _logger.LogInformation("IP Whitelisting is disabled");
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Only check whitelisting for admin routes
        if (_enabled && path.Contains("/api/v1/admin"))
        {
            var remoteIp = GetClientIpAddress(context);

            if (remoteIp == null || !IsIpWhitelisted(remoteIp))
            {
                _logger.LogWarning(
                    "Access denied from IP {IpAddress} to {Path}",
                    remoteIp ?? "unknown",
                    path);

                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Access Denied",
                    message = "Your IP address is not authorized to access this resource",
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            _logger.LogDebug("IP {IpAddress} granted access to {Path}", remoteIp, path);
        }

        await _next(context);
    }

    private string? GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header (for reverse proxy scenarios)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Check X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private bool IsIpWhitelisted(string ipAddress)
    {
        // Direct match
        if (_whitelist.Contains(ipAddress))
        {
            return true;
        }

        // Check for CIDR notation support (simple implementation)
        // This can be enhanced with a proper CIDR library if needed
        foreach (var whitelistedIp in _whitelist)
        {
            if (whitelistedIp.Contains('/'))
            {
                // CIDR notation detected - for now, just do a prefix match
                var prefix = whitelistedIp.Split('/')[0];
                if (ipAddress.StartsWith(prefix))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
