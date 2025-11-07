using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for validating alumni data against Oracle ERP system
/// [SECURITY CRITICAL] This service calls INTERNAL ERP API
/// [WARNING] ERP API URL (10.2.131.147:7010) is NEVER exposed to frontend
/// [WARNING] Backend must be deployed INSIDE KQ network to reach ERP
/// </summary>
public class ErpService : IErpService
{
  private readonly HttpClient _httpClient;
  private readonly ErpApiSettings _settings;
  private readonly ILogger<ErpService> _logger;

  public ErpService(
      HttpClient httpClient,
      IOptions<ErpApiSettings> settings,
      ILogger<ErpService> logger)
  {
    _httpClient = httpClient;
    _settings = settings.Value;
    _logger = logger;

    // Configure HttpClient for ERP API (only if valid URL and not in mock mode)
    if (!_settings.EnableMockMode && Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var baseUri))
    {
      _httpClient.BaseAddress = baseUri;
      _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }
    else if (_settings.EnableMockMode)
    {
      _logger.LogInformation("ERP Service initialized in MOCK MODE - ERP URL not configured");
    }
    else
    {
      _logger.LogWarning("ERP Service BaseUrl is invalid: {BaseUrl}. Service will fail in production mode.", _settings.BaseUrl);
    }
  }

  // ========================================
  // PUBLIC METHODS
  // ========================================

  public async Task<ErpValidationResult> ValidateStaffNumberAsync(
      string staffNumber,
      CancellationToken cancellationToken = default)
  {
    try
    {
      // [MOCK MODE] Use fake data for development (when ERP not accessible)
      if (_settings.EnableMockMode)
      {
        _logger.LogWarning("[WARNING] ERP Mock Mode Enabled - Using fake validation data");
        return GenerateMockValidationResult(staffNumber);
      }

      // [PRODUCTION] Call real ERP API (INTERNAL NETWORK ONLY)
      _logger.LogInformation("Validating staff number {StaffNumber} against ERP", staffNumber);

      var request = new { staffNumber };
      var response = await _httpClient.PostAsJsonAsync(
          _settings.Endpoint,
          request,
          cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("ERP API returned error: {StatusCode}", response.StatusCode);
        return CreateErrorResult("Unable to validate staff number. Please try again or contact HR.");
      }

      var erpData = await response.Content.ReadFromJsonAsync<ErpApiResponse>(cancellationToken);

      if (erpData == null || !erpData.Found)
      {
        _logger.LogWarning("Staff number {StaffNumber} not found in ERP", staffNumber);
        return CreateErrorResult("Staff number not found. Please verify and contact HR if issue persists.");
      }

      return new ErpValidationResult
      {
        IsValid = true,
        StaffName = erpData.FullName,
        Department = erpData.Department,
        ExitDate = erpData.ExitDate,
        NameSimilarityScore = 0, // Will be calculated in ValidateStaffDetailsAsync
        IsMockData = false
      };
    }
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "HTTP error calling ERP API for staff {StaffNumber}", staffNumber);
      return CreateErrorResult("Unable to connect to validation service. Please try again later.");
    }
    catch (TaskCanceledException ex)
    {
      _logger.LogError(ex, "ERP API timeout for staff {StaffNumber}", staffNumber);
      return CreateErrorResult("Validation service timeout. Please try again.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error validating staff {StaffNumber}", staffNumber);
      return CreateErrorResult("An unexpected error occurred. Please contact support.");
    }
  }

  public async Task<ErpValidationResult> ValidateStaffDetailsAsync(
      string staffNumber,
      string fullName,
      CancellationToken cancellationToken = default)
  {
    // Step 1: Validate staff number exists in ERP
    var result = await ValidateStaffNumberAsync(staffNumber, cancellationToken);

    if (!result.IsValid)
    {
      return result;
    }

    // Step 2: Validate name match
    if (string.IsNullOrEmpty(result.StaffName))
    {
      _logger.LogWarning("ERP returned valid staff number {StaffNumber} but no name", staffNumber);
      return result;
    }

    // [MOCK MODE] Skip name validation in development
    if (result.IsMockData)
    {
      _logger.LogInformation(
          "[MOCK MODE] Skipping name validation. Accepting '{ProvidedName}' for staff {StaffNumber}",
          fullName, staffNumber);

      result.NameSimilarityScore = 100; // Accept any name in mock mode
      return result;
    }

    // [PRODUCTION MODE] Perform strict name matching
    result.NameSimilarityScore = CalculateNameSimilarity(fullName, result.StaffName);

    // 80% threshold for name match
    if (result.NameSimilarityScore < 80)
    {
      _logger.LogWarning(
          "Name mismatch for {StaffNumber}: Expected '{ErpName}', Got '{ProvidedName}' (Similarity: {Score}%)",
          staffNumber, result.StaffName, fullName, result.NameSimilarityScore);

      result.IsValid = false;
      result.ErrorMessage = "Name does not match our records. Please use your full name as per company records.";
    }
    else
    {
      _logger.LogInformation(
          "Name validation passed for {StaffNumber}: '{ProvidedName}' matches '{ErpName}' (Similarity: {Score}%)",
          staffNumber, fullName, result.StaffName, result.NameSimilarityScore);
    }

    return result;
  }

  public async Task<ErpValidationResult> ValidateIdOrPassportAsync(
      string idOrPassport,
      CancellationToken cancellationToken = default)
  {
    try
    {
      // [MOCK MODE] Use fake data for development (when ERP not accessible)
      if (_settings.EnableMockMode)
      {
        _logger.LogWarning("[WARNING] ERP Mock Mode Enabled - Using fake validation data for ID lookup");
        return GenerateMockValidationResultForId(idOrPassport);
      }

      // [PRODUCTION] Call real ERP API (INTERNAL NETWORK ONLY)
      _logger.LogInformation("Validating ID/Passport {IdOrPassport} against ERP", idOrPassport);

      var request = new { idPassport = idOrPassport };
      var response = await _httpClient.PostAsJsonAsync(
          _settings.IdPassportEndpoint ?? _settings.Endpoint, // Use IdPassportEndpoint if available, fallback to default
          request,
          cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("ERP API returned error: {StatusCode}", response.StatusCode);
        return CreateErrorResult("Unable to validate ID/Passport. Please try again or contact HR.");
      }

      var erpData = await response.Content.ReadFromJsonAsync<ErpApiResponse>(cancellationToken);

      if (erpData == null || !erpData.Found)
      {
        _logger.LogWarning("ID/Passport {IdOrPassport} not found in ERP", idOrPassport);
        return CreateErrorResult("ID/Passport not found in our records. Please verify and contact HR if issue persists.");
      }

      return new ErpValidationResult
      {
        IsValid = true,
        StaffNumber = erpData.StaffNumber,
        StaffName = erpData.FullName,
        Department = erpData.Department,
        ExitDate = erpData.ExitDate,
        NameSimilarityScore = 0,
        IsMockData = false
      };
    }
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "HTTP error calling ERP API for ID/Passport {IdOrPassport}", idOrPassport);
      return CreateErrorResult("Unable to connect to validation service. Please try again later.");
    }
    catch (TaskCanceledException ex)
    {
      _logger.LogError(ex, "ERP API timeout for ID/Passport {IdOrPassport}", idOrPassport);
      return CreateErrorResult("Validation service timeout. Please try again.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error validating ID/Passport {IdOrPassport}", idOrPassport);
      return CreateErrorResult("An unexpected error occurred. Please contact support.");
    }
  }

  // ========================================
  // PRIVATE HELPER METHODS
  // ========================================

  /// <summary>
  /// Generates mock validation result for development
  /// [WARNING] ONLY USED IN DEVELOPMENT MODE
  /// [WARNING] Accepts ANY name to simplify testing
  /// </summary>
  private ErpValidationResult GenerateMockValidationResult(string staffNumber)
  {
    // Check if staff number is in mock whitelist
    if (!_settings.MockStaffNumbers.Contains(staffNumber))
    {
      _logger.LogWarning(
          "Staff number {StaffNumber} not in mock whitelist. Add to appsettings.Development.json MockStaffNumbers.",
          staffNumber);

      return CreateErrorResult(
          $"Staff number {staffNumber} not found in our records. Please verify your staff number and contact HR if this error persists.",
          isMockData: true);
    }

    // Generate mock staff data based on staff number pattern
    var (mockName, mockDepartment) = GetMockStaffData(staffNumber);

    _logger.LogInformation(
        "[MOCK ERP] Returning mock data for {StaffNumber} - Name: {MockName}, Dept: {MockDept}",
        staffNumber, mockName, mockDepartment);

    return new ErpValidationResult
    {
      IsValid = true,
      StaffNumber = staffNumber,
      StaffName = mockName,
      Department = mockDepartment,
      ExitDate = DateTime.UtcNow.AddMonths(-6),
      NameSimilarityScore = 100, // Mock mode always accepts name
      IsMockData = true
    };
  }

  /// <summary>
  /// Generates mock validation result for ID/Passport lookup in development
  /// [WARNING] ONLY USED IN DEVELOPMENT MODE
  /// </summary>
  private ErpValidationResult GenerateMockValidationResultForId(string idOrPassport)
  {
    // Map some test IDs to staff numbers for development
    var mockIdToStaffMap = new Dictionary<string, string>
    {
      { "12345678", "0012345" },
      { "87654321", "0087654" },
      { "11111111", "00C5050" },
      { "22222222", "00A1234" },
      { "A1234567", "00H7890" },
      { "B7654321", "00C9999" }
    };

    if (mockIdToStaffMap.TryGetValue(idOrPassport, out var staffNumber))
    {
      var (mockName, mockDepartment) = GetMockStaffData(staffNumber);

      _logger.LogInformation(
          "[MOCK ERP] ID/Passport {IdOrPassport} mapped to {StaffNumber} - Name: {MockName}, Dept: {MockDept}",
          idOrPassport, staffNumber, mockName, mockDepartment);

      return new ErpValidationResult
      {
        IsValid = true,
        StaffNumber = staffNumber,
        StaffName = mockName,
        Department = mockDepartment,
        ExitDate = DateTime.UtcNow.AddMonths(-6),
        NameSimilarityScore = 100,
        IsMockData = true
      };
    }

    _logger.LogWarning(
        "[MOCK ERP] ID/Passport {IdOrPassport} not in mock map. Add it for testing.",
        idOrPassport);

    return CreateErrorResult(
        $"ID/Passport {idOrPassport} not found in our records. Please verify and contact HR if this error persists.",
        isMockData: true);
  }

  /// <summary>
  /// Returns mock staff data based on staff number pattern
  /// </summary>
  private static (string Name, string Department) GetMockStaffData(string staffNumber)
  {
    if (staffNumber.Length < 3)
    {
      return ("Mock Alumni Member", "General Department");
    }

    return staffNumber.Substring(0, 3) switch
    {
      "000" => ("John Kamau Mwangi", "Flight Operations"),
      "00C" => ("Mary Wanjiku Njeri", "Customer Service"),
      "00A" => ("Peter Omondi Otieno", "IT Department"),
      "00H" => ("Sarah Akinyi Otieno", "Cabin Crew"),
      _ => ("Mock Alumni Member", "General Department")
    };
  }

  /// <summary>
  /// Creates a standardized error result
  /// </summary>
  private static ErpValidationResult CreateErrorResult(string errorMessage, bool isMockData = false)
  {
    return new ErpValidationResult
    {
      IsValid = false,
      ErrorMessage = errorMessage,
      IsMockData = isMockData
    };
  }

  /// <summary>
  /// Calculates name similarity using Levenshtein distance algorithm
  /// Returns percentage similarity (0-100)
  /// </summary>
  /// <param name="providedName">Name provided by user</param>
  /// <param name="erpName">Name from ERP system</param>
  /// <returns>Similarity percentage (0-100)</returns>
  private int CalculateNameSimilarity(string providedName, string erpName)
  {
    if (string.IsNullOrWhiteSpace(providedName) || string.IsNullOrWhiteSpace(erpName))
    {
      _logger.LogWarning("Name similarity calculation called with empty name(s)");
      return 0;
    }

    // Normalize names (lowercase, remove extra spaces, trim)
    var normalizedProvided = NormalizeName(providedName);
    var normalizedErp = NormalizeName(erpName);

    // Exact match after normalization
    if (normalizedProvided == normalizedErp)
    {
      _logger.LogDebug("Exact name match after normalization");
      return 100;
    }

    // Calculate Levenshtein distance
    int distance = LevenshteinDistance(normalizedProvided, normalizedErp);
    int maxLength = Math.Max(normalizedProvided.Length, normalizedErp.Length);

    if (maxLength == 0)
    {
      return 100; // Both empty strings
    }

    // Convert distance to similarity percentage
    double similarityRatio = 1.0 - ((double)distance / maxLength);
    int similarityPercentage = (int)(similarityRatio * 100);

    // Ensure result is between 0 and 100
    return Math.Max(0, Math.Min(100, similarityPercentage));
  }

  /// <summary>
  /// Normalizes a name for comparison
  /// </summary>
  private static string NormalizeName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return string.Empty;
    }

    // Convert to lowercase, split by whitespace, remove empty entries, rejoin
    var parts = name.ToLowerInvariant()
                   .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    return string.Join(" ", parts);
  }

  /// <summary>
  /// Calculates Levenshtein distance between two strings
  /// The Levenshtein distance is the minimum number of single-character edits
  /// (insertions, deletions, or substitutions) required to change one string into another
  /// </summary>
  /// <param name="s1">First string</param>
  /// <param name="s2">Second string</param>
  /// <returns>Minimum edit distance</returns>
  private static int LevenshteinDistance(string s1, string s2)
  {
    if (string.IsNullOrEmpty(s1))
    {
      return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
    }

    if (string.IsNullOrEmpty(s2))
    {
      return s1.Length;
    }

    int len1 = s1.Length;
    int len2 = s2.Length;

    // Create a 2D array to store distances
    int[,] distance = new int[len1 + 1, len2 + 1];

    // Initialize first column (distance from empty string)
    for (int i = 0; i <= len1; i++)
    {
      distance[i, 0] = i;
    }

    // Initialize first row (distance from empty string)
    for (int j = 0; j <= len2; j++)
    {
      distance[0, j] = j;
    }

    // Calculate distances
    for (int i = 1; i <= len1; i++)
    {
      for (int j = 1; j <= len2; j++)
      {
        // Cost is 0 if characters match, 1 if they don't
        int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

        distance[i, j] = Math.Min(
            Math.Min(
                distance[i - 1, j] + 1,      // Deletion
                distance[i, j - 1] + 1),     // Insertion
            distance[i - 1, j - 1] + cost);  // Substitution
      }
    }

    return distance[len1, len2];
  }
}

/// <summary>
/// ERP API response model
/// Maps to the response structure from Oracle ERP HR_Leavers service
/// </summary>
internal class ErpApiResponse
{
  /// <summary>
  /// Whether the staff number/ID was found in ERP
  /// </summary>
  public bool Found { get; set; }

  /// <summary>
  /// Staff number (populated when lookup is by ID/Passport)
  /// </summary>
  public string? StaffNumber { get; set; }

  /// <summary>
  /// Full name of the staff member from ERP records
  /// </summary>
  public string? FullName { get; set; }

  /// <summary>
  /// Department of the staff member
  /// </summary>
  public string? Department { get; set; }

  /// <summary>
  /// Exit date from Kenya Airways
  /// </summary>
  public DateTime? ExitDate { get; set; }
}
