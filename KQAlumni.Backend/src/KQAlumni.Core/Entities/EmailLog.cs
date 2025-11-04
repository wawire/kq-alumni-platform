using System.ComponentModel.DataAnnotations;

namespace KQAlumni.Core.Entities;

/// <summary>
/// Email delivery tracking log
/// </summary>
public class EmailLog
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Related registration ID (if applicable)
    /// </summary>
    public Guid? RegistrationId { get; set; }

    /// <summary>
    /// Recipient email address
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string ToEmail { get; set; } = string.Empty;

    /// <summary>
    /// Email subject
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email type: Confirmation, Approval, Rejection, Other
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EmailType { get; set; } = string.Empty;

    /// <summary>
    /// Delivery status: Sent, Failed, Queued, MockMode
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if failed
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// SMTP server used
    /// </summary>
    [MaxLength(256)]
    public string? SmtpServer { get; set; }

    /// <summary>
    /// When the email was sent/attempted
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time taken to send (milliseconds)
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Number of retry attempts (if applicable)
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    [MaxLength(1000)]
    public string? Metadata { get; set; }

    // Navigation property
    public AlumniRegistration? Registration { get; set; }
}

/// <summary>
/// Email delivery status constants
/// </summary>
public static class EmailStatus
{
    public const string Sent = "Sent";
    public const string Failed = "Failed";
    public const string Queued = "Queued";
    public const string MockMode = "MockMode";
}

/// <summary>
/// Email type constants
/// </summary>
public static class EmailType
{
    public const string Confirmation = "Confirmation";
    public const string Approval = "Approval";
    public const string Rejection = "Rejection";
    public const string Other = "Other";
}
