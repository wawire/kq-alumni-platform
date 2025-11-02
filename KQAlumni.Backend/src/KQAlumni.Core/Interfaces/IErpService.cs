namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for validating alumni data against Oracle ERP system
/// ⚠️ SECURITY: This service calls INTERNAL ERP API (10.2.131.147:7010)
/// ⚠️ NEVER expose ERP API URL to frontend
/// </summary>
public interface IErpService
{
  /// <summary>
  /// Validates staff number against ERP leavers database
  /// </summary>
  /// <param name="staffNumber">Staff number to validate (e.g., 0012345, 00C5050)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Validation result with staff details if found</returns>
  Task<ErpValidationResult> ValidateStaffNumberAsync(string staffNumber, CancellationToken cancellationToken = default);

  /// <summary>
  /// Validates staff number and name match
  /// </summary>
  /// <param name="staffNumber">Staff number</param>
  /// <param name="fullName">Full name to verify</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Validation result with similarity score</returns>
  Task<ErpValidationResult> ValidateStaffDetailsAsync(string staffNumber, string fullName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of ERP validation
/// </summary>
public class ErpValidationResult
{
  /// <summary>
  /// Whether the staff number exists in ERP
  /// </summary>
  public bool IsValid { get; set; }

  /// <summary>
  /// Staff name from ERP records
  /// </summary>
  public string? StaffName { get; set; }

  /// <summary>
  /// Department from ERP records
  /// </summary>
  public string? Department { get; set; }

  /// <summary>
  /// Exit date from ERP records
  /// </summary>
  public DateTime? ExitDate { get; set; }

  /// <summary>
  /// Name similarity score (0-100%, 80%+ is considered a match)
  /// </summary>
  public int NameSimilarityScore { get; set; }

  /// <summary>
  /// Error message if validation failed
  /// </summary>
  public string? ErrorMessage { get; set; }

  /// <summary>
  /// Whether this was validated using mock data (development only)
  /// </summary>
  public bool IsMockData { get; set; }
}
