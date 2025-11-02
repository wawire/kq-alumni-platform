namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for generating and validating verification tokens
/// </summary>
public interface ITokenService
{
  /// <summary>
  /// Generates a secure verification token for email verification
  /// </summary>
  /// <param name="registrationId">Registration ID</param>
  /// <param name="email">Alumni email address</param>
  /// <returns>Verification token (32-character alphanumeric string)</returns>
  string GenerateVerificationToken(Guid registrationId, string email);

  /// <summary>
  /// Validates a verification token
  /// </summary>
  /// <param name="token">Token to validate</param>
  /// <returns>True if token format is valid</returns>
  bool ValidateTokenFormat(string token);
}
