using KQAlumni.Core.Entities;
using KQAlumni.Core.Interfaces;
using KQAlumni.Infrastructure.BackgroundServices;
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

    services.AddOptions<ErpApiSettings>()
        .Bind(configuration.GetSection(ErpApiSettings.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddOptions<EmailSettings>()
        .Bind(configuration.GetSection(EmailSettings.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddScoped<IRegistrationService, RegistrationService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAdminRegistrationService, AdminRegistrationService>();
    services.AddScoped<IEmailTemplateService, EmailTemplateService>();

    services.AddHttpClient<IErpService, ErpService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
          // Disable proxy for internal ERP (10.2.131.147)
          // This allows direct connection without going through system proxy
          UseProxy = false,
          Proxy = null
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

    // Register cache service as singleton (shared across all requests)
    services.AddHttpClient<IErpCacheService, ErpCacheService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
          UseProxy = false,
          Proxy = null
        });
    services.AddSingleton<IErpCacheService, ErpCacheService>();

    // Register background service for automatic cache refresh
    services.AddHostedService<ErpCacheBackgroundService>();

    services.AddScoped<IEmailService, EmailServiceWithTracking>();

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
