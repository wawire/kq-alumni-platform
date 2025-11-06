using Microsoft.Extensions.Options;

namespace KQAlumni.API.Services;

/// <summary>
/// Validates required configuration settings on application startup
/// Prevents the application from starting with invalid or missing configuration
/// </summary>
public class ConfigurationValidator : IHostedService
{
    private readonly ILogger<ConfigurationValidator> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _lifetime;

    public ConfigurationValidator(
        ILogger<ConfigurationValidator> logger,
        IConfiguration configuration,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _configuration = configuration;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[VALIDATION] Starting configuration validation...");

        var errors = new List<string>();

        // Validate Database Connection String
        ValidateConnectionString(errors);

        // Validate JWT Settings
        ValidateJwtSettings(errors);

        // Validate Email Settings
        ValidateEmailSettings(errors);

        // Validate ERP API Settings
        ValidateErpSettings(errors);

        // Validate AppSettings
        ValidateAppSettings(errors);

        // Validate CORS Settings
        ValidateCorsSettings(errors);

        if (errors.Any())
        {
            _logger.LogCritical(
                "[ERROR] CONFIGURATION VALIDATION FAILED:\n" +
                "The application cannot start due to missing or invalid configuration.\n" +
                "Please fix the following errors:\n\n{Errors}\n\n" +
                "See ENVIRONMENT_SETUP.md for configuration instructions.",
                string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}")));

            // Stop the application
            _lifetime.StopApplication();
            return Task.CompletedTask;
        }

        _logger.LogInformation("[SUCCESS] Configuration validation passed successfully");
        LogConfigurationSummary();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void ValidateConnectionString(List<string> errors)
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connString))
        {
            errors.Add("[ERROR] ConnectionStrings:DefaultConnection is missing or empty");
            return;
        }

        // Check for placeholder values
        if (connString.Contains("YOUR_SQL_PASSWORD_HERE", StringComparison.OrdinalIgnoreCase) ||
            connString.Contains("UPDATE_THIS_PASSWORD", StringComparison.OrdinalIgnoreCase) ||
            connString.Contains("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("[ERROR] Database connection string contains placeholder password. " +
                      "Update 'YOUR_SQL_PASSWORD_HERE' with actual password in appsettings.Production.json");
        }

        // Validate it contains required components
        if (!connString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("[ERROR] Database connection string missing 'Server=' parameter");
        }

        if (!connString.Contains("Database=", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("[ERROR] Database connection string missing 'Database=' parameter");
        }
    }

    private void ValidateJwtSettings(List<string> errors)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"];
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            errors.Add("[ERROR] JwtSettings:SecretKey is missing");
            return;
        }

        // Check for placeholder value
        if (secretKey.Contains("CHANGE-THIS", StringComparison.OrdinalIgnoreCase) ||
            secretKey.Contains("dev-secret-key", StringComparison.OrdinalIgnoreCase))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("[WARNING] WARNING: JwtSettings:SecretKey appears to be a placeholder or development key. " +
                          "Use a strong, unique secret key (64+ characters) for production");
            }
        }

        // Validate key strength
        if (secretKey.Length < 32)
        {
            errors.Add($"[ERROR] JwtSettings:SecretKey is too short ({secretKey.Length} characters). " +
                      "Must be at least 32 characters (recommended: 64+ characters)");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            errors.Add("[ERROR] JwtSettings:Issuer is missing");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            errors.Add("[ERROR] JwtSettings:Audience is missing");
        }
    }

    private void ValidateEmailSettings(List<string> errors)
    {
        var smtpServer = _configuration["Email:SmtpServer"];
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var from = _configuration["Email:From"];
        var displayName = _configuration["Email:DisplayName"];

        if (string.IsNullOrWhiteSpace(smtpServer))
        {
            errors.Add("[WARNING] Email:SmtpServer is missing (emails will not be sent)");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("[WARNING] Email:Username is missing (emails will not be sent)");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("[WARNING] Email:Password is missing (emails will not be sent)");
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            errors.Add("[WARNING] Email:From is missing (emails will not be sent)");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors.Add("[WARNING] Email:DisplayName is missing");
        }

        // Check if email sending is disabled
        var enableSending = _configuration.GetValue<bool>("Email:EnableEmailSending");
        var useMock = _configuration.GetValue<bool>("Email:UseMockEmailService");

        if (!enableSending)
        {
            _logger.LogWarning("[WARNING] Email sending is DISABLED (EnableEmailSending = false)");
        }

        if (useMock)
        {
            _logger.LogWarning("[WARNING] Using MOCK email service (UseMockEmailService = true). Emails will be logged but not sent.");
        }
    }

    private void ValidateErpSettings(List<string> errors)
    {
        var baseUrl = _configuration["ErpApi:BaseUrl"];
        var endpoint = _configuration["ErpApi:Endpoint"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            errors.Add("[WARNING] ErpApi:BaseUrl is missing (ERP validation will fail)");
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            errors.Add("[WARNING] ErpApi:Endpoint is missing (ERP validation will fail)");
        }

        var mockMode = _configuration.GetValue<bool>("ErpApi:EnableMockMode");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        if (mockMode && env.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "[WARNING] WARNING: ERP Mock Mode is ENABLED in Production environment. " +
                "Set ErpApi:EnableMockMode = false for real ERP validation");
        }
    }

    private void ValidateAppSettings(List<string> errors)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            errors.Add("[ERROR] AppSettings:BaseUrl is missing (email verification links will be broken)");
            return;
        }

        // Check for localhost in production
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
            baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                "[ERROR] AppSettings:BaseUrl is set to localhost in Production environment. " +
                "Email verification links will not work. " +
                "Update to production URL (e.g., https://kqalumni-dev.kenya-airways.com)");
        }

        // Validate URL format
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            errors.Add($"[ERROR] AppSettings:BaseUrl is not a valid URL: {baseUrl}");
        }
        else if (uri.Scheme != "https" && !baseUrl.Contains("localhost"))
        {
            _logger.LogWarning(
                "[WARNING] AppSettings:BaseUrl is using HTTP instead of HTTPS: {BaseUrl}. " +
                "Consider using HTTPS for production",
                baseUrl);
        }
    }

    private void ValidateCorsSettings(List<string> errors)
    {
        var allowedOrigins = _configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            errors.Add("[WARNING] CorsSettings:AllowedOrigins is empty. Frontend will not be able to access the API");
            return;
        }

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            var hasLocalhost = allowedOrigins.Any(o =>
                o.Contains("localhost", StringComparison.OrdinalIgnoreCase));

            if (hasLocalhost)
            {
                _logger.LogWarning(
                    "[WARNING] WARNING: CORS allows localhost in Production environment. " +
                    "This may be a security risk if not intentional");
            }
        }
    }

    private void LogConfigurationSummary()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "Not Set";
        var smtpServer = _configuration["Email:SmtpServer"] ?? "Not Set";
        var emailEnabled = _configuration.GetValue<bool>("Email:EnableEmailSending");
        var emailMock = _configuration.GetValue<bool>("Email:UseMockEmailService");
        var erpMock = _configuration.GetValue<bool>("ErpApi:EnableMockMode");
        var connString = _configuration.GetConnectionString("DefaultConnection") ?? "Not Set";
        var server = ExtractServerFromConnectionString(connString);
        var database = ExtractDatabaseFromConnectionString(connString);

        _logger.LogInformation(
            "\n" +
            "╔════════════════════════════════════════════════════════════════╗\n" +
            "║          KQ ALUMNI PLATFORM - CONFIGURATION SUMMARY           ║\n" +
            "╠════════════════════════════════════════════════════════════════╣\n" +
            "║ Environment:        {Environment,-42} ║\n" +
            "║ Base URL:           {BaseUrl,-42} ║\n" +
            "║                                                                ║\n" +
            "║ DATABASE:                                                      ║\n" +
            "║   Server:           {DbServer,-42} ║\n" +
            "║   Database:         {Database,-42} ║\n" +
            "║                                                                ║\n" +
            "║ EMAIL:                                                         ║\n" +
            "║   SMTP Server:      {SmtpServer,-42} ║\n" +
            "║   Sending Enabled:  {EmailEnabled,-42} ║\n" +
            "║   Mock Mode:        {EmailMock,-42} ║\n" +
            "║                                                                ║\n" +
            "║ ERP:                                                           ║\n" +
            "║   Mock Mode:        {ErpMock,-42} ║\n" +
            "╚════════════════════════════════════════════════════════════════╝",
            env,
            baseUrl,
            server,
            database,
            smtpServer,
            emailEnabled ? "[SUCCESS] Yes" : "[ERROR] No",
            emailMock ? "[WARNING] Yes (Logging Only)" : "[SUCCESS] No (Real Sending)",
            erpMock ? "[WARNING] Yes (Using Mock Data)" : "[SUCCESS] No (Real API)"
        );
    }

    private string ExtractServerFromConnectionString(string connString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connString, @"Server=([^;]+)");
        return match.Success ? match.Groups[1].Value : "Not Found";
    }

    private string ExtractDatabaseFromConnectionString(string connString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connString, @"Database=([^;]+)");
        return match.Success ? match.Groups[1].Value : "Not Found";
    }
}
