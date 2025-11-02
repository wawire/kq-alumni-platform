using System.Security.Cryptography;
using System.Text;
using KQAlumni.Core.Interfaces;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for generating and validating verification tokens
/// </summary>
public class TokenService : ITokenService
{
  /// <summary>
  /// Generates a secure verification token
  /// Format: 32-character alphanumeric string (e.g., a1b2c3d4e5f6789012345678901234ab)
  /// </summary>
  public string GenerateVerificationToken(Guid registrationId, string email)
  {
    // Create unique input combining ID, email, and timestamp
    var input = $"{registrationId}|{email}|{DateTime.UtcNow.Ticks}";

    // Generate SHA256 hash
    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

    // Convert to hex string (64 characters)
    var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

    // Take first 32 characters for token
    return hashString[..32];
  }

  /// <summary>
  /// Validates token format (32-character alphanumeric lowercase)
  /// </summary>
  public bool ValidateTokenFormat(string token)
  {
    if (string.IsNullOrWhiteSpace(token))
      return false;

    if (token.Length != 32)
      return false;

    // Check if alphanumeric (a-z, 0-9)
    return token.All(c => char.IsAsciiLetterOrDigit(c) && char.IsLower(c));
  }
}
