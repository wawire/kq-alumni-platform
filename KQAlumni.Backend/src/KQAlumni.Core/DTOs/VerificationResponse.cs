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
