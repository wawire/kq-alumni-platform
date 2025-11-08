using System.Text.Json;
using KQAlumni.Core.DTOs;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Enums;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for managing alumni registrations
/// Implements the async approval workflow:
/// 1. Register → Save as Pending → Send confirmation email (immediate)
/// 2. Background job validates ERP → Send approval email (async)
/// 3. User clicks link → Email verified → Active status
/// </summary>
public class RegistrationService : IRegistrationService
{
  private readonly AppDbContext _context;
  private readonly IErpService _erpService;
  private readonly IEmailService _emailService;
  private readonly ILogger<RegistrationService> _logger;

  public RegistrationService(
      AppDbContext context,
      IErpService erpService,
      IEmailService emailService,
      ILogger<RegistrationService> logger)
  {
    _context = context;
    _erpService = erpService;
    _emailService = emailService;
    _logger = logger;
  }

  /// <summary>
  /// Registers a new alumni member
  /// NEW WORKFLOW: Saves as Pending, sends confirmation email, returns immediately
  /// Background job will handle ERP validation asynchronously
  /// </summary>
  public async Task<RegistrationResponse> RegisterAlumniAsync(
      RegistrationRequest request,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation(
          "Starting registration for staff number: {StaffNumber}",
          request.StaffNumber);


      // DUPLICATE CHECKS


      // Check: ID Number / Passport (always required)
      var existingByIdNumber = await IsIdNumberRegisteredAsync(
          request.IdNumber,
          cancellationToken);

      if (existingByIdNumber)
      {
        _logger.LogWarning(
            "ID/Passport number {IdNumber} already registered",
            request.IdNumber);

        throw new InvalidOperationException("This ID/Passport number is already registered");
      }

      // Check: Staff Number (only if provided)
      if (!string.IsNullOrWhiteSpace(request.StaffNumber))
      {
        var existingByStaffNumber = await IsStaffNumberRegisteredAsync(
            request.StaffNumber,
            cancellationToken);

        if (existingByStaffNumber)
        {
          _logger.LogWarning(
              "Staff number {StaffNumber} already registered",
              request.StaffNumber);

          throw new InvalidOperationException("This staff number is already registered");
        }
      }

      // Check: Email
      var existingByEmail = await IsEmailRegisteredAsync(
          request.Email,
          cancellationToken);

      if (existingByEmail)
      {
        _logger.LogWarning(
            "Email {Email} already registered",
            request.Email);

        throw new InvalidOperationException("This email address is already registered");
      }

      // Check: Mobile Number (if provided)
      var existingByMobile = await IsMobileRegisteredAsync(
          request.MobileCountryCode,
          request.MobileNumber,
          cancellationToken);

      if (existingByMobile)
      {
        _logger.LogWarning(
            "Mobile number {Mobile} already registered",
            $"{request.MobileCountryCode}{request.MobileNumber}");

        throw new InvalidOperationException("This mobile number is already registered");
      }

      // Check: LinkedIn Profile (if provided)
      var existingByLinkedIn = await IsLinkedInRegisteredAsync(
          request.LinkedInProfile,
          cancellationToken);

      if (existingByLinkedIn)
      {
        _logger.LogWarning(
            "LinkedIn profile {LinkedIn} already registered",
            request.LinkedInProfile);

        throw new InvalidOperationException("This LinkedIn profile is already registered");
      }


      // CREATE REGISTRATION

      // Generate user-friendly registration number
      var registrationNumber = await GenerateRegistrationNumberAsync(cancellationToken);

      var registration = new AlumniRegistration
      {
        Id = Guid.NewGuid(),
        RegistrationNumber = registrationNumber,

        // Personal Information
        StaffNumber = request.StaffNumber?.ToUpper().Trim(),
        IdNumber = request.IdNumber.Trim(),
        PassportNumber = string.IsNullOrWhiteSpace(request.PassportNumber) ? null : request.PassportNumber.Trim(),
        FullName = request.FullName.Trim(),

        // Contact Information
        Email = request.Email.ToLower().Trim(),
        MobileCountryCode = string.IsNullOrWhiteSpace(request.MobileCountryCode) ? null : request.MobileCountryCode.Trim(),
        MobileNumber = string.IsNullOrWhiteSpace(request.MobileNumber) ? null : request.MobileNumber.Trim(),
        CurrentCountry = request.CurrentCountry.Trim(),
        CurrentCountryCode = request.CurrentCountryCode.ToUpper().Trim(),
        CurrentCity = request.CurrentCity.Trim(),
        CityCustom = string.IsNullOrWhiteSpace(request.CityCustom) ? null : request.CityCustom.Trim(),

        // Employment Information
        CurrentEmployer = string.IsNullOrWhiteSpace(request.CurrentEmployer) ? null : request.CurrentEmployer.Trim(),
        CurrentJobTitle = string.IsNullOrWhiteSpace(request.CurrentJobTitle) ? null : request.CurrentJobTitle.Trim(),
        Industry = string.IsNullOrWhiteSpace(request.Industry) ? null : request.Industry.Trim(),
        LinkedInProfile = string.IsNullOrWhiteSpace(request.LinkedInProfile) ? null : request.LinkedInProfile.Trim(),

        // Education
        QualificationsAttained = JsonSerializer.Serialize(request.QualificationsAttained),
        ProfessionalCertifications = string.IsNullOrWhiteSpace(request.ProfessionalCertifications) ? null : request.ProfessionalCertifications.Trim(),

        // Engagement
        EngagementPreferences = JsonSerializer.Serialize(request.EngagementPreferences),

        // Consent
        ConsentGiven = request.ConsentGiven,
        ConsentGivenAt = DateTime.UtcNow,

        // Manual Review Flags (NEW: ERP Fallback Mode)
        RequiresManualReview = request.RequiresManualReview ?? false,
        ManualReviewReason = request.ManualReviewReason,

        // Status (NEW WORKFLOW: Start as Pending)
        RegistrationStatus = RegistrationStatus.Pending.ToString(),

        // Audit
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        CreatedBy = "System"
      };

      // Save to database
      await _context.AlumniRegistrations.AddAsync(registration, cancellationToken);
      await _context.SaveChangesAsync(cancellationToken);

      _logger.LogInformation(
          "Registration created with ID {Id} and status Pending",
          registration.Id);


      // SEND CONFIRMATION EMAIL


      try
      {
        var emailSent = await _emailService.SendConfirmationEmailAsync(
            registration.FullName,
            registration.Email,
            registration.Id,
            cancellationToken);

        if (emailSent)
        {
          registration.ConfirmationEmailSent = true;
          registration.ConfirmationEmailSentAt = DateTime.UtcNow;
          await _context.SaveChangesAsync(cancellationToken);

          _logger.LogInformation(
              "Confirmation email sent to {Email}",
              registration.Email);
        }
        else
        {
          _logger.LogWarning(
              "Failed to send confirmation email to {Email}",
              registration.Email);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex,
            "Error sending confirmation email to {Email}",
            registration.Email);
        // Don't fail registration if email fails
      }

      // Return response (background job will handle ERP validation)
      return new RegistrationResponse
      {
        Id = registration.Id,
        RegistrationNumber = registration.RegistrationNumber,
        StaffNumber = registration.StaffNumber,
        FullName = registration.FullName,
        Email = registration.Email,
        Status = registration.RegistrationStatus,
        RegisteredAt = registration.CreatedAt,
        Message = "Registration received! Check your email for confirmation. " +
                     "We will notify you within 24 hours once your registration is approved."
      };
    }
    catch (InvalidOperationException)
    {
      // Re-throw business logic exceptions
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error registering alumni");
      throw new InvalidOperationException(
          "An error occurred while processing your registration. Please try again later.",
          ex);
    }
  }

  /// <summary>
  /// Verify email using token from approval email
  /// </summary>
  public async Task<EmailVerificationResult> VerifyEmailAsync(
      string token,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Attempting to verify token: {Token}", token);

      // Find registration by token
      var registration = await _context.AlumniRegistrations
          .FirstOrDefaultAsync(
              r => r.EmailVerificationToken == token &&
                   r.RegistrationStatus == RegistrationStatus.Approved.ToString(),
              cancellationToken);

      if (registration == null)
      {
        _logger.LogWarning("Token not found or registration not approved: {Token}", token);

        return new EmailVerificationResult
        {
          Success = false,
          Message = "Invalid or expired verification token. Please contact support if you need assistance."
        };
      }

      // Check if token is expired (30 days from approval)
      if (registration.EmailVerificationTokenExpiry.HasValue &&
          registration.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
      {
        _logger.LogWarning(
            "Verification token expired for: {Email}",
            registration.Email);

        return new EmailVerificationResult
        {
          Success = false,
          Message = "Verification link has expired. Please contact KQ.Alumni@kenya-airways.com for a new link."
        };
      }

      // Check if already verified
      if (registration.EmailVerified)
      {
        _logger.LogInformation(
            "Email already verified for: {Email}",
            registration.Email);

        return new EmailVerificationResult
        {
          Success = true,
          Message = "Email already verified. Welcome back!",
          Email = registration.Email,
          FullName = registration.FullName
        };
      }

      // Mark as verified
      registration.EmailVerified = true;
      registration.EmailVerifiedAt = DateTime.UtcNow;
      registration.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync(cancellationToken);

      _logger.LogInformation(
          "[SUCCESS] Email verified successfully for: {Email}",
          registration.Email);

      return new EmailVerificationResult
      {
        Success = true,
        Message = "Email verified successfully! Welcome to the KQ Alumni family.",
        Email = registration.Email,
        FullName = registration.FullName
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error verifying email with token: {Token}", token);

      return new EmailVerificationResult
      {
        Success = false,
        Message = "An error occurred during verification. Please try again or contact support."
      };
    }
  }

  /// <summary>
  /// Get registration by email address
  /// </summary>
  public async Task<AlumniRegistration?> GetRegistrationByEmailAsync(
      string email,
      CancellationToken cancellationToken = default)
  {
    var normalized = email.ToLower().Trim();

    return await _context.AlumniRegistrations
        .Where(r => r.Email == normalized)
        .OrderByDescending(r => r.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
  }

  /// <summary>
  /// Checks if ID number or passport number is already registered
  /// </summary>
  public async Task<bool> IsIdNumberRegisteredAsync(
      string idNumber,
      CancellationToken cancellationToken = default)
  {
    var normalized = idNumber.ToUpper().Trim();
    return await _context.AlumniRegistrations
        .AnyAsync(r => r.IdNumber == normalized, cancellationToken);
  }

  /// <summary>
  /// Checks if staff number is already registered
  /// </summary>
  public async Task<bool> IsStaffNumberRegisteredAsync(
      string staffNumber,
      CancellationToken cancellationToken = default)
  {
    var normalized = staffNumber.ToUpper().Trim();
    return await _context.AlumniRegistrations
        .AnyAsync(r => r.StaffNumber == normalized, cancellationToken);
  }

  /// <summary>
  /// Checks if email is already registered
  /// </summary>
  public async Task<bool> IsEmailRegisteredAsync(
      string email,
      CancellationToken cancellationToken = default)
  {
    var normalized = email.ToLower().Trim();
    return await _context.AlumniRegistrations
        .AnyAsync(r => r.Email == normalized, cancellationToken);
  }

  /// <summary>
  /// Checks if mobile number is already registered
  /// </summary>
  public async Task<bool> IsMobileRegisteredAsync(
      string? mobileCountryCode,
      string? mobileNumber,
      CancellationToken cancellationToken = default)
  {
    // If either is null/empty, mobile is not being provided, so not a duplicate
    if (string.IsNullOrWhiteSpace(mobileCountryCode) || string.IsNullOrWhiteSpace(mobileNumber))
      return false;

    var normalizedCountryCode = mobileCountryCode.Trim();
    var normalizedNumber = mobileNumber.Trim();

    return await _context.AlumniRegistrations
        .AnyAsync(r =>
            r.MobileCountryCode == normalizedCountryCode &&
            r.MobileNumber == normalizedNumber,
            cancellationToken);
  }

  /// <summary>
  /// Checks if LinkedIn profile is already registered
  /// </summary>
  public async Task<bool> IsLinkedInRegisteredAsync(
      string? linkedInProfile,
      CancellationToken cancellationToken = default)
  {
    // If null or empty, LinkedIn is not being provided, so not a duplicate
    if (string.IsNullOrWhiteSpace(linkedInProfile))
      return false;

    var normalized = linkedInProfile.ToLower().Trim();

    return await _context.AlumniRegistrations
        .AnyAsync(r =>
            r.LinkedInProfile != null &&
            r.LinkedInProfile.ToLower() == normalized,
            cancellationToken);
  }

  /// <summary>
  /// Gets registration by ID
  /// </summary>
  public async Task<AlumniRegistration?> GetRegistrationByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    return await _context.AlumniRegistrations
        .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
  }

  /// <summary>
  /// Verify ID or Passport number in real-time against ERP
  /// Returns staff number and details if found
  /// Also checks if this ID/Passport is already registered
  /// </summary>
  public async Task<IdVerificationResponse> VerifyIdOrPassportAsync(
      string idOrPassport,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Verifying ID/Passport: {IdOrPassport}", idOrPassport);

      // Step 1: Check if this ID/Passport is already registered
      var existingRegistration = await _context.AlumniRegistrations
          .FirstOrDefaultAsync(r => r.IdNumber == idOrPassport, cancellationToken);

      if (existingRegistration != null)
      {
        _logger.LogWarning("ID/Passport {IdOrPassport} is already registered", idOrPassport);
        return new IdVerificationResponse
        {
          IsVerified = false,
          IsAlreadyRegistered = true,
          Message = "This ID/Passport number is already registered"
        };
      }

      // Step 2: Verify against ERP
      var erpResult = await _erpService.ValidateIdOrPassportAsync(idOrPassport, cancellationToken);

      if (!erpResult.IsValid)
      {
        _logger.LogWarning("ID/Passport {IdOrPassport} not found in ERP: {Error}",
            idOrPassport, erpResult.ErrorMessage);

        return new IdVerificationResponse
        {
          IsVerified = false,
          IsAlreadyRegistered = false,
          Message = erpResult.ErrorMessage ?? "ID/Passport number not found in our records"
        };
      }

      // Step 3: Return successful verification with ERP details
      _logger.LogInformation(
          "ID/Passport {IdOrPassport} verified successfully. Staff: {StaffNumber}, Name: {Name}",
          idOrPassport, erpResult.StaffNumber, erpResult.StaffName);

      return new IdVerificationResponse
      {
        IsVerified = true,
        IsAlreadyRegistered = false,
        StaffNumber = erpResult.StaffNumber,
        FullName = erpResult.StaffName,
        Department = erpResult.Department,
        ExitDate = erpResult.ExitDate,
        Message = "Verification successful"
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error verifying ID/Passport {IdOrPassport}", idOrPassport);
      return new IdVerificationResponse
      {
        IsVerified = false,
        IsAlreadyRegistered = false,
        Message = "An error occurred during verification. Please try again."
      };
    }
  }

  /// <summary>
  /// Resend verification email for an approved registration (Admin use - by ID)
  /// </summary>
  public async Task<bool> ResendVerificationEmailAsync(
      Guid registrationId,
      CancellationToken cancellationToken = default)
  {
    var registration = await _context.AlumniRegistrations
        .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

    if (registration == null)
    {
      _logger.LogWarning("Registration {RegistrationId} not found for email resend", registrationId);
      return false;
    }

    if (registration.RegistrationStatus != RegistrationStatus.Approved.ToString())
    {
      _logger.LogWarning("Cannot resend email for non-approved registration {RegistrationId}", registrationId);
      return false;
    }

    // Use existing verification token or generate new one if missing
    var token = registration.EmailVerificationToken;
    if (string.IsNullOrEmpty(token))
    {
      token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
      registration.EmailVerificationToken = token;
      registration.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(30);
      await _context.SaveChangesAsync(cancellationToken);
    }

    // Send approval email with verification link
    var emailSent = await _emailService.SendApprovalEmailAsync(
        registration.FullName,
        registration.Email,
        token,
        cancellationToken);

    if (emailSent)
    {
      registration.ApprovalEmailSent = true;
      registration.ApprovalEmailSentAt = DateTime.UtcNow;
      await _context.SaveChangesAsync(cancellationToken);

      _logger.LogInformation(
          "Verification email resent successfully for registration {RegistrationId}",
          registrationId);
    }
    else
    {
      _logger.LogError(
          "Failed to resend verification email for registration {RegistrationId}",
          registrationId);
    }

    return emailSent;
  }

  /// <summary>
  /// Resend verification email to an approved registration (User self-service - by email)
  /// </summary>
  public async Task<bool> ResendVerificationEmailAsync(
      string email,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Resending verification email to {Email}", email);

      // Find the registration by email
      var registration = await GetRegistrationByEmailAsync(email, cancellationToken);

      if (registration == null)
      {
        _logger.LogWarning("Registration not found for email {Email}", email);
        throw new InvalidOperationException("Registration not found");
      }

      // Check if registration is approved
      if (registration.RegistrationStatus != RegistrationStatus.Approved.ToString())
      {
        _logger.LogWarning(
            "Cannot resend verification email - registration status is {Status}",
            registration.RegistrationStatus);
        throw new InvalidOperationException(
            $"Cannot resend verification email. Registration status is {registration.RegistrationStatus}");
      }

      // Check if email is already verified
      if (registration.EmailVerified)
      {
        _logger.LogWarning("Email {Email} is already verified", email);
        throw new InvalidOperationException("Email is already verified");
      }

      // Check if token exists and is not expired
      if (string.IsNullOrEmpty(registration.EmailVerificationToken) ||
          !registration.EmailVerificationTokenExpiry.HasValue ||
          registration.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
      {
        _logger.LogInformation("Token expired or missing, generating new token for {Email}", email);

        // Generate new verification token (we can reuse the existing token generation approach)
        // Since we don't have ITokenService injected, we'll generate a simple secure token
        var newToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // 64 characters

        registration.EmailVerificationToken = newToken;
        registration.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(30);
        registration.UpdatedAt = DateTime.UtcNow;
        registration.UpdatedBy = "System (Resend Verification)";

        await _context.SaveChangesAsync(cancellationToken);
      }

      // Send approval email with verification link
      var emailSent = await _emailService.SendApprovalEmailAsync(
          registration.FullName,
          registration.Email,
          registration.EmailVerificationToken,
          cancellationToken);

      if (emailSent)
      {
        registration.ApprovalEmailSent = true;
        registration.ApprovalEmailSentAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification email resent successfully to {Email}", email);
        return true;
      }
      else
      {
        _logger.LogWarning("Failed to resend verification email to {Email}", email);
        return false;
      }
    }
    catch (InvalidOperationException)
    {
      throw; // Re-throw business logic exceptions
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error resending verification email to {Email}", email);
      throw new Exception("An error occurred while resending the verification email");
    }
  }

  /// <summary>
  /// Generates a unique user-friendly registration number
  /// Format: KQA-{year}-{sequential_number}
  /// Example: KQA-2024-00001
  /// </summary>
  private async Task<string> GenerateRegistrationNumberAsync(CancellationToken cancellationToken = default)
  {
    var currentYear = DateTime.UtcNow.Year;
    var prefix = $"KQA-{currentYear}-";

    // Get the latest registration number for this year
    var latestRegistration = await _context.AlumniRegistrations
        .Where(r => r.RegistrationNumber.StartsWith(prefix))
        .OrderByDescending(r => r.RegistrationNumber)
        .Select(r => r.RegistrationNumber)
        .FirstOrDefaultAsync(cancellationToken);

    int nextSequence = 1;

    if (!string.IsNullOrEmpty(latestRegistration))
    {
      // Extract the sequential number from the last registration
      // Format: KQA-2024-00001 -> extract "00001"
      var parts = latestRegistration.Split('-');
      if (parts.Length == 3 && int.TryParse(parts[2], out int lastSequence))
      {
        nextSequence = lastSequence + 1;
      }
    }

    // Format: KQA-2024-00001 (5 digits with leading zeros)
    return $"{prefix}{nextSequence:D5}";
  }

  /// <summary>
  /// Resend verification email for an approved registration
  /// </summary>
  public async Task<bool> ResendVerificationEmailAsync(
      Guid registrationId,
      CancellationToken cancellationToken = default)
  {
    var registration = await _context.AlumniRegistrations
        .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

    if (registration == null)
    {
      _logger.LogWarning("Registration {RegistrationId} not found for email resend", registrationId);
      return false;
    }

    if (registration.RegistrationStatus != "Approved")
    {
      _logger.LogWarning("Cannot resend email for non-approved registration {RegistrationId}", registrationId);
      return false;
    }

    // Use existing verification token or generate new one if missing
    var token = registration.EmailVerificationToken;
    if (string.IsNullOrEmpty(token))
    {
      token = Guid.NewGuid().ToString("N");
      registration.EmailVerificationToken = token;
      await _context.SaveChangesAsync(cancellationToken);
    }

    // Send approval email with verification link
    var emailSent = await _emailService.SendApprovalEmailAsync(
        registration.FullName,
        registration.Email,
        token,
        cancellationToken);

    if (emailSent)
    {
      _logger.LogInformation(
          "Verification email resent successfully for registration {RegistrationId}",
          registrationId);
    }
    else
    {
      _logger.LogError(
          "Failed to resend verification email for registration {RegistrationId}",
          registrationId);
    }

    return emailSent;
  }
}
