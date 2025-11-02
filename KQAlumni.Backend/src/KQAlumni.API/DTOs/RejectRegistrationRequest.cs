using System.ComponentModel.DataAnnotations;

namespace KQAlumni.API.DTOs;

/// <summary>
/// Request DTO for manually rejecting a registration
/// </summary>
public class RejectRegistrationRequest
{
    /// <summary>
    /// Reason for rejection (required)
    /// </summary>
    [Required(ErrorMessage = "Rejection reason is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10 and 500 characters")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the rejection
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
