using KQAlumni.Core.DTOs;
using KQAlumni.Core.Entities;

namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for managing alumni registrations
/// Orchestrates validation, ERP checks, database persistence, and email notifications
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers a new alumni member
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration response with alumni details</returns>
    Task<RegistrationResponse> RegisterAlumniAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if staff number is already registered
    /// </summary>
    /// <param name="staffNumber">Staff number to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if already registered</returns>
    Task<bool> IsStaffNumberRegisteredAsync(
        string staffNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if email is already registered
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if already registered</returns>
    Task<bool> IsEmailRegisteredAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if mobile number is already registered
    /// </summary>
    /// <param name="mobileCountryCode">Country code (e.g., +254)</param>
    /// <param name="mobileNumber">Phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if already registered</returns>
    Task<bool> IsMobileRegisteredAsync(
        string? mobileCountryCode,
        string? mobileNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if LinkedIn profile is already registered
    /// </summary>
    /// <param name="linkedInProfile">LinkedIn profile URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if already registered</returns>
    Task<bool> IsLinkedInRegisteredAsync(
        string? linkedInProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets registration by ID
    /// </summary>
    /// <param name="id">Registration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Alumni registration or null</returns>
    Task<AlumniRegistration?> GetRegistrationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify email using token from approval email
    /// </summary>
    /// <param name="token">Email verification token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    Task<EmailVerificationResult> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get registration by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Alumni registration or null</returns>
    Task<AlumniRegistration?> GetRegistrationByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
