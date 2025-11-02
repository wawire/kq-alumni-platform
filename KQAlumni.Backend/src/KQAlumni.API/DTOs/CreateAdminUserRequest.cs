using System.ComponentModel.DataAnnotations;

namespace KQAlumni.API.DTOs;

/// <summary>
/// Request DTO for creating a new admin user
/// </summary>
public class CreateAdminUserRequest
{
    /// <summary>
    /// Admin username (unique)
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Admin email (unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Admin full name
    /// </summary>
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Admin role: SuperAdmin, HRManager, HROfficer
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(SuperAdmin|HRManager|HROfficer)$", ErrorMessage = "Role must be SuperAdmin, HRManager, or HROfficer")]
    public string Role { get; set; } = "HROfficer";
}
