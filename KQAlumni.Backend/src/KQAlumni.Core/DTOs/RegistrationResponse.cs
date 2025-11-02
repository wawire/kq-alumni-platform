namespace KQAlumni.Core.DTOs;

/// <summary>
/// Response model for successful alumni registration
/// </summary>
public class RegistrationResponse
{
  /// <summary>
  /// Unique registration ID
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Staff number
  /// </summary>
  public string StaffNumber { get; set; } = string.Empty;

  /// <summary>
  /// Full name
  /// </summary>
  public string FullName { get; set; } = string.Empty;

  /// <summary>
  /// Email address
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Full mobile number (formatted)
  /// </summary>
  public string? Mobile { get; set; }

  /// <summary>
  /// Registration status (Verified, Pending, etc.)
  /// </summary>
  public string Status { get; set; } = string.Empty;

  /// <summary>
  /// Timestamp when registered (UTC)
  /// </summary>
  public DateTime RegisteredAt { get; set; }

  /// <summary>
  /// Success message
  /// </summary>
  public string Message { get; set; } = string.Empty;
}
