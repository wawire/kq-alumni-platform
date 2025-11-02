namespace KQAlumni.API.DTOs;

/// <summary>
/// Response for staff number duplicate check
/// </summary>
public record StaffNumberCheckResponse
{
    /// <summary>
    /// Whether the staff number already exists
    /// </summary>
    public bool Exists { get; init; }

    /// <summary>
    /// The staff number that was checked
    /// </summary>
    public string StaffNumber { get; init; } = string.Empty;
}

/// <summary>
/// Response for email duplicate check
/// </summary>
public record EmailCheckResponse
{
    /// <summary>
    /// Whether the email already exists
    /// </summary>
    public bool Exists { get; init; }

    /// <summary>
    /// The email that was checked
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
