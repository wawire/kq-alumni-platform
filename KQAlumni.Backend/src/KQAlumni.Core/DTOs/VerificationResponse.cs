namespace KQAlumni.Core.DTOs;

/// <summary>
/// Response after email verification
/// </summary>
public class VerificationResponse
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public string? Email { get; set; }
  public string? FullName { get; set; }
}

/// <summary>
/// Registration status check response
/// </summary>
public class RegistrationStatusResponse
{
  public string Status { get; set; } = string.Empty;
  public DateTime RegisteredAt { get; set; }
  public DateTime? ApprovedAt { get; set; }
  public bool EmailVerified { get; set; }
  public DateTime? EmailVerifiedAt { get; set; }
  public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Internal verification result (used by service layer)
/// </summary>
public class EmailVerificationResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public string? Email { get; set; }
  public string? FullName { get; set; }
}

/// <summary>
/// Response for ID/Passport verification during registration
/// </summary>
public class IdVerificationResponse
{
  /// <summary>
  /// Whether the ID/Passport was successfully verified in ERP
  /// </summary>
  public bool IsVerified { get; set; }

  /// <summary>
  /// Staff number from ERP (if verified)
  /// </summary>
  public string? StaffNumber { get; set; }

  /// <summary>
  /// Full name from ERP records (if verified)
  /// </summary>
  public string? FullName { get; set; }

  /// <summary>
  /// Department from ERP (if verified)
  /// </summary>
  public string? Department { get; set; }

  /// <summary>
  /// Exit date from ERP (if available)
  /// </summary>
  public DateTime? ExitDate { get; set; }

  /// <summary>
  /// Message explaining the verification result
  /// </summary>
  public string Message { get; set; } = string.Empty;

  /// <summary>
  /// Whether this person is already registered
  /// </summary>
  public bool IsAlreadyRegistered { get; set; }
}
