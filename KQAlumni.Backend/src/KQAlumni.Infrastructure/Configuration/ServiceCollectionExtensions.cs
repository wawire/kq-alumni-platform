using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.Data;
using KQAlumni.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace KQAlumni.Infrastructure.Configuration;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers all infrastructure services (Database, ERP, Email, etc.)
  /// </summary>
  public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // ========================================
    // Database (Entity Framework Core)
    // ========================================

    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(connectionString, sqlOptions =>
      {
        sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
      });

      // Enable detailed errors in development
      var environment = configuration.GetValue<string>("Environment") ?? "Production";
      if (environment == "Development")
      {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
      }
    });

    // ========================================
    // Configuration Options
    // ========================================

    services.Configure<ErpApiSettings>(
        configuration.GetSection(ErpApiSettings.SectionName));

    services.Configure<EmailSettings>(
        configuration.GetSection(EmailSettings.SectionName));

    // ========================================
    // Business Services
    // ========================================

    services.AddScoped<IRegistrationService, RegistrationService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAdminRegistrationService, AdminRegistrationService>();

    // ========================================
    // ERP Service with HttpClient + Polly Resilience
    // ========================================

    services.AddHttpClient<IErpService, ErpService>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

    // ========================================
    // Email Service
    // ========================================

    services.AddScoped<IEmailService, EmailService>();

    // ========================================
    // Service Registration Complete
    // ========================================
    // Infrastructure services registered:
    // - DbContext: AppDbContext
    // - IRegistrationService: RegistrationService
    // - ITokenService: TokenService
    // - IErpService: ErpService (with Polly policies)
    // - IEmailService: EmailService
    // - Configuration: ErpApiSettings, EmailSettings

    return services;
  }

  /// <summary>
  /// Retry policy for ERP API calls
  /// Retries 3 times with exponential backoff
  /// </summary>
  private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
              // Retry logging handled by ErpService with ILogger
            });
  }

  /// <summary>
  /// Circuit breaker policy for ERP API
  /// Breaks circuit after 5 consecutive failures
  /// </summary>
  private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
  {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, timespan) =>
            {
              // Circuit breaker logging handled by ErpService with ILogger
            },
            onReset: () =>
            {
              // Circuit breaker reset logging handled by ErpService with ILogger
            });
  }
}
