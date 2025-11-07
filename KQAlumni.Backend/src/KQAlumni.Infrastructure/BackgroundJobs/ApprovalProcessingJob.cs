using KQAlumni.Core.Configuration;
using KQAlumni.Core.Entities;
using KQAlumni.Core.Enums;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KQAlumni.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire background job for processing pending alumni registrations
/// HYBRID WORKFLOW: Automatically approves valid registrations, flags exceptions for HR manual review
/// Smart scheduling: Every 2 minutes during business hours, less frequent off-hours
/// Validates staff numbers against ERP and sends approval emails
/// </summary>
public class ApprovalProcessingJob
{
  private readonly AppDbContext _context;
  private readonly IErpService _erpService;
  private readonly IEmailService _emailService;
  private readonly ITokenService _tokenService;
  private readonly ILogger<ApprovalProcessingJob> _logger;
  private readonly BackgroundJobSettings _jobSettings;

  public ApprovalProcessingJob(
      AppDbContext context,
      IErpService erpService,
      IEmailService emailService,
      ITokenService tokenService,
      ILogger<ApprovalProcessingJob> logger,
      IOptions<BackgroundJobSettings> jobSettings)
  {
    _context = context;
    _erpService = erpService;
    _emailService = emailService;
    _tokenService = tokenService;
    _logger = logger;
    _jobSettings = jobSettings.Value;
  }

  /// <summary>
  /// Processes pending registrations
  /// Called by Hangfire on configured schedule
  /// HYBRID WORKFLOW: Only processes registrations that haven't been manually reviewed or flagged
  /// </summary>
  public async Task ProcessPendingRegistrations()
  {
    var startTime = DateTime.UtcNow;
    _logger.LogInformation("Starting approval processing job at {Time}", startTime.ToString("MM/dd/yyyy HH:mm:ss"));

    try
    {
      // Get pending registrations (only process older than 1 second to avoid race conditions)
      // SKIP registrations that have been manually reviewed or require manual review
      var cutoffTime = DateTime.UtcNow.AddSeconds(-1);

      var pendingRegistrations = await _context.AlumniRegistrations
          .Where(r => r.RegistrationStatus == RegistrationStatus.Pending.ToString())
          .Where(r => r.CreatedAt <= cutoffTime)
          .Where(r => !r.ManuallyReviewed)           // SKIP manually-reviewed registrations
          .Where(r => !r.RequiresManualReview)        // SKIP registrations flagged for manual review
          .OrderBy(r => r.CreatedAt)
          .Take(_jobSettings.BatchSize) // Use configured batch size
          .ToListAsync();

      if (!pendingRegistrations.Any())
      {
        _logger.LogInformation("No pending registrations to process (excluding manually-reviewed/flagged registrations)");
        return;
      }

      _logger.LogInformation("Processing {Count} pending registrations (hybrid workflow - auto-processing only)", pendingRegistrations.Count);

      var processed = 0;
      var approved = 0;
      var flaggedForManualReview = 0;
      var retried = 0;

      foreach (var registration in pendingRegistrations)
      {
        var result = await ProcessSingleRegistration(registration);

        processed++;

        switch (result)
        {
          case ProcessingResult.Approved:
            approved++;
            break;
          case ProcessingResult.FlaggedForManualReview:
            flaggedForManualReview++;
            break;
          case ProcessingResult.Retry:
            retried++;
            break;
        }
      }

      await _context.SaveChangesAsync();

      var duration = (DateTime.UtcNow - startTime).TotalSeconds;
      _logger.LogInformation(
          "[SUCCESS] Completed approval processing job. Processed {Processed} registrations " +
          "({Approved} auto-approved, {Flagged} flagged for HR review, {Retried} retried) in {Duration:F2}s",
          processed,
          approved,
          flaggedForManualReview,
          retried,
          duration);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in approval processing job");
      throw; // Hangfire will handle retry
    }
  }

