using System.Security.Claims;
using KQAlumni.API.DTOs;
using KQAlumni.Core.DTOs;
using KQAlumni.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KQAlumni.API.Controllers;

/// <summary>
/// Controller for admin registration management (approve, reject, list)
/// </summary>
[ApiController]
[Route("api/v1/admin/registrations")]
[Authorize(Policy = "HROfficer")] // All HR roles can access
[Produces("application/json")]
public class AdminRegistrationsController : ControllerBase
{
    private readonly IAdminRegistrationService _adminRegistrationService;
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<AdminRegistrationsController> _logger;

    public AdminRegistrationsController(
        IAdminRegistrationService adminRegistrationService,
        IRegistrationService registrationService,
        ILogger<AdminRegistrationsController> logger)
    {
        _adminRegistrationService = adminRegistrationService;
        _registrationService = registrationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all registrations with pagination and filtering
    /// </summary>
    /// <param name="status">Filter by status (Pending, Approved, Rejected, Active)</param>
    /// <param name="requiresManualReview">Filter by manual review requirement</param>
    /// <param name="searchQuery">Search by registration number, name, email, staff number, or ID</param>
    /// <param name="dateFrom">Filter registrations created from this date</param>
    /// <param name="dateTo">Filter registrations created to this date</param>
    /// <param name="emailVerified">Filter by email verification status</param>
    /// <param name="sortBy">Sort by column (fullName, createdAt, registrationStatus, staffNumber, email)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of registrations</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetRegistrations(
        [FromQuery] string? status = null,
        [FromQuery] bool? requiresManualReview = null,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        [FromQuery] bool? emailVerified = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // Parse dates if provided
            DateTime? parsedDateFrom = null;
            DateTime? parsedDateTo = null;

            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var from))
            {
                parsedDateFrom = from;
            }

            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var to))
            {
                parsedDateTo = to.AddDays(1).AddSeconds(-1); // Include full day
            }

            var (registrations, totalCount) = await _adminRegistrationService.GetRegistrationsAsync(
                status,
                requiresManualReview,
                searchQuery,
                parsedDateFrom,
                parsedDateTo,
                emailVerified,
                sortBy,
                sortOrder,
                pageNumber,
                pageSize,
                cancellationToken);

            var response = new
            {
                data = registrations,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving registrations");
            return StatusCode(500, new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred while retrieving registrations"
            });
        }
    }

    /// <summary>
    /// Get registrations requiring manual review
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of registrations requiring manual review</returns>
    [HttpGet("requiring-review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetRegistrationsRequiringReview(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var registrations = await _adminRegistrationService
                .GetRegistrationsRequiringManualReviewAsync(cancellationToken);

            return Ok(new { data = registrations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving registrations requiring review");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Get a specific registration by ID
    /// </summary>
    /// <param name="id">Registration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetRegistration(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id, cancellationToken);

            if (registration == null)
            {
                return NotFound(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Registration {id} not found"
                });
            }

            return Ok(registration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving registration {RegistrationId}", id);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Manually approve a registration
    /// </summary>
    /// <param name="id">Registration ID</param>
    /// <param name="request">Approval request with optional notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = "HRManager")] // Only HRManager and SuperAdmin can approve
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ApproveRegistration(
        [FromRoute] Guid id,
        [FromBody] ApproveRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var adminUsername = User.Identity?.Name ?? "Unknown";

            await _adminRegistrationService.ApproveRegistrationAsync(
                id,
                adminUserId,
                adminUsername,
                request.Notes,
                cancellationToken);

            _logger.LogInformation(
                "Registration {RegistrationId} approved by {AdminUsername}",
                id, adminUsername);

            return Ok(new { message = "Registration approved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to approve registration {RegistrationId}", id);
            return BadRequest(new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving registration {RegistrationId}", id);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Manually reject a registration
    /// </summary>
    /// <param name="id">Registration ID</param>
    /// <param name="request">Rejection request with reason and notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/reject")]
    [Authorize(Policy = "HRManager")] // Only HRManager and SuperAdmin can reject
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> RejectRegistration(
        [FromRoute] Guid id,
        [FromBody] RejectRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var adminUsername = User.Identity?.Name ?? "Unknown";

            await _adminRegistrationService.RejectRegistrationAsync(
                id,
                adminUserId,
                adminUsername,
                request.Reason,
                request.Notes,
                cancellationToken);

            _logger.LogInformation(
                "Registration {RegistrationId} rejected by {AdminUsername}. Reason: {Reason}",
                id, adminUsername, request.Reason);

            return Ok(new { message = "Registration rejected successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to reject registration {RegistrationId}", id);
            return BadRequest(new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting registration {RegistrationId}", id);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("~/api/v1/admin/dashboard/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetDashboardStats(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _adminRegistrationService.GetDashboardStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Get audit logs for a registration
    /// </summary>
    /// <param name="id">Registration ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs</returns>
    [HttpGet("{id}/audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetAuditLogs(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _adminRegistrationService.GetAuditLogsAsync(id, cancellationToken);
            return Ok(new { data = auditLogs });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for registration {RegistrationId}", id);
            return StatusCode(500);
        }
    }
}
