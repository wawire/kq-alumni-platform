using KQAlumni.Core.Entities;

namespace KQAlumni.Core.Interfaces;

/// <summary>
/// Service for admin-side registration management (approve, reject, list)
/// </summary>
public interface IAdminRegistrationService
{
    /// <summary>
    /// Get all registrations that require manual review
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of registrations requiring manual review</returns>
    Task<List<AlumniRegistration>> GetRegistrationsRequiringManualReviewAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all registrations with pagination and filtering
    /// </summary>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="requiresManualReview">Filter by manual review flag (optional)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of registrations</returns>
    Task<(List<AlumniRegistration> Registrations, int TotalCount)> GetRegistrationsAsync(
        string? status = null,
        bool? requiresManualReview = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually approve a registration
    /// </summary>
    /// <param name="registrationId">Registration ID</param>
    /// <param name="adminUserId">Admin user ID performing the action</param>
    /// <param name="adminUsername">Admin username</param>
    /// <param name="notes">Optional notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ApproveRegistrationAsync(
        Guid registrationId,
        int adminUserId,
        string adminUsername,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually reject a registration
    /// </summary>
    /// <param name="registrationId">Registration ID</param>
    /// <param name="adminUserId">Admin user ID performing the action</param>
    /// <param name="adminUsername">Admin username</param>
    /// <param name="reason">Rejection reason</param>
    /// <param name="notes">Optional additional notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RejectRegistrationAsync(
        Guid registrationId,
        int adminUserId,
        string adminUsername,
        string reason,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    Task<AdminDashboardStats> GetDashboardStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs for a registration
    /// </summary>
    /// <param name="registrationId">Registration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs</returns>
    Task<List<AuditLog>> GetAuditLogsAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk approve multiple registrations
    /// </summary>
    /// <param name="registrationIds">List of registration IDs</param>
    /// <param name="adminUserId">Admin user ID performing the action</param>
    /// <param name="adminUsername">Admin username</param>
    /// <param name="notes">Optional notes to apply to all approvals</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (Success count, Failed count, Results list)</returns>
    Task<(int SuccessCount, int FailureCount, List<BulkOperationResult> Results)> BulkApproveRegistrationsAsync(
        List<Guid> registrationIds,
        int adminUserId,
        string adminUsername,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk reject multiple registrations
    /// </summary>
    /// <param name="registrationIds">List of registration IDs</param>
    /// <param name="adminUserId">Admin user ID performing the action</param>
    /// <param name="adminUsername">Admin username</param>
    /// <param name="reason">Rejection reason to apply to all</param>
    /// <param name="notes">Optional notes to apply to all rejections</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (Success count, Failed count, Results list)</returns>
    Task<(int SuccessCount, int FailureCount, List<BulkOperationResult> Results)> BulkRejectRegistrationsAsync(
        List<Guid> registrationIds,
        int adminUserId,
        string adminUsername,
        string reason,
        string? notes = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result for a single bulk operation
/// </summary>
public class BulkOperationResult
{
    public Guid RegistrationId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Dashboard statistics DTO
/// </summary>
public class AdminDashboardStats
{
    public int TotalRegistrations { get; set; }
    public int PendingApproval { get; set; }
    public int RequiringManualReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Active { get; set; }
    public int EmailVerified { get; set; }
    public int EmailNotVerified { get; set; }
}
