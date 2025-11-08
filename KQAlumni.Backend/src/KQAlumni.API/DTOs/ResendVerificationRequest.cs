namespace KQAlumni.API.DTOs;

/// <summary>
/// Request model for resending verification email
/// </summary>
public class ResendVerificationRequest
{
  /// <summary>
  /// Email address to resend verification to
  /// </summary>
  public string Email { get; set; } = string.Empty;
}
