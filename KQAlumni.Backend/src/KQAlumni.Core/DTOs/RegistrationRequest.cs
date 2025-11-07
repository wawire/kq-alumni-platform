namespace KQAlumni.Core.DTOs;

/// <summary>
/// Request model for alumni registration
/// Contains all fields collected during the registration process
/// </summary>
public class RegistrationRequest
{
  // ========================================
  // PERSONAL INFORMATION (Mandatory)
  // ========================================

  /// <summary>
  /// Staff number (7 characters): 000XXXX | 00CXXXX | 00AXXXX | 00HXXXX
  /// Optional - will be auto-populated from ERP using ID/Passport
  /// </summary>
  public string? StaffNumber { get; set; }

  /// <summary>
  /// National ID number or Passport number (Required)
  /// Used for ERP validation and verification
  /// </summary>
  public string IdNumber { get; set; } = string.Empty;

  /// <summary>
  /// Passport number (Optional - legacy field, now combined with IdNumber)
  /// </summary>
  public string? PassportNumber { get; set; }

  /// <summary>
  /// Full name as per company records
  /// </summary>
  public string FullName { get; set; } = string.Empty;

  // ========================================
  // CONTACT INFORMATION
  // ========================================

  /// <summary>
  /// Current email address
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Mobile country code (e.g., +254) - Optional
  /// </summary>
  public string? MobileCountryCode { get; set; }

  /// <summary>
  /// Mobile number without country code (e.g., 712345678) - Optional
  /// </summary>
  public string? MobileNumber { get; set; }

  /// <summary>
  /// Current country of residence (full name, e.g., "Kenya")
  /// </summary>
  public string CurrentCountry { get; set; } = string.Empty;

  /// <summary>
  /// Current country code (ISO 3166-1 Alpha-2, e.g., "KE")
  /// </summary>
  public string CurrentCountryCode { get; set; } = string.Empty;

  /// <summary>
  /// Current city of residence
  /// </summary>
  public string CurrentCity { get; set; } = string.Empty;

  /// <summary>
  /// Custom city name if "Other" was selected
  /// </summary>
  public string? CityCustom { get; set; }

  // ========================================
  // EMPLOYMENT INFORMATION (Optional)
  // ========================================

  /// <summary>
  /// Current employer name
  /// </summary>
  public string? CurrentEmployer { get; set; }

  /// <summary>
  /// Current job title
  /// </summary>
  public string? CurrentJobTitle { get; set; }

  /// <summary>
  /// Industry or field of work
  /// </summary>
  public string? Industry { get; set; }

  /// <summary>
  /// LinkedIn profile URL
  /// </summary>
  public string? LinkedInProfile { get; set; }

  // ========================================
  // EDUCATION & PROFESSIONAL DEVELOPMENT
  // ========================================

  /// <summary>
  /// Qualifications attained (e.g., ["MASTERS", "BACHELORS"])
  /// </summary>
  public List<string> QualificationsAttained { get; set; } = new();

  /// <summary>
  /// Professional certifications (free text)
  /// </summary>
  public string? ProfessionalCertifications { get; set; }

  // ========================================
  // ALUMNI ENGAGEMENT
  // ========================================

  /// <summary>
  /// Areas of interest (e.g., ["MENTORSHIP", "NETWORKING"])
  /// </summary>
  public List<string> EngagementPreferences { get; set; } = new();

  // ========================================
  // CONSENT & VERIFICATION
  // ========================================

  /// <summary>
  /// Whether the alumni gave consent for data processing
  /// </summary>
  public bool ConsentGiven { get; set; }
}
