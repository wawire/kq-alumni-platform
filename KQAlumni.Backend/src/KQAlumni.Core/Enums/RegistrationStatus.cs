namespace KQAlumni.Core.Enums;

/// <summary>
/// Registration status workflow states
/// </summary>
public enum RegistrationStatus
{
  /// <summary>
  /// Registration submitted, awaiting ERP validation
  /// </summary>
  Pending = 0,

  /// <summary>
  /// ERP validated, verification email sent, awaiting user to click link
  /// </summary>
  Approved = 1,

  /// <summary>
  /// User clicked verification link, email verified, fully active
  /// </summary>
  Active = 2,

  /// <summary>
  /// ERP validation failed after 5 retry attempts
  /// </summary>
  Rejected = 3
}
