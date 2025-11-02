using System.ComponentModel.DataAnnotations;

namespace KQAlumni.API.DTOs;

/// <summary>
/// Request DTO for bulk approving multiple registrations
/// </summary>
public class BulkApprovalRequest
{
    /// <summary>
    /// List of registration IDs to approve
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one registration ID is required")]
    [MaxLength(100, ErrorMessage = "Cannot approve more than 100 registrations at once")]
    public List<Guid> RegistrationIds { get; set; } = new();

    /// <summary>
    /// Optional notes to apply to all approvals
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for bulk rejecting multiple registrations
/// </summary>
public class BulkRejectionRequest
{
    /// <summary>
    /// List of registration IDs to reject
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one registration ID is required")]
    [MaxLength(100, ErrorMessage = "Cannot reject more than 100 registrations at once")]
    public List<Guid> RegistrationIds { get; set; } = new();

    /// <summary>
    /// Reason for rejection (applies to all)
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes to apply to all rejections
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Response DTO for bulk operations
/// </summary>
public class BulkOperationResponse
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkOperationResult> Results { get; set; } = new();
}

/// <summary>
/// Individual result for a bulk operation
/// </summary>
public class BulkOperationResult
{
    public Guid RegistrationId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
