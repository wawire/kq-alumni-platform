using System.Collections.Concurrent;

namespace KQAlumni.API.Services;

/// <summary>
/// Monitors rate limiting metrics and provides insights
/// Runs as a background service to periodically log rate limiting statistics
/// </summary>
public class RateLimitMonitor : BackgroundService
{
    private readonly ILogger<RateLimitMonitor> _logger;
    private readonly ConcurrentDictionary<string, RateLimitStats> _stats = new();
    private readonly TimeSpan _reportingInterval = TimeSpan.FromMinutes(15);

    public RateLimitMonitor(ILogger<RateLimitMonitor> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[MONITOR] Rate Limit Monitor started. Reporting every {Interval} minutes",
            _reportingInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_reportingInterval, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                LogRateLimitStats();
            }
        }

        _logger.LogInformation("[MONITOR] Rate Limit Monitor stopped");
    }

    /// <summary>
    /// Record a rate limit hit
    /// </summary>
    public void RecordRateLimitHit(string ipAddress, string endpoint)
    {
        var key = $"{ipAddress}|{endpoint}";
        _stats.AddOrUpdate(key,
            _ => new RateLimitStats
            {
                IpAddress = ipAddress,
                Endpoint = endpoint,
                HitCount = 1,
                FirstHit = DateTime.UtcNow,
                LastHit = DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.HitCount++;
                existing.LastHit = DateTime.UtcNow;
                return existing;
            });
    }

    /// <summary>
    /// Record a successful request
    /// </summary>
    public void RecordSuccessfulRequest(string ipAddress, string endpoint)
    {
        var key = $"{ipAddress}|{endpoint}";
        _stats.AddOrUpdate(key,
            _ => new RateLimitStats
            {
                IpAddress = ipAddress,
                Endpoint = endpoint,
                SuccessCount = 1,
                FirstHit = DateTime.UtcNow,
                LastHit = DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.SuccessCount++;
                existing.LastHit = DateTime.UtcNow;
                return existing;
            });
    }

    /// <summary>
    /// Get current statistics
    /// </summary>
    public Dictionary<string, RateLimitStats> GetStats()
    {
        return new Dictionary<string, RateLimitStats>(_stats);
    }

    /// <summary>
    /// Log aggregated rate limiting statistics
    /// </summary>
    private void LogRateLimitStats()
    {
        if (_stats.IsEmpty)
        {
            _logger.LogInformation("[STATS] Rate Limiting Stats: No activity in the last {Interval} minutes",
                _reportingInterval.TotalMinutes);
            return;
        }

        var totalHits = _stats.Values.Sum(s => s.HitCount);
        var totalSuccess = _stats.Values.Sum(s => s.SuccessCount);
        var totalRequests = totalHits + totalSuccess;
        var hitRate = totalRequests > 0 ? (totalHits * 100.0 / totalRequests) : 0;

        var topOffenders = _stats.Values
            .Where(s => s.HitCount > 0)
            .OrderByDescending(s => s.HitCount)
            .Take(10)
            .ToList();

        _logger.LogInformation(
            "\n" +
            "╔════════════════════════════════════════════════════════════════╗\n" +
            "║              RATE LIMITING STATISTICS REPORT                  ║\n" +
            "╠════════════════════════════════════════════════════════════════╣\n" +
            "║ Reporting Period: Last {Period,-10} minutes                    ║\n" +
            "║                                                                ║\n" +
            "║ SUMMARY:                                                       ║\n" +
            "║   Total Requests:        {TotalRequests,-34} ║\n" +
            "║   Successful:            {TotalSuccess,-34} ║\n" +
            "║   Rate Limited (429):    {TotalHits,-34} ║\n" +
            "║   Hit Rate:              {HitRate,-33}% ║\n" +
            "╚════════════════════════════════════════════════════════════════╝",
            _reportingInterval.TotalMinutes,
            totalRequests,
            totalSuccess,
            totalHits,
            hitRate.ToString("F2")
        );

        if (topOffenders.Any())
        {
            _logger.LogWarning(
                "[WARNING] Top 10 Rate Limited IP Addresses:\n{TopOffenders}",
                string.Join("\n", topOffenders.Select((s, i) =>
                    $"   {i + 1,2}. {s.IpAddress,-15} | {s.Endpoint,-30} | Hits: {s.HitCount,4} | Last: {s.LastHit:HH:mm:ss}")));
        }

        // Clean up old stats (older than 1 hour)
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var keysToRemove = _stats.Where(kvp => kvp.Value.LastHit < cutoff).Select(kvp => kvp.Key).ToList();

        foreach (var key in keysToRemove)
        {
            _stats.TryRemove(key, out _);
        }

        if (keysToRemove.Any())
        {
            _logger.LogDebug("[CLEANUP] Cleaned up {Count} old rate limit stats", keysToRemove.Count);
        }
    }
}

/// <summary>
/// Rate limiting statistics for a specific IP/endpoint combination
/// </summary>
public class RateLimitStats
{
    public string IpAddress { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int HitCount { get; set; }
    public int SuccessCount { get; set; }
    public DateTime FirstHit { get; set; }
    public DateTime LastHit { get; set; }
}