  /// <summary>
  /// Processes a single registration
  /// Implements exponential backoff retry strategy
  /// </summary>
  private async Task<ProcessingResult> ProcessSingleRegistration(Core.Entities.AlumniRegistration registration)
  {
    try
    {
      // Initialize retry tracking if not set
      if (!registration.ErpValidationAttempts.HasValue)
      {
        registration.ErpValidationAttempts = 0;
      }

      // Check if we should retry now (exponential backoff)
      if (!ShouldRetryNow(registration))
      {
        _logger.LogInformation(
            "Skipping registration {Id} - not yet time for retry (attempt {Attempt})",
            registration.Id,
            registration.ErpValidationAttempts);
        return ProcessingResult.Skipped;
      }

      // Check if staff number is available (should be populated during ID verification)
      if (string.IsNullOrWhiteSpace(registration.StaffNumber))
      {
        _logger.LogWarning(
            "Registration {Id} has no staff number - marking for manual review",
            registration.Id);

        registration.RequiresManualReview = true;
        registration.ManualReviewReason = "Staff number not available. May require re-verification.";
        await _context.SaveChangesAsync(CancellationToken.None);
        return ProcessingResult.FlaggedForManualReview;
      }

      // Increment attempt counter
      registration.ErpValidationAttempts++;
      registration.LastErpValidationAttempt = DateTime.UtcNow;

      _logger.LogInformation(
          "Validating staff number {StaffNumber} for registration {Id} (attempt {Attempt}/{MaxAttempts})",
          registration.StaffNumber,
          registration.Id,
          registration.ErpValidationAttempts,
          _jobSettings.MaxRetryAttempts);

      // Call ERP validation
      var erpResult = await _erpService.ValidateStaffNumberAsync(
          registration.StaffNumber,
          CancellationToken.None);

      if (erpResult.IsValid)
      {
        await HandleValidRegistration(registration, erpResult);
        return ProcessingResult.Approved;
      }
      else
      {
        return await HandleInvalidRegistration(registration, erpResult);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex,
          "Error processing registration {Id} for staff number {StaffNumber}",
          registration.Id,
          registration.StaffNumber);

      // Don't throw - continue processing other registrations
      return ProcessingResult.Error;
    }
  }

  /// <summary>
  /// Determines if we should retry ERP validation now
  /// Implements exponential backoff based on configured RetryDelayMinutes
  /// Formula: delay = base_delay * 2^(attempt - 1)
  /// Example with base_delay = 10 minutes:
  /// - Attempt 1: immediate (at registration time)
  /// - Attempt 2: 10 minutes after attempt 1
  /// - Attempt 3: 20 minutes after attempt 2
  /// - Attempt 4: 40 minutes after attempt 3
  /// - Attempt 5: 80 minutes after attempt 4
  /// </summary>
  private bool ShouldRetryNow(Core.Entities.AlumniRegistration registration)
  {
    var attemptCount = registration.ErpValidationAttempts ?? 0;

    // First attempt should happen immediately
    if (attemptCount == 0)
    {
      return true;
    }

    // No more retries after max attempts
    if (attemptCount >= _jobSettings.MaxRetryAttempts)
    {
      return false;
    }

    var lastAttempt = registration.LastErpValidationAttempt ?? registration.CreatedAt;

    // Calculate exponential backoff delay: base_delay * 2^(attempt - 1)
    var delayMinutes = _jobSettings.RetryDelayMinutes * Math.Pow(2, attemptCount - 1);

    var nextRetryTime = lastAttempt.AddMinutes(delayMinutes);

    return DateTime.UtcNow >= nextRetryTime;
  }

