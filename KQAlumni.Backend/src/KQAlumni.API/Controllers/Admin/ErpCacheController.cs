using KQAlumni.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KQAlumni.API.Controllers.Admin;

/// <summary>
/// Admin API for managing ERP cache
/// </summary>
[ApiController]
[Route("api/v1/admin/erp-cache")]
[Authorize(Roles = "Admin")]
public class ErpCacheController : ControllerBase
{
  private readonly IErpCacheService _cacheService;
  private readonly ILogger<ErpCacheController> _logger;

  public ErpCacheController(
      IErpCacheService cacheService,
      ILogger<ErpCacheController> logger)
  {
    _cacheService = cacheService;
    _logger = logger;
  }

  /// <summary>
  /// Get ERP cache statistics
  /// </summary>
  [HttpGet("stats")]
  [ProducesResponseType(typeof(ErpCacheStats), StatusCodes.Status200OK)]
  public IActionResult GetCacheStats()
  {
    var stats = _cacheService.GetCacheStats();
    return Ok(stats);
  }

  /// <summary>
  /// Manually refresh ERP cache
  /// Forces an immediate cache refresh instead of waiting for background service
  /// </summary>
  [HttpPost("refresh")]
  [ProducesResponseType(typeof(ErpCacheRefreshResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RefreshCache(CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Manual ERP cache refresh requested by admin");

      await _cacheService.RefreshCacheAsync(cancellationToken);

      var stats = _cacheService.GetCacheStats();

      return Ok(new ErpCacheRefreshResponse
      {
        Success = true,
        Message = "ERP cache refreshed successfully",
        RecordCount = stats.CachedRecordCount,
        RefreshTime = stats.LastRefreshTime ?? DateTime.UtcNow
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to manually refresh ERP cache");

      return StatusCode(500, new ErpCacheRefreshResponse
      {
        Success = false,
        Message = $"Failed to refresh cache: {ex.Message}"
      });
    }
  }
}

/// <summary>
/// Response for cache refresh operation
/// </summary>
public class ErpCacheRefreshResponse
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public int RecordCount { get; set; }
  public DateTime RefreshTime { get; set; }
}
