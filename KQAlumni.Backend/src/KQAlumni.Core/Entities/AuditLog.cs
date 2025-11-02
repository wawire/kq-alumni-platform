namespace KQAlumni.Core.Entities;

/// <summary>
/// Audit log for tracking all manual approval/rejection actions
/// Provides compliance and accountability
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the registration being acted upon
    /// </summary>
    public Guid RegistrationId { get; set; }

    /// <summary>
    /// Navigation property to the registration
    /// </summary>
    public virtual AlumniRegistration Registration { get; set; } = null!;

    /// <summary>
    /// Action performed: Approved, Rejected, Updated, Deleted, etc.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Email/username of the person who performed the action
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the admin user (if applicable)
    /// </summary>
    public int? AdminUserId { get; set; }

    /// <summary>
    /// Navigation property to the admin user
    /// </summary>
    public virtual AdminUser? AdminUser { get; set; }

    /// <summary>
    /// Additional notes or reason for the action
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reason for rejection (if action is Rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// When the action was performed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the admin user (for security)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Previous status (before action)
    /// </summary>
    public string? PreviousStatus { get; set; }

    /// <summary>
    /// New status (after action)
    /// </summary>
    public string? NewStatus { get; set; }

    /// <summary>
    /// Whether this was an automatic action or manual
    /// </summary>
    public bool IsAutomated { get; set; } = false;
}

/// <summary>
/// Audit action types
/// </summary>
public static class AuditActions
{
    public const string ManualApproval = "ManualApproval";
    public const string ManualRejection = "ManualRejection";
    public const string AutomaticApproval = "AutomaticApproval";
    public const string AutomaticRejection = "AutomaticRejection";
    public const string StatusUpdate = "StatusUpdate";
    public const string OverrideDecision = "OverrideDecision";
    public const string Deleted = "Deleted";
    public const string Updated = "Updated";
}
