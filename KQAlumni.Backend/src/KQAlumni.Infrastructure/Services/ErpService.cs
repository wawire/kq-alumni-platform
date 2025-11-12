using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
  private readonly IErpCacheService? _cacheService;

  public ErpService(
      HttpClient httpClient,
      IOptions<ErpApiSettings> settings,
      ILogger<ErpService> logger,
      IErpCacheService? cacheService = null)
  {
    _httpClient = httpClient;
    _settings = settings.Value;
    _logger = logger;
    _cacheService = cacheService;

    // Configure HttpClient timeout
    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

    // Add Basic Authentication if credentials are provided
    if (!string.IsNullOrEmpty(_settings.BasicAuthUsername) && !string.IsNullOrEmpty(_settings.BasicAuthPassword))
    {
      var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.BasicAuthUsername}:{_settings.BasicAuthPassword}"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
      _logger.LogInformation("ERP Service configured with Basic Authentication");
    }

    if (_settings.EnableMockMode)
    {
      _logger.LogInformation("ERP Service initialized in MOCK MODE - ERP URL not configured");
    }
    else
    {
      _logger.LogInformation("ERP Service initialized for production: {BaseUrl}{Endpoint}", _settings.BaseUrl, _settings.Endpoint);
    }
  }

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

      // [CACHE MODE] Use cached data for fast lookups (if caching enabled)
      if (_settings.EnableCaching && _cacheService != null)
      {
        _logger.LogInformation("Looking up ID/Passport {IdOrPassport} in ERP cache", idOrPassport);

        var cachedEmployee = _cacheService.FindByNationalId(idOrPassport);

        if (cachedEmployee != null)
        {
          _logger.LogInformation(
            "Found in cache: Staff={StaffId}, Name={FullName}",
            cachedEmployee.StaffId, cachedEmployee.FullName);

          return new ErpValidationResult
          {
            IsValid = true,
            StaffNumber = cachedEmployee.StaffId,
            StaffName = cachedEmployee.FullName,
            Department = cachedEmployee.Department,
            ExitDate = cachedEmployee.ExitDate,
            NameSimilarityScore = 0,
            IsMockData = false
          };
        }

        _logger.LogWarning("ID/Passport {IdOrPassport} not found in cache - falling back to direct ERP API call", idOrPassport);
        // Fall through to direct ERP API call below instead of returning error
      }

      // [FALLBACK] Call real ERP API directly (when caching disabled OR not found in cache)
      _logger.LogInformation("Validating ID/Passport {IdOrPassport} against ERP API directly", idOrPassport);

      // Build full URL - use IdPassportEndpoint if specified, otherwise use Endpoint
      // Note: Must check for empty string, not just null
      var endpoint = string.IsNullOrEmpty(_settings.IdPassportEndpoint)
          ? _settings.Endpoint
          : _settings.IdPassportEndpoint;

      // ERP requires ALL query parameters to be present (even if empty)
      // Format: ?primaryKeyId=&staffid=&fullname=&nationalIdentifier=VALUE&contracttype=&...
      var queryParams = $"primaryKeyId=&staffid=&fullname=&nationalIdentifier={Uri.EscapeDataString(idOrPassport)}&contracttype=&persontype=&terminationYear=&actualTerminationDate=&leavingReason=&hiredate=&gradeName=&designation=&payrollname=&organisation=&department=";
      var fullUrl = $"{_settings.BaseUrl}{endpoint}?{queryParams}";

      _logger.LogInformation("Calling ERP API: {Url}", fullUrl);

      var response = await _httpClient.GetAsync(fullUrl, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("ERP API returned error: {StatusCode}", response.StatusCode);
        return CreateErrorResult("Unable to validate ID/Passport. Please try again or contact HR.");
      }

      // Parse JSON response (ERP returns JSON array of all leavers)
      var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
      var erpData = ParseErpJsonResponse(jsonContent, idOrPassport);

      if (erpData == null || !erpData.Found)
      {
        _logger.LogWarning("ID/Passport {IdOrPassport} not found in ERP", idOrPassport);
        return CreateErrorResult("ID/Passport not found in our records. Please verify and contact HR if issue persists.");
      }

      _logger.LogInformation(
          "ID/Passport {IdOrPassport} validated successfully. Staff: {StaffNumber}, Name: {FullName}",
          idOrPassport, erpData.StaffNumber, erpData.FullName);

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

  /// <summary>
  /// Parses JSON response from ERP API and finds the leaver matching the nationalIdentifier
  /// ERP returns JSON array of all leavers - we filter to find the matching record
  /// Handles NATIONAL_IDENTIFIER as either string or null object {"@nil": "true"}
  /// </summary>
  private ErpApiResponse? ParseErpJsonResponse(string jsonContent, string searchNationalId)
  {
    try
    {
      var jsonDoc = JsonDocument.Parse(jsonContent);
      JsonElement arrayElement;

      // Handle both response formats:
      // Format 1 (Old): Direct array [ {...}, {...} ]
      // Format 2 (New): Object with array { "ExEmployeesView": [ {...}, {...} ] }
      if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
      {
        // Old format: Direct array
        arrayElement = jsonDoc.RootElement;
        _logger.LogInformation("ERP returned direct array format");
      }
      else if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
      {
        // New format: Check for ExEmployeesView property
        if (jsonDoc.RootElement.TryGetProperty("ExEmployeesView", out var exEmployeesView))
        {
          arrayElement = exEmployeesView;
          _logger.LogInformation("ERP returned ExEmployeesView object format");
        }
        else
        {
          _logger.LogWarning("ERP API returned object but no ExEmployeesView property found");
          return null;
        }
      }
      else
      {
        _logger.LogWarning("ERP API returned unexpected format (not array or object)");
        return null;
      }

      if (arrayElement.ValueKind != JsonValueKind.Array)
      {
        _logger.LogWarning("Expected array but got {ValueKind}", arrayElement.ValueKind);
        return null;
      }

      var totalRecords = arrayElement.GetArrayLength();
      _logger.LogInformation(
        "Parsing ERP response: {TotalRecords} records returned, searching for ID '{SearchId}' (Length: {Length})",
        totalRecords, searchNationalId, searchNationalId.Length);

      var recordsWithIds = 0;
      var sampleIds = new List<string>();

      // Search through array to find matching NATIONAL_IDENTIFIER
      foreach (var element in arrayElement.EnumerateArray())
      {
        // Get NATIONAL_IDENTIFIER - try both lowercase (new API) and uppercase (old API)
        string? nationalId = null;
        if (element.TryGetProperty("nationalIdentifier", out var natIdProperty) ||
            element.TryGetProperty("NATIONAL_IDENTIFIER", out natIdProperty))
        {
          if (natIdProperty.ValueKind == JsonValueKind.String)
          {
            nationalId = natIdProperty.GetString();
            if (!string.IsNullOrEmpty(nationalId))
            {
              recordsWithIds++;
              if (sampleIds.Count < 5)
              {
                sampleIds.Add(nationalId);
              }
            }
          }
          // Skip if it's an object (null marker: {"@nil": "true"})
        }

        // Check if this record matches our search ID
        if (string.IsNullOrEmpty(nationalId) ||
            !nationalId.Equals(searchNationalId, StringComparison.OrdinalIgnoreCase))
        {
          continue; // Not a match, try next record
        }

        // Found matching record! Extract all fields (try both lowercase and uppercase)
        var staffId = element.TryGetProperty("staffid", out var staffIdProp) ||
                      element.TryGetProperty("STAFFID", out staffIdProp)
            ? staffIdProp.GetString() : null;
        var fullName = element.TryGetProperty("fullname", out var fullNameProp) ||
                       element.TryGetProperty("FULLNAME", out fullNameProp)
            ? fullNameProp.GetString() : null;
        var department = element.TryGetProperty("department", out var deptProp) ||
                         element.TryGetProperty("DEPARTMENT", out deptProp)
            ? deptProp.GetString()
            : element.TryGetProperty("organisation", out var orgProp) ||
              element.TryGetProperty("ORGANISATION", out orgProp)
                ? orgProp.GetString()
                : null;
        var actualTerminationDate = element.TryGetProperty("actualTerminationDate", out var dateProp) ||
                                    element.TryGetProperty("ACTUAL_TERMINATION_DATE", out dateProp)
            ? dateProp.GetString() : null;

        // Parse exit date
        DateTime? exitDate = null;
        if (!string.IsNullOrEmpty(actualTerminationDate) &&
            DateTime.TryParse(actualTerminationDate, out var parsedDate))
        {
          exitDate = parsedDate;
        }

        _logger.LogInformation(
            "Found ERP match for ID {NationalId}: Staff={StaffId}, Name={FullName}, Dept={Department}",
            searchNationalId, staffId, fullName, department);

        return new ErpApiResponse
        {
          Found = !string.IsNullOrEmpty(staffId),
          StaffNumber = staffId,
          FullName = fullName,
          Department = department,
          ExitDate = exitDate
        };
      }

      // No match found in the array
      _logger.LogWarning(
        "No matching NATIONAL_IDENTIFIER found in ERP response for: '{SearchId}'. Total records: {TotalRecords}, Records with IDs: {RecordsWithIds}, Sample IDs: {SampleIds}",
        searchNationalId, totalRecords, recordsWithIds, string.Join(", ", sampleIds.Select(id => $"'{id}'")));
      return null;
    }
    catch (JsonException ex)
    {
      _logger.LogError(ex, "Failed to parse ERP JSON response");
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error parsing ERP response");
      return null;
    }
  }

  /// <summary>
  /// Generates mock validation result for development
  /// [WARNING] ONLY USED IN DEVELOPMENT MODE
  /// [WARNING] Accepts ANY name to simplify testing
  /// </summary>
  private ErpValidationResult GenerateMockValidationResult(string staffNumber)
  {
    // First, check if employee exists in MockEmployees (new comprehensive mock data)
    var mockEmployee = _settings.MockEmployees.FirstOrDefault(e => e.StaffNumber == staffNumber);
    if (mockEmployee != null)
    {
      _logger.LogInformation(
          "[MOCK ERP] Found employee in MockEmployees - Staff: {StaffNumber}, Name: {Name}, Email: {Email}",
          mockEmployee.StaffNumber, mockEmployee.FullName, mockEmployee.Email);

      return new ErpValidationResult
      {
        IsValid = true,
        StaffNumber = mockEmployee.StaffNumber,
        StaffName = mockEmployee.FullName,
        Department = mockEmployee.Department,
        ExitDate = mockEmployee.ExitDate ?? DateTime.UtcNow.AddMonths(-6),
        NameSimilarityScore = 100,
        IsMockData = true
      };
    }

    // Fallback: Check legacy MockStaffNumbers list
    if (!_settings.MockStaffNumbers.Contains(staffNumber))
    {
      _logger.LogWarning(
          "Staff number {StaffNumber} not found in MockEmployees or MockStaffNumbers. Add to appsettings.Development.json",
          staffNumber);

      return CreateErrorResult(
          $"Staff number {staffNumber} not found in our records. Please verify your staff number and contact HR if this error persists.",
          isMockData: true);
    }

    // Generate generic mock data based on staff number pattern (legacy support)
    var (mockName, mockDepartment) = GetMockStaffData(staffNumber);

    _logger.LogInformation(
        "[MOCK ERP] Using legacy mock data for {StaffNumber} - Name: {MockName}, Dept: {MockDept}",
        staffNumber, mockName, mockDepartment);

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

  /// <summary>
  /// Generates mock validation result for ID/Passport lookup in development
  /// [WARNING] ONLY USED IN DEVELOPMENT MODE
  /// </summary>
  private ErpValidationResult GenerateMockValidationResultForId(string idOrPassport)
  {
    // Search MockEmployees by ID number or Passport number
    var mockEmployee = _settings.MockEmployees.FirstOrDefault(e =>
        (e.IdNumber != null && e.IdNumber.Equals(idOrPassport, StringComparison.OrdinalIgnoreCase)) ||
        (e.PassportNumber != null && e.PassportNumber.Equals(idOrPassport, StringComparison.OrdinalIgnoreCase)));

    if (mockEmployee != null)
    {
      _logger.LogInformation(
          "[MOCK ERP] Found employee by ID/Passport {IdOrPassport} - Staff: {StaffNumber}, Name: {Name}, Email: {Email}",
          idOrPassport, mockEmployee.StaffNumber, mockEmployee.FullName, mockEmployee.Email);

      return new ErpValidationResult
      {
        IsValid = true,
        StaffNumber = mockEmployee.StaffNumber,
        StaffName = mockEmployee.FullName,
        Department = mockEmployee.Department,
        ExitDate = mockEmployee.ExitDate ?? DateTime.UtcNow.AddMonths(-6),
        NameSimilarityScore = 100,
        IsMockData = true
      };
    }

    _logger.LogWarning(
        "[MOCK ERP] ID/Passport {IdOrPassport} not found in MockEmployees. Add to appsettings.Development.json",
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
