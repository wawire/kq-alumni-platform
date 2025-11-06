using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KQAlumni.Infrastructure.Services;

/// <summary>
/// Service for admin-side registration management
/// </summary>
public class AdminRegistrationService : IAdminRegistrationService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminRegistrationService> _logger;

    public AdminRegistrationService(
        AppDbContext context,
        IEmailService emailService,
        ILogger<AdminRegistrationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Get all registrations that require manual review
    /// </summary>
    public async Task<List<AlumniRegistration>> GetRegistrationsRequiringManualReviewAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AlumniRegistrations
            .Where(r => r.RequiresManualReview && !r.ManuallyReviewed)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all registrations with pagination and filtering
    /// </summary>
    public async Task<(List<AlumniRegistration> Registrations, int TotalCount)> GetRegistrationsAsync(
        string? status = null,
        bool? requiresManualReview = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AlumniRegistrations.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.RegistrationStatus == status);
        }

        if (requiresManualReview.HasValue)
        {
            query = query.Where(r => r.RequiresManualReview == requiresManualReview.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var registrations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (registrations, totalCount);
    }

    /// <summary>
    /// Manually approve a registration
    /// </summary>
    public async Task ApproveRegistrationAsync(
        Guid registrationId,
        int adminUserId,
        string adminUsername,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var registration = await _context.AlumniRegistrations
            .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration == null)
        {
            throw new InvalidOperationException($"Registration {registrationId} not found");
        }

        // Validate current status
        if (registration.RegistrationStatus == "Approved" || registration.RegistrationStatus == "Active")
        {
            throw new InvalidOperationException("Registration is already approved");
        }

        var previousStatus = registration.RegistrationStatus;

        // Update registration status
        registration.RegistrationStatus = "Approved";
        registration.ApprovedAt = DateTime.UtcNow;
        registration.ManuallyReviewed = true;
        registration.ReviewedBy = adminUsername;
        registration.ReviewedAt = DateTime.UtcNow;
        registration.ReviewNotes = notes;
        registration.UpdatedAt = DateTime.UtcNow;
        registration.UpdatedBy = adminUsername;

        // Create audit log
        var auditLog = new AuditLog
        {
            RegistrationId = registration.Id,
            Action = "Manual Approval",
            PerformedBy = adminUsername,
            AdminUserId = adminUserId,
            Notes = notes,
            PreviousStatus = previousStatus,
            NewStatus = "Approved",
            IsAutomated = false,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Registration {RegistrationId} manually approved by {AdminUsername}",
            registrationId, adminUsername);

        // Send approval email
        try
        {
            if (!registration.ApprovalEmailSent)
            {
                // CRITICAL FIX: Generate verification token if it doesn't exist
                if (string.IsNullOrEmpty(registration.EmailVerificationToken))
                {
                    registration.EmailVerificationToken = Guid.NewGuid().ToString("N");
                    registration.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(30);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Generated verification token for manual approval of registration {RegistrationId}",
                        registrationId);
                }

                await _emailService.SendApprovalEmailAsync(
                    registration.Email,
                    registration.FullName,
                    registration.EmailVerificationToken!);

                registration.ApprovalEmailSent = true;
                registration.ApprovalEmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Approval email sent successfully for registration {RegistrationId}",
                    registrationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval email for registration {RegistrationId}",
                registrationId);
        }
    }

    /// <summary>
    /// Manually reject a registration
    /// </summary>
    public async Task RejectRegistrationAsync(
        Guid registrationId,
        int adminUserId,
        string adminUsername,
        string reason,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var registration = await _context.AlumniRegistrations
            .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration == null)
        {
            throw new InvalidOperationException($"Registration {registrationId} not found");
        }

        // Validate current status
        if (registration.RegistrationStatus == "Rejected")
        {
            throw new InvalidOperationException("Registration is already rejected");
        }

        var previousStatus = registration.RegistrationStatus;

        // Update registration status
        registration.RegistrationStatus = "Rejected";
        registration.RejectedAt = DateTime.UtcNow;
        registration.RejectionReason = reason;
        registration.ManuallyReviewed = true;
        registration.ReviewedBy = adminUsername;
        registration.ReviewedAt = DateTime.UtcNow;
        registration.ReviewNotes = notes;
        registration.UpdatedAt = DateTime.UtcNow;
        registration.UpdatedBy = adminUsername;

        // Create audit log
        var auditLog = new AuditLog
        {
            RegistrationId = registration.Id,
            Action = "Manual Rejection",
            PerformedBy = adminUsername,
            AdminUserId = adminUserId,
            Notes = notes,
            RejectionReason = reason,
            PreviousStatus = previousStatus,
            NewStatus = "Rejected",
            IsAutomated = false,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Registration {RegistrationId} manually rejected by {AdminUsername}. Reason: {Reason}",
            registrationId, adminUsername, reason);

        // Send rejection email
        try
        {
            if (!registration.RejectionEmailSent)
            {
                await _emailService.SendRejectionEmailAsync(
                    registration.Email,
                    registration.FullName,
                    reason);

                registration.RejectionEmailSent = true;
                registration.RejectionEmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send rejection email for registration {RegistrationId}",
                registrationId);
        }
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    public async Task<AdminDashboardStats> GetDashboardStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var stats = new AdminDashboardStats
        {
            TotalRegistrations = await _context.AlumniRegistrations.CountAsync(cancellationToken),
            PendingApproval = await _context.AlumniRegistrations
                .CountAsync(r => r.RegistrationStatus == "Pending", cancellationToken),
            RequiringManualReview = await _context.AlumniRegistrations
                .CountAsync(r => r.RequiresManualReview && !r.ManuallyReviewed, cancellationToken),
            Approved = await _context.AlumniRegistrations
                .CountAsync(r => r.RegistrationStatus == "Approved", cancellationToken),
            Rejected = await _context.AlumniRegistrations
                .CountAsync(r => r.RegistrationStatus == "Rejected", cancellationToken),
            Active = await _context.AlumniRegistrations
                .CountAsync(r => r.RegistrationStatus == "Active", cancellationToken),
            EmailVerified = await _context.AlumniRegistrations
                .CountAsync(r => r.EmailVerified, cancellationToken),
            EmailNotVerified = await _context.AlumniRegistrations
                .CountAsync(r => !r.EmailVerified, cancellationToken)
        };

        return stats;
    }

    /// <summary>
    /// Get audit logs for a registration
    /// </summary>
    public async Task<List<AuditLog>> GetAuditLogsAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.RegistrationId == registrationId)
            .Include(a => a.AdminUser)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Bulk approve multiple registrations
    /// </summary>
    public async Task<(int SuccessCount, int FailureCount, List<BulkOperationResult> Results)> BulkApproveRegistrationsAsync(
        List<Guid> registrationIds,
        int adminUserId,
        string adminUsername,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<BulkOperationResult>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var registrationId in registrationIds)
        {
            try
            {
                await ApproveRegistrationAsync(
                    registrationId,
                    adminUserId,
                    adminUsername,
                    notes,
                    cancellationToken);

                results.Add(new BulkOperationResult
                {
                    RegistrationId = registrationId,
                    Success = true
                });
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to approve registration {RegistrationId} in bulk operation",
                    registrationId);

                results.Add(new BulkOperationResult
                {
                    RegistrationId = registrationId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
                failureCount++;
            }
        }

        _logger.LogInformation(
            "Bulk approve completed by {AdminUsername}. Success: {SuccessCount}, Failed: {FailureCount}",
            adminUsername, successCount, failureCount);

        return (successCount, failureCount, results);
    }

    /// <summary>
    /// Bulk reject multiple registrations
    /// </summary>
    public async Task<(int SuccessCount, int FailureCount, List<BulkOperationResult> Results)> BulkRejectRegistrationsAsync(
        List<Guid> registrationIds,
        int adminUserId,
        string adminUsername,
        string reason,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<BulkOperationResult>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var registrationId in registrationIds)
        {
            try
            {
                await RejectRegistrationAsync(
                    registrationId,
                    adminUserId,
                    adminUsername,
                    reason,
                    notes,
                    cancellationToken);

                results.Add(new BulkOperationResult
                {
                    RegistrationId = registrationId,
                    Success = true
                });
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to reject registration {RegistrationId} in bulk operation",
                    registrationId);

                results.Add(new BulkOperationResult
                {
                    RegistrationId = registrationId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
                failureCount++;
            }
        }

        _logger.LogInformation(
            "Bulk reject completed by {AdminUsername}. Success: {SuccessCount}, Failed: {FailureCount}",
            adminUsername, successCount, failureCount);

        return (successCount, failureCount, results);
    }
}
