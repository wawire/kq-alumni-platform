using KQAlumni.Core.DTOs;
using KQAlumni.Core.Enums;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KQAlumni.API.Controllers;

/// <summary>
/// Controller for handling email verification workflow
///
/// RESPONSIBILITY: Single point of truth for email verification operations
/// - Verify email tokens from approval emails
/// - Check registration status
/// - Handle verification expiry and retry logic
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class VerificationController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly ITokenService _tokenService;
  private readonly ILogger<VerificationController> _logger;

  public VerificationController(
      AppDbContext context,
      ITokenService tokenService,
      ILogger<VerificationController> logger)
  {
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Verifies email using token from approval email
  ///
  /// FLOW:
  /// 1. Validates token format
  /// 2. Retrieves registration record
  /// 3. Checks token expiry
  /// 4. Marks email as verified
  /// 5. Updates registration status to Active
  /// 6. Clears token for one-time use
  /// 7. Redirects to dashboard
  /// </summary>
  /// <param name="token">Verification token (32-character alphanumeric)</param>
  /// <returns>Redirect to dashboard or error response</returns>
  /// <response code="302">Email verified successfully, redirects to dashboard</response>
  /// <response code="400">Invalid or expired token</response>
  /// <response code="500">Internal server error during verification</response>
  [HttpGet("verify/{token}")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> VerifyEmail(string token)
  {
    try
    {
      // Step 1: Validate token format
      if (string.IsNullOrWhiteSpace(token) || !_tokenService.ValidateTokenFormat(token))
      {
        _logger.LogWarning("Invalid token format: {Token}", token);
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Invalid Verification Token",
          Detail = "The verification token format is invalid."
        });
      }

      // Step 2: Find registration by token
      var registration = await _context.AlumniRegistrations
          .FirstOrDefaultAsync(r => r.EmailVerificationToken == token);

      if (registration == null)
      {
        _logger.LogWarning("Token not found: {Token}", token);
        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Invalid Verification Token",
          Detail = "This verification token does not exist or has already been used."
        });
      }

      // Step 3: Check if token expired
      if (registration.EmailVerificationTokenExpiry.HasValue &&
          registration.EmailVerificationTokenExpiry < DateTime.UtcNow)
      {
        _logger.LogWarning(
            "Token expired for registration {Id}. Expired at: {Expiry}",
            registration.Id,
            registration.EmailVerificationTokenExpiry);

        return BadRequest(new ProblemDetails
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Verification Link Expired",
          Detail = "This verification link has expired. Please contact KQ.Alumni@kenya-airways.com for assistance."
        });
      }

      // Step 4: Check if already verified
      if (registration.EmailVerified)
      {
        _logger.LogInformation(
            "Email already verified for registration {Id}. Redirecting to dashboard.",
            registration.Id);

        return Redirect($"/dashboard?id={registration.Id}");
      }

      // Step 5: Mark as verified
      registration.EmailVerified = true;
      registration.EmailVerifiedAt = DateTime.UtcNow;
      registration.RegistrationStatus = RegistrationStatus.Active.ToString();
      registration.EmailVerificationToken = null; // One-time use
      registration.EmailVerificationTokenExpiry = null;
      registration.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      _logger.LogInformation(
          "Email verified successfully for registration {Id} ({StaffNumber})",
          registration.Id,
          registration.StaffNumber);

      // Redirect to dashboard with verification success indicator
      return Redirect($"/dashboard?id={registration.Id}&verified=true");
    }
    catch (DbUpdateException dbEx)
    {
      _logger.LogError(dbEx, "Database error during email verification with token: {Token}", token);
      return StatusCode(
          StatusCodes.Status500InternalServerError,
          new ProblemDetails
          {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Database Error",
            Detail = "An error occurred while verifying your email. Please try again or contact support."
          });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error verifying email with token: {Token}", token);
      return StatusCode(
          StatusCodes.Status500InternalServerError,
          new ProblemDetails
          {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Verification Error",
            Detail = "An unexpected error occurred while verifying your email. Please try again or contact support."
          });
    }
  }

  /// <summary>
  /// Check registration status by ID
  ///
  /// Provides detailed status information including:
  /// - Current registration state (Pending, Approved, Active, Rejected)
  /// - Timestamps for key lifecycle events
  /// - User-friendly status message
  /// </summary>
  /// <param name="id">Registration unique identifier</param>
  /// <returns>Registration status with lifecycle information</returns>
  /// <response code="200">Registration status retrieved successfully</response>
  /// <response code="404">Registration not found</response>
  [HttpGet("registrations/{id:guid}/status")]
  [ProducesResponseType(typeof(RegistrationStatusDetailResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetRegistrationStatus(Guid id)
  {
    try
    {
      var registration = await _context.AlumniRegistrations
          .AsNoTracking()
          .Where(r => r.Id == id)
          .Select(r => new RegistrationStatusDetailResponse
          {
            Id = r.Id,
            StaffNumber = r.StaffNumber,
            FullName = r.FullName,
            Email = r.Email,
            Status = r.RegistrationStatus,
            CreatedAt = r.CreatedAt,
            ApprovedAt = r.ApprovedAt,
            EmailVerifiedAt = r.EmailVerifiedAt,
            RejectedAt = r.RejectedAt,
            EmailVerified = r.EmailVerified,
            Message = GetStatusMessage(r.RegistrationStatus, r.EmailVerified)
          })
          .FirstOrDefaultAsync();

      if (registration == null)
      {
        _logger.LogWarning("Registration not found: {Id}", id);
        return NotFound(new ProblemDetails
        {
          Status = StatusCodes.Status404NotFound,
          Title = "Registration Not Found",
          Detail = $"No registration found with ID: {id}"
        });
      }

      return Ok(registration);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving registration status for ID: {Id}", id);
      return StatusCode(
          StatusCodes.Status500InternalServerError,
          new ProblemDetails
          {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Status Check Error",
            Detail = "An error occurred while checking registration status."
          });
    }
  }

  /// <summary>
  /// Helper method to generate user-friendly status messages
  /// </summary>
  private static string GetStatusMessage(string registrationStatus, bool emailVerified) =>
      registrationStatus switch
      {
        nameof(RegistrationStatus.Pending) =>
            "Your registration is being reviewed. Check your email for updates.",
        nameof(RegistrationStatus.Approved) =>
            "Registration approved! Check your email for the verification link.",
        nameof(RegistrationStatus.Active) when emailVerified =>
            "Email verified! Your registration is complete.",
        nameof(RegistrationStatus.Active) =>
            "Your registration is active. Please verify your email.",
        nameof(RegistrationStatus.Rejected) =>
            "Registration rejected. Please contact HR at KQ.Alumni@kenya-airways.com",
        _ => "Unknown registration status. Please contact support."
      };
}

/// <summary>
/// DTO for registration status detail response
///
/// Encapsulates all registration lifecycle information needed by clients
/// </summary>
public record RegistrationStatusDetailResponse
{
  /// <summary>Unique registration identifier</summary>
  public required Guid Id { get; init; }

  /// <summary>Staff number used for registration</summary>
  public required string StaffNumber { get; init; }

  /// <summary>Alumnus full name</summary>
  public required string FullName { get; init; }

  /// <summary>Email address</summary>
  public required string Email { get; init; }

  /// <summary>Current registration status (Pending, Approved, Active, Rejected)</summary>
  public required string Status { get; init; }

  /// <summary>Email verification flag</summary>
  public required bool EmailVerified { get; init; }

  /// <summary>Timestamp when registration was created</summary>
  public required DateTime CreatedAt { get; init; }

  /// <summary>Timestamp when registration was approved (nullable)</summary>
  public DateTime? ApprovedAt { get; init; }

  /// <summary>Timestamp when email was verified (nullable)</summary>
  public DateTime? EmailVerifiedAt { get; init; }

  /// <summary>Timestamp when registration was rejected (nullable)</summary>
  public DateTime? RejectedAt { get; init; }

  /// <summary>User-friendly status message based on current state</summary>
  public required string Message { get; init; }
}
