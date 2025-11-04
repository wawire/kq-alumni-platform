using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using KQAlumni.API.Middleware;
using KQAlumni.Core.Configuration;
using KQAlumni.Core.Validators;
using KQAlumni.Infrastructure.BackgroundJobs;
using KQAlumni.Infrastructure.Configuration;
using KQAlumni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using KQAlumni.API.HealthChecks;
using KQAlumni.API.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURE SERVICES
// ========================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationRequestValidator>();

// ========================================
// RESPONSE COMPRESSION
// ========================================

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/css",
        "text/html",
        "application/javascript",
        "text/javascript"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// ========================================
// RESPONSE CACHING & DISTRIBUTED CACHE
// ========================================

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024 * 1024;
    options.SizeLimit = 100 * 1024 * 1024;
    options.UseCaseSensitivePaths = false;
});

var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", false);
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

if (redisEnabled && !string.IsNullOrEmpty(redisConnectionString))
{
    try
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName", "KQAlumni:");
        });
        Console.WriteLine("✅ Redis distributed cache configured successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Redis configuration failed, falling back to memory cache: {ex.Message}");
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    Console.WriteLine("ℹ️ Redis disabled, using in-memory cache.");
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddEndpointsApiExplorer();

// ========================================
// SWAGGER CONFIGURATION
// ========================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KQ Alumni Association API",
        Version = "v1.0.0",
        Description = "Enterprise-grade API for managing Kenya Airways Alumni registrations",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "KQ Alumni Team",
            Email = "KQ.Alumni@kenya-airways.com"
        }
    });

    // Resolve route conflicts
    options.ResolveConflictingActions(apiDescriptions =>
    {
        var description = apiDescriptions.First();
        Console.WriteLine($"⚠️ Swagger conflict resolved: Using {description.ActionDescriptor.DisplayName}");
        return description;
    });

    // JWT Security Definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 2. CONFIGURE SETTINGS
// ========================================

var backgroundJobSettings = builder.Configuration
    .GetSection("BackgroundJobs:ApprovalProcessing")
    .Get<BackgroundJobSettings>() ?? new BackgroundJobSettings();

builder.Services.Configure<BackgroundJobSettings>(
    builder.Configuration.GetSection("BackgroundJobs:ApprovalProcessing"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

// ========================================
// 3. ADD INFRASTRUCTURE
// ========================================

builder.Services.AddInfrastructureServices(builder.Configuration);

// ========================================
// 3A. HOSTED SERVICES (Configuration Validation & Monitoring)
// ========================================

// Configuration validator - runs on startup and validates all required settings
builder.Services.AddHostedService<ConfigurationValidator>();

// Rate limiting monitor - tracks and logs rate limiting metrics
builder.Services.AddSingleton<RateLimitMonitor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RateLimitMonitor>());

builder.Services.AddHttpClient();

// ========================================
// 4. DATABASE & HANGFIRE
// ========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

bool isDatabaseAvailable = false;
try
{
    using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
    testConn.Open();
    isDatabaseAvailable = true;
    Console.WriteLine("✅ Database connection successful.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database unavailable: {ex.Message}");
}

if (isDatabaseAvailable)
{
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "Hangfire",
            PrepareSchemaIfNecessary = true
        }));

    var workerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", 5);
    builder.Services.AddHangfireServer(options => options.WorkerCount = workerCount);
}
else
{
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings());
}

// ========================================
// 5. CORS
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// ========================================
// 6. JWT AUTH
// ========================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("HRManager", policy => policy.RequireRole("SuperAdmin", "HRManager"));
    options.AddPolicy("HROfficer", policy => policy.RequireRole("SuperAdmin", "HRManager", "HROfficer"));
});

// ========================================
// 7. HEALTH CHECKS (Enhanced with detailed monitoring)
// ========================================

