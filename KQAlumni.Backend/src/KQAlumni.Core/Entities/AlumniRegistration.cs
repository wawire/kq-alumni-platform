using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KQAlumni.Core.Entities;

/// <summary>
/// Represents an alumni registration in the KQ Alumni Association system.
/// This entity stores all information collected during the registration process.
/// </summary>
public class AlumniRegistration
{
  /// <summary>
  /// Unique identifier for the registration (GUID/UUID)
  /// </summary>
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
  /// User-friendly registration number (e.g., KQA-2024-00001)
  /// Generated automatically on registration
  /// Format: KQA-{year}-{sequential_number}
  /// </summary>
  [Required]
  [StringLength(20)]
  [Column(TypeName = "varchar(20)")]
  public string RegistrationNumber { get; set; } = string.Empty;

  /// <summary>
  /// Staff number (7 characters): 00XXXXX (any alphanumeric after 00)
  /// Examples: 0007601, 00C5050, 00RG002, 00PW057
  /// Optional during registration - will be auto-populated from ERP validation using ID/Passport
  /// </summary>
  [StringLength(7, MinimumLength = 7)]
  [Column(TypeName = "varchar(7)")]
  public string? StaffNumber { get; set; }

  /// <summary>
  /// National ID number or Passport number (Required during registration)
  /// Used for ERP validation and verification
  /// Can contain either Kenyan ID or international passport number
  /// </summary>
  [Required]
  [StringLength(50)]
  [Column(TypeName = "varchar(50)")]
  public string IdNumber { get; set; } = string.Empty;

  /// <summary>
  /// Passport number (Optional - legacy field for compatibility)
  /// </summary>
  [StringLength(50)]
  [Column(TypeName = "varchar(50)")]
  public string? PassportNumber { get; set; }

  /// <summary>
  /// Full name as per company records
  /// </summary>
  [Required]
  [StringLength(200, MinimumLength = 2)]
  [Column(TypeName = "nvarchar(200)")]
  public string FullName { get; set; } = string.Empty;

  /// <summary>
  /// Current email address for communication
  /// </summary>
  [Required]
  [EmailAddress]
  [StringLength(255)]
  [Column(TypeName = "nvarchar(255)")]
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Country code (e.g., +254)
  /// </summary>
  [StringLength(5)]
  [Column(TypeName = "varchar(5)")]
  public string? MobileCountryCode { get; set; }

  /// <summary>
  /// Mobile number without country code (e.g., 712345678)
  /// </summary>
  [StringLength(15)]
  [Column(TypeName = "varchar(15)")]
  public string? MobileNumber { get; set; }

  /// <summary>
  /// Computed full mobile number (computed in code, not stored in database)
  /// This property is calculated at runtime and not persisted
  /// </summary>
  [NotMapped]
  public string? FullMobile =>
      !string.IsNullOrEmpty(MobileCountryCode) && !string.IsNullOrEmpty(MobileNumber)
          ? $"{MobileCountryCode}{MobileNumber}"
          : null;

  /// <summary>
  /// Current country of residence (full name, e.g., "Kenya")
  /// </summary>
  [Required]
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string CurrentCountry { get; set; } = string.Empty;

  /// <summary>
  /// Current country code (ISO 3166-1 Alpha-2, e.g., "KE")
  /// </summary>
  [Required]
  [StringLength(2)]
  [Column(TypeName = "varchar(2)")]
  public string CurrentCountryCode { get; set; } = string.Empty;

  /// <summary>
  /// Current city of residence
  /// </summary>
  [Required]
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string CurrentCity { get; set; } = string.Empty;

  /// <summary>
  /// Custom city name if "Other" was selected
  /// </summary>
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string? CityCustom { get; set; }

  /// <summary>
  /// Current employer name
  /// </summary>
  [StringLength(200)]
  [Column(TypeName = "nvarchar(200)")]
  public string? CurrentEmployer { get; set; }

  /// <summary>
  /// Current job title
  /// </summary>
  [StringLength(200)]
  [Column(TypeName = "nvarchar(200)")]
  public string? CurrentJobTitle { get; set; }

  /// <summary>
  /// Industry or field of work
  /// </summary>
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string? Industry { get; set; }

  /// <summary>
  /// LinkedIn profile URL (Optional - nullable)
  /// NOTE: Unique index filters out NULL values, allowing multiple registrations without LinkedIn
  /// </summary>
  [StringLength(500)]
  [Column(TypeName = "nvarchar(500)")]
  [Url]
  public string? LinkedInProfile { get; set; } = null; // ✅ Explicitly nullable with default null

  /// <summary>
  /// Qualifications attained (stored as JSON array: ["MASTERS", "BACHELORS"])
  /// </summary>
  [Required]
  [Column(TypeName = "nvarchar(max)")]
  public string QualificationsAttained { get; set; } = "[]";

  /// <summary>
  /// Professional certifications (free text)
  /// </summary>
  [StringLength(1000)]
  [Column(TypeName = "nvarchar(1000)")]
  public string? ProfessionalCertifications { get; set; }

  /// <summary>
  /// Areas of interest (stored as JSON array: ["MENTORSHIP", "NETWORKING"])
  /// </summary>
  [Required]
  [Column(TypeName = "nvarchar(max)")]
  public string EngagementPreferences { get; set; } = "[]";

