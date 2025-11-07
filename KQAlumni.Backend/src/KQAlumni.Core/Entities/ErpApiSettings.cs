using System.ComponentModel.DataAnnotations;

namespace KQAlumni.Core.Entities;

/// <summary>
/// Configuration settings for ERP API integration
/// Maps to "ErpApi" section in appsettings.json
/// </summary>
public class ErpApiSettings : IValidatableObject
{
  public const string SectionName = "ErpApi";

  /// <summary>
  /// Base URL of the ERP API (e.g., http://10.2.131.147:7010)
  /// ⚠️ INTERNAL NETWORK ONLY - Never expose to frontend
  /// </summary>
  [Required(ErrorMessage = "ERP API BaseUrl is required")]
  public string BaseUrl { get; set; } = string.Empty;

  /// <summary>
  /// Endpoint path for leavers validation
  /// </summary>
  [Required(ErrorMessage = "ERP API Endpoint is required")]
  public string Endpoint { get; set; } = string.Empty;

  /// <summary>
  /// Endpoint path for ID/Passport validation (optional, defaults to Endpoint if not set)
  /// </summary>
  public string? IdPassportEndpoint { get; set; }

  /// <summary>
  /// Full URL (computed property)
  /// </summary>
  public string FullUrl => $"{BaseUrl.TrimEnd('/')}/{Endpoint.TrimStart('/')}";

  /// <summary>
  /// Request timeout in seconds
  /// </summary>
  [Range(1, 300, ErrorMessage = "TimeoutSeconds must be between 1 and 300")]
  public int TimeoutSeconds { get; set; } = 10;

  /// <summary>
  /// Number of retry attempts on failure
  /// </summary>
  [Range(0, 10, ErrorMessage = "RetryCount must be between 0 and 10")]
  public int RetryCount { get; set; } = 3;

  /// <summary>
  /// Delay between retries in seconds
  /// </summary>
  [Range(1, 60, ErrorMessage = "RetryDelaySeconds must be between 1 and 60")]
  public int RetryDelaySeconds { get; set; } = 2;

  /// <summary>
  /// Circuit breaker failure threshold
  /// </summary>
  [Range(1, 100, ErrorMessage = "CircuitBreakerFailureThreshold must be between 1 and 100")]
  public int CircuitBreakerFailureThreshold { get; set; } = 5;

  /// <summary>
  /// Circuit breaker sampling duration in seconds
  /// </summary>
  [Range(10, 600, ErrorMessage = "CircuitBreakerSamplingDurationSeconds must be between 10 and 600")]
  public int CircuitBreakerSamplingDurationSeconds { get; set; } = 60;

  /// <summary>
  /// Circuit breaker minimum throughput
  /// </summary>
  [Range(1, 100, ErrorMessage = "CircuitBreakerMinimumThroughput must be between 1 and 100")]
  public int CircuitBreakerMinimumThroughput { get; set; } = 3;

  /// <summary>
  /// Circuit breaker break duration in seconds
  /// </summary>
  [Range(10, 600, ErrorMessage = "CircuitBreakerBreakDurationSeconds must be between 10 and 600")]
  public int CircuitBreakerBreakDurationSeconds { get; set; } = 30;

  /// <summary>
  /// Enable mock mode for development (bypasses real ERP calls)
  /// </summary>
  public bool EnableMockMode { get; set; }

  /// <summary>
  /// Mock staff numbers that will pass validation in mock mode (legacy support)
  /// </summary>
  public List<string> MockStaffNumbers { get; set; } = new();

  /// <summary>
  /// Mock employee data for comprehensive testing (includes ID, Passport, Email, etc.)
  /// </summary>
  public List<MockEmployee> MockEmployees { get; set; } = new();

  /// <summary>
  /// API Key for ERP authentication (production only)
  /// </summary>
  public string? ApiKey { get; set; }

  /// <summary>
  /// Authentication scheme (ApiKey, Bearer, etc.)
  /// </summary>
  public string? AuthenticationScheme { get; set; }

  /// <summary>
  /// Custom validation logic for ERP API settings
  /// </summary>
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    var results = new List<ValidationResult>();

    // Validate BaseUrl is a valid URI (if not in mock mode)
    if (!EnableMockMode && !string.IsNullOrEmpty(BaseUrl))
    {
      if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
      {
        results.Add(new ValidationResult(
          $"ERP API BaseUrl '{BaseUrl}' is not a valid URI",
          new[] { nameof(BaseUrl) }));
      }
    }

    // In production, ensure mock mode is disabled
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (environment == "Production" && EnableMockMode)
    {
      results.Add(new ValidationResult(
        "ERP Mock Mode must be disabled in Production environment",
        new[] { nameof(EnableMockMode) }));
    }

    return results;
  }
}

/// <summary>
/// Mock employee data for development and testing
/// </summary>
public class MockEmployee
{
  /// <summary>
  /// Staff number (e.g., "0012345")
  /// </summary>
  [Required(ErrorMessage = "StaffNumber is required for mock employees")]
  [RegularExpression(@"^00[0-9A-Z]{5}$", ErrorMessage = "StaffNumber must match format: 00XXXXX (7 characters)")]
  public string StaffNumber { get; set; } = string.Empty;

  /// <summary>
  /// National ID number (e.g., "12345678")
  /// </summary>
  [MinLength(6, ErrorMessage = "IdNumber must be at least 6 characters")]
  [MaxLength(20, ErrorMessage = "IdNumber cannot exceed 20 characters")]
  public string? IdNumber { get; set; }

  /// <summary>
  /// Passport number (e.g., "A1234567")
  /// </summary>
  [MinLength(6, ErrorMessage = "PassportNumber must be at least 6 characters")]
  [MaxLength(20, ErrorMessage = "PassportNumber cannot exceed 20 characters")]
  public string? PassportNumber { get; set; }

  /// <summary>
  /// Full name of employee
  /// </summary>
  [Required(ErrorMessage = "FullName is required for mock employees")]
  [MinLength(2, ErrorMessage = "FullName must be at least 2 characters")]
  [MaxLength(200, ErrorMessage = "FullName cannot exceed 200 characters")]
  public string FullName { get; set; } = string.Empty;

  /// <summary>
  /// Email address
  /// </summary>
  [EmailAddress(ErrorMessage = "Email must be a valid email address")]
  public string? Email { get; set; }

  /// <summary>
  /// Department
  /// </summary>
  [Required(ErrorMessage = "Department is required for mock employees")]
  [MinLength(2, ErrorMessage = "Department must be at least 2 characters")]
  public string Department { get; set; } = string.Empty;

  /// <summary>
  /// Exit date (defaults to 6 months ago if not specified)
  /// </summary>
  public DateTime? ExitDate { get; set; }
}