builder.Services.AddHealthChecks()
    // Database health check with connection testing
    .AddCheck<SqlServerHealthCheck>("database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database", "critical", "ready" })

    // Email/SMTP health check with connectivity testing
    .AddCheck<SmtpHealthCheck>("smtp",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "email", "external" })

    // Legacy email settings check (kept for compatibility)
    .AddCheck<EmailHealthCheck>("email_settings",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "email", "settings" })

    // ERP API health check with timing
    .AddCheck<ErpApiHealthCheck>("erp_api",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "erp", "external" })

    // DbContext check (kept for Hangfire compatibility)
    .AddDbContextCheck<AppDbContext>("ef_core",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database", "ef" });

// ========================================
// 8. BUILD APP
// ========================================

var app = builder.Build();

app.Logger.LogInformation("🌍 CORS Allowed Origins:");
foreach (var origin in builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
{
    app.Logger.LogInformation("   ✓ {Origin}", origin);
}

// ========================================
// 9. PIPELINE
// ========================================

app.UseResponseCompression();
app.UseResponseCaching();
app.UseMiddleware<CacheHeadersMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseCors("AllowFrontend");
app.UseMiddleware<IpWhitelistMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "UAT")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KQ Alumni API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "KQ Alumni API Documentation";
        c.DisplayRequestDuration();
    });

    app.Logger.LogInformation("📚 Swagger UI available at: /swagger");
}

if (isDatabaseAvailable && builder.Configuration.GetValue<bool>("Hangfire:DashboardEnabled", true))
{
    var dashboardPath = builder.Configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        DashboardTitle = "KQ Alumni - Background Jobs"
    });
    app.Logger.LogInformation("📊 Hangfire Dashboard: {DashboardPath}", dashboardPath);
}
else
{
    app.Logger.LogWarning("⚠️ Hangfire Dashboard disabled (database unavailable)");
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ========================================
// 10. HEALTH CHECK ENDPOINTS
// ========================================

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data,
                tags = e.Value.Tags
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        }, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        await context.Response.WriteAsync(result);
    }
}).WithTags("Health");

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    AllowCachingResponses = false
}).WithTags("Health");

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
}).WithTags("Health");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/test", () => new
    {
        status = "healthy",
        message = "API is running!",
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName
    }).WithTags("Test");
}

// ========================================
// 11. MIGRATIONS
// ========================================

if (app.Environment.IsDevelopment() && isDatabaseAvailable)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        if (db.Database.CanConnect())
        {
            db.Database.Migrate();
            app.Logger.LogInformation("✅ Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Failed to apply migrations");
    }
}

// ========================================
// 12. SCHEDULE HANGFIRE JOBS
// ========================================

if (isDatabaseAvailable)
{
    TimeZoneInfo tz;
    try
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById(backgroundJobSettings.TimeZone);
    }
    catch
    {
        app.Logger.LogWarning("⚠️ Timezone '{TimeZone}' not found. Using UTC.", backgroundJobSettings.TimeZone);
        tz = TimeZoneInfo.Utc;
    }

    if (backgroundJobSettings.EnableSmartScheduling)
    {
        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "business-hours", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.BusinessHoursSchedule, new RecurringJobOptions { TimeZone = tz });

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "off-hours", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.OffHoursSchedule, new RecurringJobOptions { TimeZone = tz });

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "weekends", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.WeekendSchedule, new RecurringJobOptions { TimeZone = tz });

        app.Logger.LogInformation("✅ Hangfire jobs scheduled (Smart Scheduling)");
    }
    else
    {
        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "default", job => job.ProcessPendingRegistrations(),
            "*/5 * * * *", new RecurringJobOptions { TimeZone = tz });

        app.Logger.LogInformation("✅ Hangfire jobs scheduled (Default Schedule)");
    }
}

// ========================================
// 13. RATE LIMIT CLEANUP
// ========================================

var window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 60));
RateLimitingMiddleware.StartCleanupTask(window);

// ========================================
// 14. RUN
// ========================================

app.Logger.LogInformation("🚀 KQ Alumni API Starting...");
app.Logger.LogInformation("🌐 Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("📍 Base URL: {BaseUrl}", builder.Configuration["AppSettings:BaseUrl"]);

app.Run();

// ========================================
// HANGFIRE DASHBOARD AUTH
// ========================================

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var env = http.RequestServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsProduction())
            return http.User.Identity?.IsAuthenticated == true && http.User.IsInRole("Admin");

        var ip = http.Connection.RemoteIpAddress;
        return ip != null && (ip.Equals(http.Connection.LocalIpAddress) || ip.ToString() is "127.0.0.1" or "::1");
    }
}