  /// <summary>
  /// Whether the alumni gave consent for data processing
  /// </summary>
  [Required]
  public bool ConsentGiven { get; set; }

  /// <summary>
  /// Timestamp when consent was given
  /// </summary>
  public DateTime? ConsentGivenAt { get; set; }

  /// <summary>
  /// Email verification token (32-character alphanumeric)
  /// </summary>
  [StringLength(500)]
  [Column(TypeName = "nvarchar(500)")]
  public string? EmailVerificationToken { get; set; }

  /// <summary>
  /// When the verification token expires (30 days from generation)
  /// </summary>
  public DateTime? EmailVerificationTokenExpiry { get; set; }

  /// <summary>
  /// Whether the email has been verified (user clicked link)
  /// </summary>
  public bool EmailVerified { get; set; }

  /// <summary>
  /// When the email was verified
  /// </summary>
  public DateTime? EmailVerifiedAt { get; set; }

  /// <summary>
  /// Whether the staff number was validated against ERP
  /// </summary>
  public bool ErpValidated { get; set; }

  /// <summary>
  /// Timestamp of successful ERP validation
  /// </summary>
  public DateTime? ErpValidatedAt { get; set; }

  /// <summary>
  /// Number of ERP validation attempts (for retry logic)
  /// </summary>
  public int? ErpValidationAttempts { get; set; }

  /// <summary>
  /// Last ERP validation attempt timestamp
  /// </summary>
  public DateTime? LastErpValidationAttempt { get; set; }

  /// <summary>
  /// Staff name from ERP records (for cross-verification)
  /// </summary>
  [StringLength(200)]
  [Column(TypeName = "nvarchar(200)")]
  public string? ErpStaffName { get; set; }

  /// <summary>
  /// Department from ERP records
  /// </summary>
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string? ErpDepartment { get; set; }

  /// <summary>
  /// Exit date from ERP records
  /// </summary>
  [Column(TypeName = "date")]
  public DateTime? ErpExitDate { get; set; }

  /// <summary>
  /// Registration status: Pending | Approved | Active | Rejected
  /// </summary>
  [Required]
  [StringLength(50)]
  [Column(TypeName = "nvarchar(50)")]
  public string RegistrationStatus { get; set; } = "Pending";

  /// <summary>
  /// When the registration was approved (ERP validated)
  /// </summary>
  public DateTime? ApprovedAt { get; set; }

  /// <summary>
  /// When the registration was rejected (ERP validation failed after 5 attempts)
  /// </summary>
  public DateTime? RejectedAt { get; set; }

  /// <summary>
  /// Reason for rejection
  /// </summary>
  [StringLength(500)]
  [Column(TypeName = "nvarchar(500)")]
  public string? RejectionReason { get; set; }

  /// <summary>
  /// Whether this registration requires manual HR review
  /// Set to true when automatic ERP validation fails
  /// </summary>
  public bool RequiresManualReview { get; set; } = false;

  /// <summary>
  /// Reason why manual review is required
  /// </summary>
  [StringLength(500)]
  [Column(TypeName = "nvarchar(500)")]
  public string? ManualReviewReason { get; set; }

  /// <summary>
  /// Whether this registration has been manually reviewed by HR
  /// </summary>
  public bool ManuallyReviewed { get; set; } = false;

  /// <summary>
  /// Email/username of the HR person who reviewed
  /// </summary>
  [StringLength(200)]
  [Column(TypeName = "nvarchar(200)")]
  public string? ReviewedBy { get; set; }

  /// <summary>
  /// When the manual review was performed
  /// </summary>
  public DateTime? ReviewedAt { get; set; }

  /// <summary>
  /// Notes from the manual review
  /// </summary>
  [StringLength(1000)]
  [Column(TypeName = "nvarchar(1000)")]
  public string? ReviewNotes { get; set; }

  /// <summary>
  /// Navigation property: Audit logs for this registration
  /// </summary>
  public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

  /// <summary>
  /// Whether confirmation email was sent (Email 1: Registration Received)
  /// </summary>
  public bool ConfirmationEmailSent { get; set; }

  /// <summary>
  /// When confirmation email was sent
  /// </summary>
  public DateTime? ConfirmationEmailSentAt { get; set; }

  /// <summary>
  /// Whether approval email was sent (Email 2: Welcome + Verification Link)
  /// </summary>
  public bool ApprovalEmailSent { get; set; }

  /// <summary>
  /// When approval email was sent
  /// </summary>
  public DateTime? ApprovalEmailSentAt { get; set; }

  /// <summary>
  /// Whether rejection email was sent (Email 3: Unable to Verify)
  /// </summary>
  public bool RejectionEmailSent { get; set; }

  /// <summary>
  /// When rejection email was sent
  /// </summary>
  public DateTime? RejectionEmailSentAt { get; set; }

  /// <summary>
  /// When the registration was created (UTC)
  /// </summary>
  [Required]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// When the registration was last updated (UTC)
  /// </summary>
  [Required]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Who created the registration (system user ID or "System")
  /// </summary>
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string? CreatedBy { get; set; } = "System";

  /// <summary>
  /// Who last updated the registration
  /// </summary>
  [StringLength(100)]
  [Column(TypeName = "nvarchar(100)")]
  public string? UpdatedBy { get; set; }
}
