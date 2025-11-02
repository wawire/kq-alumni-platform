namespace KQAlumni.Core.DTOs;

/// <summary>
/// Standard error response model for API errors
/// Follows RFC 7807 Problem Details specification
/// </summary>
public class ErrorResponse
{
  /// <summary>
  /// A URI reference that identifies the problem type
  /// </summary>
  public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

  /// <summary>
  /// A short, human-readable summary of the problem
  /// </summary>
  public string Title { get; set; } = "One or more validation errors occurred";

  /// <summary>
  /// HTTP status code
  /// </summary>
  public int Status { get; set; }

  /// <summary>
  /// A human-readable explanation specific to this occurrence of the problem
  /// </summary>
  public string? Detail { get; set; }

  /// <summary>
  /// Dictionary of validation errors (field name -> error messages)
  /// </summary>
  public Dictionary<string, List<string>>? Errors { get; set; }

  /// <summary>
  /// Timestamp when error occurred
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