  /// <summary>
  /// Handles a valid ERP validation result
  /// Updates status to Approved and sends verification email
  /// Creates audit log for automatic approval
  /// </summary>
  private async Task HandleValidRegistration(
      Core.Entities.AlumniRegistration registration,
      ErpValidationResult erpResult)
  {
    _logger.LogInformation(
        "[SUCCESS] ERP validation successful for staff number {StaffNumber}. Status → Approved (Automatic)",
        registration.StaffNumber);

    var previousStatus = registration.RegistrationStatus;

    // Update registration with ERP data
    registration.ErpValidated = true;
    registration.ErpValidatedAt = DateTime.UtcNow;
    registration.ErpStaffName = erpResult.StaffName;
    registration.ErpDepartment = erpResult.Department;
    registration.ErpExitDate = erpResult.ExitDate;

    // Generate verification token (30-day expiry)
    var verificationToken = _tokenService.GenerateVerificationToken(
        registration.Id,
        registration.Email);

    registration.EmailVerificationToken = verificationToken;
    registration.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(30);

    // Update status to Approved
    registration.RegistrationStatus = RegistrationStatus.Approved.ToString();
    registration.ApprovedAt = DateTime.UtcNow;
    registration.UpdatedAt = DateTime.UtcNow;
    registration.UpdatedBy = "System (Automatic ERP Validation)";

    // Create audit log for automatic approval
    var auditLog = new AuditLog
    {
      RegistrationId = registration.Id,
      Action = AuditActions.AutomaticApproval,
      PerformedBy = "System",
      Notes = $"Automatically approved based on ERP validation. Staff Name: {erpResult.StaffName}, Department: {erpResult.Department}",
      PreviousStatus = previousStatus,
      NewStatus = RegistrationStatus.Approved.ToString(),
      IsAutomated = true,
      Timestamp = DateTime.UtcNow
    };

    _context.AuditLogs.Add(auditLog);

    // Send approval email with verification link
    try
    {
      var emailSent = await _emailService.SendApprovalEmailAsync(
          registration.FullName,
          registration.Email,
          verificationToken,
          CancellationToken.None);

      if (emailSent)
      {
        registration.ApprovalEmailSent = true;
        registration.ApprovalEmailSentAt = DateTime.UtcNow;
        _logger.LogInformation("Approval email sent to {Email}", registration.Email);
      }
      else
      {
        _logger.LogWarning("Failed to send approval email to {Email}", registration.Email);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending approval email to {Email}", registration.Email);
    }
  }

  /// <summary>
  /// Handles an invalid ERP validation result
  /// HYBRID WORKFLOW: Retries up to configured max attempts, then FLAGS for HR manual review (instead of auto-rejecting)
  /// This allows HR to investigate edge cases, data mismatches, or system issues
  /// </summary>
  private Task<ProcessingResult> HandleInvalidRegistration(
      Core.Entities.AlumniRegistration registration,
      ErpValidationResult erpResult)
  {
    var attemptCount = registration.ErpValidationAttempts ?? 0;

    _logger.LogWarning(
        "[WARNING] ERP validation failed for staff number {StaffNumber} (attempt {Attempt}/{MaxAttempts}): {Error}",
        registration.StaffNumber,
        attemptCount,
        _jobSettings.MaxRetryAttempts,
        erpResult.ErrorMessage);

    // Check if we've exhausted all retry attempts
    if (attemptCount >= _jobSettings.MaxRetryAttempts)
    {
      _logger.LogWarning(
          "[FLAG] Max retry attempts reached for staff number {StaffNumber}. Flagging for HR manual review (HYBRID WORKFLOW)",
          registration.StaffNumber);

      var previousStatus = registration.RegistrationStatus;

      // FLAG for manual review instead of auto-rejecting
      registration.RequiresManualReview = true;
      registration.ManualReviewReason =
          $"ERP validation failed after {_jobSettings.MaxRetryAttempts} attempts. " +
          $"Last error: {erpResult.ErrorMessage}. " +
          $"HR should verify staff number and employment history manually.";
      registration.UpdatedAt = DateTime.UtcNow;
      registration.UpdatedBy = "System (Automatic ERP Validation)";

      // Create audit log for flagging
      var auditLog = new AuditLog
      {
        RegistrationId = registration.Id,
        Action = "Flagged for Manual Review",
        PerformedBy = "System",
        Notes = registration.ManualReviewReason,
        PreviousStatus = previousStatus,
        NewStatus = "Pending (Requires Manual Review)",
        IsAutomated = true,
        Timestamp = DateTime.UtcNow
      };

      _context.AuditLogs.Add(auditLog);

      _logger.LogInformation(
          "Registration {Id} flagged for HR manual review. HR can now approve or reject via admin dashboard.",
          registration.Id);

      return Task.FromResult(ProcessingResult.FlaggedForManualReview);
    }
    else
    {
      _logger.LogInformation(
          "Will retry ERP validation for staff number {StaffNumber} (next attempt: {NextAttempt}/{MaxAttempts})",
          registration.StaffNumber,
          attemptCount + 1,
          _jobSettings.MaxRetryAttempts);

      // Keep status as Pending for retry
      registration.UpdatedAt = DateTime.UtcNow;

      return Task.FromResult(ProcessingResult.Retry);
    }
  }

  /// <summary>
  /// Processing result for tracking statistics
  /// HYBRID WORKFLOW: No longer auto-rejects, only flags for manual review
  /// </summary>
  private enum ProcessingResult
  {
    Approved,                  // Automatically approved via ERP validation
    FlaggedForManualReview,    // ERP validation failed - flagged for HR review
    Retry,                     // Will retry ERP validation later
    Skipped,                   // Skipped due to backoff timing
    Error                      // Processing error occurred
  }
}
