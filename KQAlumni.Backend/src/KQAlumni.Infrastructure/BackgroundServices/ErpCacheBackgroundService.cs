using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KQAlumni.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that automatically refreshes ERP cache
/// Runs on startup and then periodically based on configuration
/// </summary>
public class ErpCacheBackgroundService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ErpCacheBackgroundService> _logger;
  private readonly ErpApiSettings _settings;

  public ErpCacheBackgroundService(
      IServiceProvider serviceProvider,
      ILogger<ErpCacheBackgroundService> logger,
      IOptions<ErpApiSettings> settings)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
    _settings = settings.Value;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // Wait a bit for application to fully start
    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

    // Check if caching is enabled
    if (!_settings.EnableCaching)
    {
      _logger.LogInformation("ERP caching is disabled - background service will not run");
      return;
    }

    // Check if in mock mode
    if (_settings.EnableMockMode)
    {
      _logger.LogInformation("ERP in mock mode - cache refresh not needed");
      return;
    }

    _logger.LogInformation(
      "ERP Cache Background Service starting - will refresh every {Minutes} minutes",
      _settings.CacheRefreshIntervalMinutes);

    // Initial cache load on startup
    await RefreshCacheAsync(stoppingToken);

    // Set up periodic refresh
    var refreshInterval = TimeSpan.FromMinutes(_settings.CacheRefreshIntervalMinutes);
    using var timer = new PeriodicTimer(refreshInterval);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // Wait for next tick
        await timer.WaitForNextTickAsync(stoppingToken);

        // Refresh cache
        await RefreshCacheAsync(stoppingToken);
      }
      catch (OperationCanceledException)
      {
        // Application is shutting down
        _logger.LogInformation("ERP Cache Background Service stopping");
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unhandled error in ERP Cache Background Service");
        // Continue running, will retry next interval
      }
    }
  }

  private async Task RefreshCacheAsync(CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Background ERP cache refresh starting...");

      // Create scope to get scoped services
      using var scope = _serviceProvider.CreateScope();
      var cacheService = scope.ServiceProvider.GetRequiredService<IErpCacheService>();

      // Refresh cache
      await cacheService.RefreshCacheAsync(cancellationToken);

      // Log stats
      var stats = cacheService.GetCacheStats();
      _logger.LogInformation(
        "Background ERP cache refresh completed: {Count} employees cached",
        stats.CachedRecordCount);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Background ERP cache refresh failed - will retry next interval");
      // Don't throw - let the service continue running
    }
  }
}
