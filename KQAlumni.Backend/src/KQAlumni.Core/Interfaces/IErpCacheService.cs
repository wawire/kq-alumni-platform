namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for caching ERP employee data to improve performance
/// Reduces ERP API calls from 20+ seconds to milliseconds
/// </summary>
public interface IErpCacheService
{
  /// <summary>
  /// Finds an employee in the cache by National ID/Passport
  /// Returns null if not found
  /// </summary>
  ErpCachedEmployee? FindByNationalId(string nationalId);

  /// <summary>
  /// Refreshes the cache by fetching all employees from ERP
  /// Called by background service and admin manual refresh
  /// </summary>
  Task RefreshCacheAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets cache statistics
  /// </summary>
  ErpCacheStats GetCacheStats();
}

/// <summary>
/// Cached employee data from ERP
/// </summary>
public class ErpCachedEmployee
{
  public string? StaffId { get; set; }
  public string? FullName { get; set; }
  public string? NationalIdentifier { get; set; }
  public string? Department { get; set; }
  public DateTime? ExitDate { get; set; }
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public class ErpCacheStats
{
  public DateTime? LastRefreshTime { get; set; }
  public int CachedRecordCount { get; set; }
  public bool IsHealthy { get; set; }
  public string? LastError { get; set; }
  public TimeSpan? CacheAge => LastRefreshTime.HasValue
    ? DateTime.UtcNow - LastRefreshTime.Value
    : null;
}
