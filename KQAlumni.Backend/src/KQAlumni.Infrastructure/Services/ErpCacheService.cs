using System.Text.Json;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// In-memory cache service for ERP employee data
/// Fetches all employees once, then serves from memory (20s → 1ms)
/// Background service refreshes cache hourly
/// </summary>
public class ErpCacheService : IErpCacheService
{
  private readonly HttpClient _httpClient;
  private readonly ErpApiSettings _settings;
  private readonly ILogger<ErpCacheService> _logger;

  // In-memory cache
  private List<ErpCachedEmployee> _cache = new();
  private DateTime? _lastRefreshTime;
  private string? _lastError;
  private readonly SemaphoreSlim _refreshLock = new(1, 1);

  public ErpCacheService(
      HttpClient httpClient,
      IOptions<ErpApiSettings> settings,
      ILogger<ErpCacheService> logger)
  {
    _httpClient = httpClient;
    _settings = settings.Value;
    _logger = logger;

    // Configure HttpClient timeout for large payload
    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
  }

  /// <summary>
  /// Fast lookup in cache by National ID
  /// </summary>
  public ErpCachedEmployee? FindByNationalId(string nationalId)
  {
    if (string.IsNullOrWhiteSpace(nationalId))
      return null;

    _logger.LogInformation(
      "Searching cache for National ID: '{NationalId}' (Length: {Length}, Cache Size: {CacheSize})",
      nationalId, nationalId.Length, _cache.Count);

    // Log first 5 IDs in cache for comparison (if cache has data)
    if (_cache.Count > 0 && _cache.Count <= 5)
    {
      var sampleIds = string.Join(", ", _cache.Take(5).Select(e => $"'{e.NationalIdentifier}'"));
      _logger.LogInformation("Sample IDs in cache: {SampleIds}", sampleIds);
    }
    else if (_cache.Count > 5)
    {
      var sampleIds = string.Join(", ", _cache.Take(5).Select(e => $"'{e.NationalIdentifier}'"));
      _logger.LogInformation("Sample IDs in cache (first 5 of {Total}): {SampleIds}", _cache.Count, sampleIds);
    }

    var result = _cache.FirstOrDefault(e =>
      !string.IsNullOrEmpty(e.NationalIdentifier) &&
      e.NationalIdentifier.Equals(nationalId, StringComparison.OrdinalIgnoreCase));

    if (result != null)
    {
      _logger.LogInformation("Found match in cache: Staff={StaffId}, Name={FullName}", result.StaffId, result.FullName);
    }
    else
    {
      _logger.LogWarning("No match found in cache for National ID: '{NationalId}'", nationalId);
    }

    return result;
  }

  /// <summary>
  /// Refreshes cache by fetching all employees from ERP
  /// Thread-safe: Only one refresh at a time
  /// </summary>
  public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
  {
    // Prevent concurrent refreshes
    await _refreshLock.WaitAsync(cancellationToken);
    try
    {
      _logger.LogInformation("Starting ERP cache refresh...");

      // Check if caching is enabled
      if (!_settings.EnableCaching)
      {
        _logger.LogWarning("ERP caching is disabled in configuration");
        return;
      }

      // Check if mock mode
      if (_settings.EnableMockMode)
      {
        _logger.LogInformation("ERP in mock mode - skipping cache refresh");
        return;
      }

      // Build URL to fetch ALL employees
      var fullUrl = $"{_settings.BaseUrl}{_settings.Endpoint}";
      _logger.LogInformation("Fetching all ERP employees from: {Url}", fullUrl);

      // Fetch from ERP
      var response = await _httpClient.GetAsync(fullUrl, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var error = $"ERP API returned error: {response.StatusCode}";
        _logger.LogError(error);
        _lastError = error;
        return;
      }

      // Parse JSON array
      var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
      var employees = ParseAllEmployees(jsonContent);

      // Update cache
      _cache = employees;
      _lastRefreshTime = DateTime.UtcNow;
      _lastError = null;

      _logger.LogInformation(
        "ERP cache refreshed successfully: {Count} employees cached, {Size:N0} KB",
        _cache.Count,
        jsonContent.Length / 1024.0);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to refresh ERP cache");
      _lastError = ex.Message;
      throw;
    }
    finally
    {
      _refreshLock.Release();
    }
  }

  /// <summary>
  /// Returns cache statistics
  /// </summary>
  public ErpCacheStats GetCacheStats()
  {
    return new ErpCacheStats
    {
      LastRefreshTime = _lastRefreshTime,
      CachedRecordCount = _cache.Count,
      IsHealthy = _lastRefreshTime.HasValue && _lastError == null,
      LastError = _lastError
    };
  }

  /// <summary>
  /// Parses all employees from ERP JSON response
  /// </summary>
  private List<ErpCachedEmployee> ParseAllEmployees(string jsonContent)
  {
    var employees = new List<ErpCachedEmployee>();

    try
    {
      var jsonArray = JsonDocument.Parse(jsonContent);

      if (jsonArray.RootElement.ValueKind != JsonValueKind.Array)
      {
        _logger.LogWarning("ERP API returned non-array response");
        return employees;
      }

      // Parse each employee record
      foreach (var element in jsonArray.RootElement.EnumerateArray())
      {
        try
        {
          // Extract NATIONAL_IDENTIFIER (can be string or null object)
          string? nationalId = null;
          if (element.TryGetProperty("NATIONAL_IDENTIFIER", out var natIdProp))
          {
            if (natIdProp.ValueKind == JsonValueKind.String)
            {
              nationalId = natIdProp.GetString();
            }
          }

          // Skip records without NATIONAL_IDENTIFIER
          if (string.IsNullOrWhiteSpace(nationalId))
            continue;

          // Extract other fields
          var staffId = element.TryGetProperty("STAFFID", out var staffIdProp)
            ? staffIdProp.GetString() : null;
          var fullName = element.TryGetProperty("FULLNAME", out var fullNameProp)
            ? fullNameProp.GetString() : null;
          var department = element.TryGetProperty("DEPARTMENT", out var deptProp)
            ? deptProp.GetString()
            : element.TryGetProperty("ORGANISATION", out var orgProp)
              ? orgProp.GetString()
              : null;
          var actualTerminationDate = element.TryGetProperty("ACTUAL_TERMINATION_DATE", out var dateProp)
            ? dateProp.GetString() : null;

          // Parse exit date
          DateTime? exitDate = null;
          if (!string.IsNullOrEmpty(actualTerminationDate) &&
              DateTime.TryParse(actualTerminationDate, out var parsedDate))
          {
            exitDate = parsedDate;
          }

          // Add to cache
          employees.Add(new ErpCachedEmployee
          {
            StaffId = staffId,
            FullName = fullName,
            NationalIdentifier = nationalId,
            Department = department,
            ExitDate = exitDate
          });
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Failed to parse single employee record");
          // Continue processing other records
        }
      }

      _logger.LogInformation("Parsed {Count} employees from ERP response", employees.Count);
      return employees;
    }
    catch (JsonException ex)
    {
      _logger.LogError(ex, "Failed to parse ERP JSON response");
      return employees;
    }
  }
}
