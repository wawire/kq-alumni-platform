using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KQAlumni.Core.Entities;

/// <summary>
/// Email template entity for customizable email content
/// Stores HTML templates with variable placeholders
/// </summary>
public class EmailTemplate
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Template type/key (Confirmation, Approval, Rejection)
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Template name for display
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Email subject with variable placeholders
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML email body with variable placeholders
    /// Variables: {{alumniName}}, {{registrationNumber}}, {{staffNumber}},
    /// {{verificationLink}}, {{rejectionReason}}, {{registrationId}}
    /// </summary>
    [Required]
    [Column(TypeName = "ntext")]
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Available variables for this template (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? AvailableVariables { get; set; }

    /// <summary>
    /// Whether this is the active template
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a system default template (cannot be deleted)
    /// </summary>
    public bool IsSystemDefault { get; set; } = false;

    /// <summary>
    /// When the template was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the template was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the template
    /// </summary>
    [StringLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the template
    /// </summary>
    [StringLength(100)]
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Template keys enum for type safety
/// </summary>
public static class EmailTemplateKeys
{
    public const string Confirmation = "CONFIRMATION";
    public const string Approval = "APPROVAL";
    public const string Rejection = "REJECTION";

    public static readonly string[] All = { Confirmation, Approval, Rejection };
}
