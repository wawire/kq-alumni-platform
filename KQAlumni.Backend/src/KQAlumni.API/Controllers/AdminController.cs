using KQAlumni.API.DTOs;
using KQAlumni.Core.DTOs;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KQAlumni.API.Controllers;

/// <summary>
/// Controller for admin authentication and user management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminController> _logger;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        IAuthService authService,
        ILogger<AdminController> logger,
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _logger = logger;
        _context = context;
        _environment = environment;
    }

    /// <summary>
    /// Admin user login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user details</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminLoginResponse>> Login([FromBody] AdminLoginRequest request)
    {
        try
        {
            _logger.LogInformation("Admin login attempt for user: {Username}", request.Username);

            var token = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (token == null)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Username);
                return Unauthorized(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Authentication failed",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Invalid username or password"
                });
            }

            // Get user details
            var adminUser = await _authService.GetAdminUserByUsernameAsync(request.Username);

            if (adminUser == null)
            {
                return Unauthorized(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Authentication failed",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "User not found"
                });
            }

            var response = new AdminLoginResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(480), // 8 hours (from JwtSettings)
                UserId = adminUser.Id,
                Username = adminUser.Username,
                FullName = adminUser.FullName,
                Role = adminUser.Role,
                Email = adminUser.Email,
                RequiresPasswordChange = adminUser.RequiresPasswordChange
            };

            _logger.LogInformation("User {Username} logged in successfully with role {Role}. RequiresPasswordChange: {RequiresPasswordChange}",
                request.Username, adminUser.Role, adminUser.RequiresPasswordChange);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Get current logged-in admin user details
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdminLoginResponse>> GetCurrentUser()
    {
        try
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var adminUser = await _authService.GetAdminUserByUsernameAsync(username);

            if (adminUser == null)
            {
                return Unauthorized();
            }

            var response = new AdminLoginResponse
            {
                Token = string.Empty, // Don't return token in /me endpoint
                ExpiresAt = DateTime.UtcNow,
                UserId = adminUser.Id,
                Username = adminUser.Username,
                FullName = adminUser.FullName,
                Role = adminUser.Role,
                Email = adminUser.Email,
                RequiresPasswordChange = adminUser.RequiresPasswordChange
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Change current admin user's password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success message</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            _logger.LogInformation("Password change request for user: {Username}", username);

            var success = await _authService.ChangePasswordAsync(
                username,
                request.CurrentPassword,
                request.NewPassword);

            if (!success)
            {
                return BadRequest(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Password change failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Invalid current password or password change failed"
                });
            }

            _logger.LogInformation("Password changed successfully for user: {Username}", username);

            return Ok(new
            {
                message = "Password changed successfully",
                requiresPasswordChange = false
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid password format for user: {Username}", User.Identity?.Name);
            return BadRequest(new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid password",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {Username}", User.Identity?.Name);
            return StatusCode(500, new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred while changing password"
            });
        }
    }

    /// <summary>
    /// Create a new admin user (SuperAdmin only)
    /// </summary>
    /// <param name="request">New admin user details</param>
    /// <returns>Created admin user</returns>
    [HttpPost("users")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateAdminUser([FromBody] CreateAdminUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating admin user: {Username} with role {Role}",
                request.Username, request.Role);

            var adminUser = await _authService.CreateAdminUserAsync(
                request.Username,
                request.Email,
                request.Password,
                request.FullName,
                request.Role);

            return CreatedAtAction(nameof(GetCurrentUser), new { id = adminUser.Id }, new
            {
                id = adminUser.Id,
                username = adminUser.Username,
                email = adminUser.Email,
                fullName = adminUser.FullName,
                role = adminUser.Role
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create admin user: {Message}", ex.Message);
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
            _logger.LogError(ex, "Error creating admin user");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Seed initial SuperAdmin user (Development only)
    /// Creates the first admin user if none exist
    /// </summary>
    /// <returns>Created admin user details</returns>
    [HttpPost("seed-initial-admin")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SeedInitialAdmin()
    {
        // Only allow in development environment
        if (!_environment.IsDevelopment())
        {
            return StatusCode(403, new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "This endpoint is only available in Development environment"
            });
        }

        try
        {
            // Check if any admin users already exist
            var adminExists = await _context.AdminUsers.AnyAsync();

            if (adminExists)
            {
                return BadRequest(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Admin user already exists",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Admin users already exist. Cannot seed initial admin."
                });
            }

            // Create initial SuperAdmin user
            var adminUser = await _authService.CreateAdminUserAsync(
                username: "admin",
                email: "admin@kenya-airways.com",
                password: "Admin@123456",
                fullName: "System Administrator",
                role: "SuperAdmin");

            _logger.LogWarning(
                "[WARNING] Initial SuperAdmin user created via seed endpoint:\n" +
                "   Username: admin\n" +
                "   Password: Admin@123456\n" +
                "   CHANGE THIS PASSWORD IMMEDIATELY!");

            return CreatedAtAction(nameof(GetCurrentUser), new { id = adminUser.Id }, new
            {
                message = "Initial SuperAdmin user created successfully",
                username = "admin",
                password = "Admin@123456",
                email = "admin@kenya-airways.com",
                warning = "[WARNING] CHANGE THIS PASSWORD IMMEDIATELY AFTER FIRST LOGIN!",
                instructions = new[]
                {
                    "1. Login at /api/v1/admin/login with these credentials",
                    "2. Create additional admin users via /api/v1/admin/users",
                    "3. Change this default password immediately"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding initial admin user");
            return StatusCode(500, new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred while seeding the initial admin user"
            });
        }
    }
}
