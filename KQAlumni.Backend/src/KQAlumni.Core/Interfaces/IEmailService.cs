namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for sending emails to alumni
/// </summary>
public interface IEmailService
{
  /// <summary>
  /// Sends confirmation email immediately after registration (Email 1)
  /// Subject: "Registration Received - KQ Alumni Network"
  /// </summary>
  /// <param name="alumniName">Alumni full name</param>
  /// <param name="email">Alumni email address</param>
  /// <param name="registrationId">Registration ID for tracking</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if email sent successfully</returns>
  Task<bool> SendConfirmationEmailAsync(
      string alumniName,
      string email,
      Guid registrationId,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Sends approval email with verification link (Email 2)
  /// Subject: "Welcome to KQ Alumni Network - Registration Approved!"
  /// </summary>
  /// <param name="alumniName">Alumni full name</param>
  /// <param name="email">Alumni email address</param>
  /// <param name="verificationToken">32-character verification token</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if email sent successfully</returns>
  Task<bool> SendApprovalEmailAsync(
      string alumniName,
      string email,
      string verificationToken,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Sends rejection email with HR contact info (Email 3)
  /// Subject: "KQ Alumni Registration - Unable to Verify"
  /// </summary>
  /// <param name="alumniName">Alumni full name</param>
  /// <param name="email">Alumni email address</param>
  /// <param name="staffNumber">Staff number that failed validation</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if email sent successfully</returns>
  Task<bool> SendRejectionEmailAsync(
      string alumniName,
      string email,
      string staffNumber,
      CancellationToken cancellationToken = default);
}
