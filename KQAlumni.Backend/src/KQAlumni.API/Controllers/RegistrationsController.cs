using KQAlumni.API.DTOs;
using KQAlumni.Core.DTOs;
using KQAlumni.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KQAlumni.API.Controllers;

/// <summary>
/// Controller for managing alumni registrations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RegistrationsController : ControllerBase
{
  private readonly IRegistrationService _registrationService;
  private readonly ILogger<RegistrationsController> _logger;

  public RegistrationsController(
      IRegistrationService registrationService,
      ILogger<RegistrationsController> logger)
  {
    _registrationService = registrationService;
    _logger = logger;
  }

  /// <summary>
  /// Register a new alumni member
  /// </summary>
  /// <param name="request">Registration details</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Registration response with ID</returns>
  [HttpPost]
  [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<RegistrationResponse>> RegisterAlumni(
      [FromBody] RegistrationRequest request,
      CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Registration request received: {Email}", request.Email);
      var response = await _registrationService.RegisterAlumniAsync(request, cancellationToken);

      return CreatedAtAction(nameof(GetRegistration), new { id = response.Id }, response);
    }
    catch (InvalidOperationException ex)
    {
      _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);

      if (ex.Message.Contains("already registered"))
      {
        return Conflict(new ErrorResponse
        {
          Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
          Title = "Duplicate registration",
          Status = StatusCodes.Status409Conflict,
          Detail = ex.Message
        });
      }

      return BadRequest(new ErrorResponse
      {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        Title = "Registration failed",
        Status = StatusCodes.Status400BadRequest,
        Detail = ex.Message
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error during registration");
      throw;
    }
  }

  /// <summary>
  /// Get registration status by email
  /// </summary>
  /// <param name="email">Alumni email address</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Registration status details</returns>
  [HttpGet("status")]
  [ProducesResponseType(typeof(RegistrationStatusResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RegistrationStatusResponse>> GetRegistrationStatus(
      [FromQuery] string email,
      CancellationToken cancellationToken)
  {
    var registration = await _registrationService.GetRegistrationByEmailAsync(email, cancellationToken);

    if (registration == null)
    {
      return NotFound(new ErrorResponse
      {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        Title = "Registration not found",
        Status = StatusCodes.Status404NotFound,
        Detail = $"No registration found for email: {email}"
      });
    }

    return Ok(new RegistrationStatusResponse
    {
      Status = registration.RegistrationStatus,
      RegisteredAt = registration.CreatedAt,
      ApprovedAt = registration.ApprovedAt,
      EmailVerified = registration.EmailVerified,
      EmailVerifiedAt = registration.EmailVerifiedAt,
      FullName = registration.FullName
    });
  }

  /// <summary>
  /// Get registration by ID
  /// </summary>
  /// <param name="id">Registration unique identifier</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Complete registration details</returns>
  [HttpGet("{id:guid}", Name = nameof(GetRegistration))]
  [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RegistrationResponse>> GetRegistration(
      Guid id,
      CancellationToken cancellationToken)
  {
    var registration = await _registrationService.GetRegistrationByIdAsync(id, cancellationToken);

    if (registration == null)
    {
      return NotFound(new ErrorResponse
      {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        Title = "Registration not found",
        Status = StatusCodes.Status404NotFound,
        Detail = $"No registration found with ID: {id}"
      });
    }

    var response = new RegistrationResponse
    {
      Id = registration.Id,
      RegistrationNumber = registration.RegistrationNumber,
      StaffNumber = registration.StaffNumber,
      FullName = registration.FullName,
      Email = registration.Email,
      Mobile = registration.FullMobile,
      Status = registration.RegistrationStatus,
      RegisteredAt = registration.CreatedAt,
      Message = "Registration retrieved successfully"
    };

    return Ok(response);
  }

  /// <summary>
  /// Check if staff number is already registered
  /// </summary>
  /// <param name="staffNumber">Staff number to check</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Response containing existence flag and staff number</returns>
  [HttpGet("check/staff-number/{staffNumber}")]
  [ProducesResponseType(typeof(StaffNumberCheckResponse), StatusCodes.Status200OK)]
  public async Task<ActionResult<StaffNumberCheckResponse>> CheckStaffNumber(
      string staffNumber,
      CancellationToken cancellationToken)
  {
    var exists = await _registrationService.IsStaffNumberRegisteredAsync(staffNumber, cancellationToken);
    return Ok(new StaffNumberCheckResponse
    {
      Exists = exists,
      StaffNumber = staffNumber
    });
  }

  /// <summary>
  /// Check if email is already registered
  /// </summary>
  /// <param name="email">Email address to check</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Response containing existence flag and email</returns>
  [HttpGet("check/email/{email}")]
  [ProducesResponseType(typeof(EmailCheckResponse), StatusCodes.Status200OK)]
  public async Task<ActionResult<EmailCheckResponse>> CheckEmail(
      string email,
      CancellationToken cancellationToken)
  {
    var exists = await _registrationService.IsEmailRegisteredAsync(email, cancellationToken);
    return Ok(new EmailCheckResponse
    {
      Exists = exists,
      Email = email
    });
  }

  /// <summary>
  /// Verify email using token from approval email
  /// </summary>
  /// <param name="token">Email verification token</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Redirect to dashboard on success</returns>
  [HttpGet("verify/{token}")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> VerifyEmail(
      string token,
      CancellationToken cancellationToken)
  {
    try
    {
      var result = await _registrationService.VerifyEmailAsync(token, cancellationToken);

      if (!result.Success)
      {
        return BadRequest(new ErrorResponse
        {
          Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          Title = "Email Verification Failed",
          Status = StatusCodes.Status400BadRequest,
          Detail = result.Message
        });
      }

      _logger.LogInformation("Email verified successfully for: {Email}", result.Email);

      // Redirect to frontend dashboard
      return Redirect($"/dashboard?verified=true");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during email verification");
      return StatusCode(500, new ErrorResponse
      {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        Title = "Verification Error",
        Status = StatusCodes.Status500InternalServerError,
        Detail = "An unexpected error occurred while verifying your email"
      });
    }
  }

  /// <summary>
  /// Verify ID or Passport number in real-time during registration
  /// Returns staff number and name if found in ERP
  /// </summary>
  /// <param name="idOrPassport">National ID or Passport number</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Verification response with staff details</returns>
  [HttpGet("verify-id/{idOrPassport}")]
  [ProducesResponseType(typeof(Core.DTOs.IdVerificationResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<Core.DTOs.IdVerificationResponse>> VerifyIdOrPassport(
      string idOrPassport,
      CancellationToken cancellationToken)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(idOrPassport))
      {
        return BadRequest(new ErrorResponse
        {
          Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
          Title = "Invalid Request",
          Status = StatusCodes.Status400BadRequest,
          Detail = "ID or Passport number is required"
        });
      }

      var result = await _registrationService.VerifyIdOrPassportAsync(
          idOrPassport.Trim().ToUpper(),
          cancellationToken);

      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error verifying ID/Passport: {IdOrPassport}", idOrPassport);
      return StatusCode(500, new ErrorResponse
      {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        Title = "Verification Error",
        Status = StatusCodes.Status500InternalServerError,
        Detail = "An unexpected error occurred during verification. Please try again."
      });
    }
  }
}
