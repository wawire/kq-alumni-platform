using KQAlumni.Core.Configuration;
using KQAlumni.Core.Entities;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace KQAlumni.API.HealthChecks;

/// <summary>
/// Health check that validates all critical configuration settings
/// Ensures configuration is valid before the application starts accepting requests
/// </summary>
public class ConfigurationHealthCheck : IHealthCheck
{
    private readonly ErpApiSettings _erpSettings;
    private readonly EmailSettings _emailSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationHealthCheck> _logger;

    public ConfigurationHealthCheck(
        IOptions<ErpApiSettings> erpSettings,
        IOptions<EmailSettings> emailSettings,
        IOptions<JwtSettings> jwtSettings,
        IConfiguration configuration,
        ILogger<ConfigurationHealthCheck> logger)
    {
        _erpSettings = erpSettings.Value;
        _emailSettings = emailSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate ERP API Settings
        ValidateSettings(_erpSettings, "ErpApi", errors, warnings);

        // Validate Email Settings
        ValidateSettings(_emailSettings, "Email", errors, warnings);

        // Validate JWT Settings
        ValidateSettings(_jwtSettings, "JwtSettings", errors, warnings);

        // Additional custom checks
        CheckConnectionString(errors);
        CheckEnvironmentSpecificSettings(errors, warnings);

        // Determine health status
        if (errors.Any())
        {
            var errorMessage = $"Configuration validation failed:\n{string.Join("\n", errors)}";
            _logger.LogError("Configuration Health Check FAILED: {Errors}", string.Join("; ", errors));

            return Task.FromResult(HealthCheckResult.Unhealthy(
                errorMessage,
                data: new Dictionary<string, object>
                {
                    { "errors", errors },
                    { "warnings", warnings },
                    { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" }
                }));
        }

        if (warnings.Any())
        {
            var warningMessage = $"Configuration warnings:\n{string.Join("\n", warnings)}";
            _logger.LogWarning("Configuration Health Check passed with warnings: {Warnings}", string.Join("; ", warnings));

            return Task.FromResult(HealthCheckResult.Degraded(
                warningMessage,
                data: new Dictionary<string, object>
                {
                    { "warnings", warnings },
                    { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" }
                }));
        }

        _logger.LogInformation("Configuration Health Check PASSED - All settings are valid");
        return Task.FromResult(HealthCheckResult.Healthy(
            "All configuration settings are valid",
            data: new Dictionary<string, object>
            {
                { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown" }
            }));
    }

    /// <summary>
    /// Validates any object with DataAnnotations
    /// </summary>
    private void ValidateSettings<T>(T settings, string settingsName, List<string> errors, List<string> warnings)
    {
        var validationContext = new ValidationContext(settings);
        var validationResults = new List<ValidationResult>();

        // Validate DataAnnotations
        var isValid = Validator.TryValidateObject(settings, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                var message = $"{settingsName}: {validationResult.ErrorMessage}";

                // Check if this is a warning (contains "should" or "recommended")
                if (validationResult.ErrorMessage?.Contains("should", StringComparison.OrdinalIgnoreCase) == true ||
                    validationResult.ErrorMessage?.Contains("recommended", StringComparison.OrdinalIgnoreCase) == true)
                {
                    warnings.Add(message);
                }
                else
                {
                    errors.Add(message);
                }
            }
        }

        // Custom validation using IValidatableObject
        if (settings is IValidatableObject validatable)
        {
            var customResults = validatable.Validate(validationContext);
            foreach (var result in customResults)
            {
                var message = $"{settingsName}: {result.ErrorMessage}";

                if (result.ErrorMessage?.Contains("should", StringComparison.OrdinalIgnoreCase) == true ||
                    result.ErrorMessage?.Contains("recommended", StringComparison.OrdinalIgnoreCase) == true)
                {
                    warnings.Add(message);
                }
                else
                {
                    errors.Add(message);
                }
            }
        }
    }

    /// <summary>
    /// Validates database connection string
    /// </summary>
    private void CheckConnectionString(List<string> errors)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Database connection string 'DefaultConnection' is not configured");
            return;
        }

        // Check for placeholder values
        if (connectionString.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("CHANGE-THIS", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("USE_ENVIRONMENT_VARIABLE", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Database connection string contains placeholder values - replace with actual values");
        }
    }

    /// <summary>
    /// Checks environment-specific configuration requirements
    /// </summary>
    private void CheckEnvironmentSpecificSettings(List<string> errors, List<string> warnings)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment == "Production")
        {
            // Production-specific checks
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl) || baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("AppSettings:BaseUrl must not be localhost in Production environment");
            }

            // Check CORS settings
            var corsOrigins = _configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
            if (corsOrigins == null || corsOrigins.Length == 0)
            {
                errors.Add("CorsSettings:AllowedOrigins must be configured in Production");
            }
            else if (corsOrigins.Any(o => o.Contains("localhost", StringComparison.OrdinalIgnoreCase)))
            {
                warnings.Add("CorsSettings:AllowedOrigins contains localhost URLs in Production - this may be intentional for testing");
            }
        }
        else if (environment == "Development")
        {
            // Development-specific warnings
            if (!_erpSettings.EnableMockMode)
            {
                warnings.Add("ERP Mock Mode is disabled in Development - you may have issues if ERP is not accessible");
            }

            if (_emailSettings.EnableEmailSending && !_emailSettings.UseMockEmailService)
            {
                warnings.Add("Email sending is enabled without mock service in Development - real emails will be sent");
            }
        }
    }
}
