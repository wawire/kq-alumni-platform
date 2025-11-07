namespace KQAlumni.Core.Entities;

/// <summary>
/// Configuration settings for ERP API integration
/// Maps to "ErpApi" section in appsettings.json
/// </summary>
public class ErpApiSettings
{
  public const string SectionName = "ErpApi";

  /// <summary>
  /// Base URL of the ERP API (e.g., http://10.2.131.147:7010)
  /// ⚠️ INTERNAL NETWORK ONLY - Never expose to frontend
  /// </summary>
  public string BaseUrl { get; set; } = string.Empty;

  /// <summary>
  /// Endpoint path for leavers validation
  /// </summary>
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
  public int TimeoutSeconds { get; set; } = 10;

  /// <summary>
  /// Number of retry attempts on failure
  /// </summary>
  public int RetryCount { get; set; } = 3;

  /// <summary>
  /// Delay between retries in seconds
  /// </summary>
  public int RetryDelaySeconds { get; set; } = 2;

  /// <summary>
  /// Circuit breaker failure threshold
  /// </summary>
  public int CircuitBreakerFailureThreshold { get; set; } = 5;

  /// <summary>
  /// Circuit breaker sampling duration in seconds
  /// </summary>
  public int CircuitBreakerSamplingDurationSeconds { get; set; } = 60;

  /// <summary>
  /// Circuit breaker minimum throughput
  /// </summary>
  public int CircuitBreakerMinimumThroughput { get; set; } = 3;

  /// <summary>
  /// Circuit breaker break duration in seconds
  /// </summary>
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
}

/// <summary>
/// Mock employee data for development and testing
/// </summary>
public class MockEmployee
{
  /// <summary>
  /// Staff number (e.g., "0012345")
  /// </summary>
  public string StaffNumber { get; set; } = string.Empty;

  /// <summary>
  /// National ID number (e.g., "12345678")
  /// </summary>
  public string? IdNumber { get; set; }

  /// <summary>
  /// Passport number (e.g., "A1234567")
  /// </summary>
  public string? PassportNumber { get; set; }

  /// <summary>
  /// Full name of employee
  /// </summary>
  public string FullName { get; set; } = string.Empty;

  /// <summary>
  /// Email address
  /// </summary>
  public string? Email { get; set; }

  /// <summary>
  /// Department
  /// </summary>
  public string Department { get; set; } = string.Empty;

  /// <summary>
  /// Exit date (defaults to 6 months ago if not specified)
  /// </summary>
  public DateTime? ExitDate { get; set; }
}
