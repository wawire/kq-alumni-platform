using System.ComponentModel.DataAnnotations;

namespace KQAlumni.API.DTOs;

/// <summary>
/// Request DTO for manually approving a registration
/// </summary>
public class ApproveRegistrationRequest
{
    /// <summary>
    /// Optional notes about the approval
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
