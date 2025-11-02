using KQAlumni.Core.DTOs;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace KQAlumni.API.Middleware;

/// <summary>
/// Rate limiting middleware
/// Limits requests per IP address for registration and admin login endpoints
/// - Registration: Configurable via RateLimiting:RequestsPerHour and RateLimiting:WindowMinutes
/// - Admin Login: Configurable via RateLimiting:MaxLoginAttempts and RateLimiting:LoginWindowMinutes
/// All settings are in appsettings.json
/// </summary>
public class RateLimitingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<RateLimitingMiddleware> _logger;
  private readonly IConfiguration _configuration;

  // Store IP addresses with their request timestamps
  private static readonly ConcurrentDictionary<string, List<DateTime>> _requestLog = new();
  private static readonly ConcurrentDictionary<string, List<DateTime>> _loginAttemptLog = new();

  // Rate limit settings (configurable)
  private readonly int _maxRequests;
  private readonly TimeSpan _timeWindow;
  private readonly int _maxLoginAttempts;
  private readonly TimeSpan _loginTimeWindow;

  public RateLimitingMiddleware(
      RequestDelegate next,
      ILogger<RateLimitingMiddleware> logger,
      IConfiguration configuration)
  {
    _next = next;
    _logger = logger;
    _configuration = configuration;

    // Read rate limiting configuration
    _maxRequests = _configuration.GetValue<int>("RateLimiting:RequestsPerHour", 10);
    var windowMinutes = _configuration.GetValue<int>("RateLimiting:WindowMinutes", 60);
    _timeWindow = TimeSpan.FromMinutes(windowMinutes);

    // Login-specific rate limiting (more strict for security)
    _maxLoginAttempts = _configuration.GetValue<int>("RateLimiting:MaxLoginAttempts", 5);
    var loginWindowMinutes = _configuration.GetValue<int>("RateLimiting:LoginWindowMinutes", 15);
    _loginTimeWindow = TimeSpan.FromMinutes(loginWindowMinutes);

    _logger.LogInformation(
      "Rate limiting configured: {MaxRequests} requests per {WindowMinutes} minutes, {MaxLoginAttempts} login attempts per {LoginWindowMinutes} minutes",
      _maxRequests,
      windowMinutes,
      _maxLoginAttempts,
      loginWindowMinutes);
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var ipAddress = GetClientIpAddress(context);

    // Apply rate limiting to registration POST endpoint
    if (context.Request.Method == HttpMethods.Post &&
        context.Request.Path.StartsWithSegments("/api/v1/registrations"))
    {
      if (!IsRequestAllowed(ipAddress))
      {
        _logger.LogWarning("Registration rate limit exceeded for IP: {IpAddress}", ipAddress);
        await HandleRateLimitExceeded(context, ipAddress, false);
        return;
      }

      // Log the request
      LogRequest(ipAddress);
    }

    // Apply rate limiting to admin login endpoint
    if (context.Request.Method == HttpMethods.Post &&
        context.Request.Path.StartsWithSegments("/api/v1/admin/login"))
    {
      if (!IsLoginAllowed(ipAddress))
      {
        _logger.LogWarning("Login rate limit exceeded for IP: {IpAddress}", ipAddress);
        await HandleRateLimitExceeded(context, ipAddress, true);
        return;
      }

      // Log the login attempt (we log before execution to count failed attempts too)
      LogLoginAttempt(ipAddress);
    }

    await _next(context);
  }

  private bool IsRequestAllowed(string ipAddress)
  {
    var now = DateTime.UtcNow;

    // Get or create request log for this IP
    var requests = _requestLog.GetOrAdd(ipAddress, _ => new List<DateTime>());

    lock (requests)
    {
      // Remove old requests outside the time window
      requests.RemoveAll(timestamp => now - timestamp > _timeWindow);

      // Check if limit exceeded
      return requests.Count < _maxRequests;
    }
  }

  private void LogRequest(string ipAddress)
  {
    var now = DateTime.UtcNow;
    var requests = _requestLog.GetOrAdd(ipAddress, _ => new List<DateTime>());

    lock (requests)
    {
      requests.Add(now);

      // Cleanup: Remove old entries
      requests.RemoveAll(timestamp => now - timestamp > _timeWindow);
    }
  }

  private bool IsLoginAllowed(string ipAddress)
  {
    var now = DateTime.UtcNow;

    // Get or create login attempt log for this IP
    var attempts = _loginAttemptLog.GetOrAdd(ipAddress, _ => new List<DateTime>());

    lock (attempts)
    {
      // Remove old attempts outside the time window
      attempts.RemoveAll(timestamp => now - timestamp > _loginTimeWindow);

      // Check if limit exceeded
      return attempts.Count < _maxLoginAttempts;
    }
  }

  private void LogLoginAttempt(string ipAddress)
  {
    var now = DateTime.UtcNow;
    var attempts = _loginAttemptLog.GetOrAdd(ipAddress, _ => new List<DateTime>());

    lock (attempts)
    {
      attempts.Add(now);

      // Cleanup: Remove old entries
      attempts.RemoveAll(timestamp => now - timestamp > _loginTimeWindow);
    }
  }

  private async Task HandleRateLimitExceeded(HttpContext context, string ipAddress, bool isLoginAttempt)
  {
    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
    context.Response.ContentType = "application/json";

    // Calculate retry-after time based on request type
    DateTime? oldestRequest = null;
    TimeSpan timeWindow;
    int maxAttempts;
    string requestType;

    if (isLoginAttempt)
    {
      var attempts = _loginAttemptLog.GetOrAdd(ipAddress, _ => new List<DateTime>());
      lock (attempts)
      {
        if (attempts.Count > 0)
        {
          oldestRequest = attempts.OrderBy(x => x).First();
        }
      }
      timeWindow = _loginTimeWindow;
      maxAttempts = _maxLoginAttempts;
      requestType = "login";
    }
    else
    {
      var requests = _requestLog.GetOrAdd(ipAddress, _ => new List<DateTime>());
      lock (requests)
      {
        if (requests.Count > 0)
        {
          oldestRequest = requests.OrderBy(x => x).First();
        }
      }
      timeWindow = _timeWindow;
      maxAttempts = _maxRequests;
      requestType = "registration";
    }

    var retryAfter = oldestRequest.HasValue
        ? oldestRequest.Value.Add(timeWindow)
        : DateTime.UtcNow.Add(timeWindow);

    // Use indexer to set header (ASP0019 fix - prevents duplicate key exceptions)
    context.Response.Headers["Retry-After"] = ((int)(retryAfter - DateTime.UtcNow).TotalSeconds).ToString();

    var errorResponse = new ErrorResponse
    {
      Type = "https://tools.ietf.org/html/rfc6585#section-4",
      Title = "Rate limit exceeded",
      Status = (int)HttpStatusCode.TooManyRequests,
      Detail = $"Maximum {maxAttempts} {requestType} {(isLoginAttempt ? "attempts" : "requests")} per {(isLoginAttempt ? timeWindow.TotalMinutes : timeWindow.TotalMinutes)} minutes allowed. Please try again later.",
      Timestamp = DateTime.UtcNow
    };

    var jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
    await context.Response.WriteAsync(json);
  }

  private static string GetClientIpAddress(HttpContext context)
  {
    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    // Only trust proxy headers if request comes from a known proxy
    // In production, configure this list based on your actual infrastructure
    var knownProxies = new[]
    {
      "127.0.0.1",      // localhost
      "::1",            // localhost IPv6
      "10.0.0.0/8",     // Private network (if using internal load balancer)
      "172.16.0.0/12",  // Private network
      "192.168.0.0/16"  // Private network
    };

    // Check if request is from a known proxy
    bool isFromKnownProxy = false;
    if (context.Connection.RemoteIpAddress != null)
    {
      var remoteAddress = context.Connection.RemoteIpAddress.ToString();
      isFromKnownProxy = remoteAddress == "127.0.0.1" ||
                         remoteAddress == "::1" ||
                         remoteAddress.StartsWith("10.") ||
                         remoteAddress.StartsWith("172.") ||
                         remoteAddress.StartsWith("192.168.");
    }

    // Only trust forwarded headers if from known proxy
    if (isFromKnownProxy)
    {
      // Check for forwarded IP (if behind proxy/load balancer)
      var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
      if (!string.IsNullOrEmpty(forwardedFor))
      {
        var firstIp = forwardedFor.Split(',')[0].Trim();
        // Validate that it's a valid IP address
        if (System.Net.IPAddress.TryParse(firstIp, out _))
        {
          return firstIp;
        }
      }

      // Check for real IP header
      var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
      if (!string.IsNullOrEmpty(realIp) && System.Net.IPAddress.TryParse(realIp, out _))
      {
        return realIp;
      }
    }

    // Fallback to remote IP
    return remoteIp;
  }

  /// <summary>
  /// Background task to cleanup old entries (prevents memory leak)
  /// </summary>
  /// <param name="timeWindow">Time window for rate limiting (used for cleanup threshold)</param>
  public static void StartCleanupTask(TimeSpan timeWindow)
  {
    Task.Run(async () =>
    {
      while (true)
      {
        await Task.Delay(TimeSpan.FromMinutes(30));

        var now = DateTime.UtcNow;
        var keysToRemove = new List<string>();

        // Cleanup registration request log
        foreach (var kvp in _requestLog)
        {
          lock (kvp.Value)
          {
            kvp.Value.RemoveAll(timestamp => now - timestamp > timeWindow);

            // Remove IP if no recent requests
            if (kvp.Value.Count == 0)
            {
              keysToRemove.Add(kvp.Key);
            }
          }
        }

        // Remove empty entries from registration log
        foreach (var key in keysToRemove)
        {
          _requestLog.TryRemove(key, out _);
        }

        // Cleanup login attempt log
        keysToRemove.Clear();
        foreach (var kvp in _loginAttemptLog)
        {
          lock (kvp.Value)
          {
            kvp.Value.RemoveAll(timestamp => now - timestamp > timeWindow);

            // Remove IP if no recent login attempts
            if (kvp.Value.Count == 0)
            {
              keysToRemove.Add(kvp.Key);
            }
          }
        }

        // Remove empty entries from login attempt log
        foreach (var key in keysToRemove)
        {
          _loginAttemptLog.TryRemove(key, out _);
        }
      }
    });
  }
}
