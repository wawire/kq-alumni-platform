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
using Microsoft.AspNetCore.OpenApi;
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

// 1. CONFIGURE SERVICES

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

// RESPONSE COMPRESSION

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

// RESPONSE CACHING & DISTRIBUTED CACHE

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

// SWAGGER CONFIGURATION

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

// 2. CONFIGURE SETTINGS

var backgroundJobSettings = builder.Configuration
    .GetSection("BackgroundJobs:ApprovalProcessing")
    .Get<BackgroundJobSettings>() ?? new BackgroundJobSettings();

builder.Services.Configure<BackgroundJobSettings>(
    builder.Configuration.GetSection("BackgroundJobs:ApprovalProcessing"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

// 3. ADD INFRASTRUCTURE

builder.Services.AddInfrastructureServices(builder.Configuration);

// 3A. HOSTED SERVICES (Configuration Validation & Monitoring)

// Configuration validator - runs on startup and validates all required settings
builder.Services.AddHostedService<ConfigurationValidator>();

// Rate limiting monitor - tracks and logs rate limiting metrics
builder.Services.AddSingleton<RateLimitMonitor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RateLimitMonitor>());

builder.Services.AddHttpClient();

// 4. DATABASE & HANGFIRE

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("🔌 DATABASE CONNECTION TEST");
Console.WriteLine("═══════════════════════════════════════════════════════");

// Sanitize connection string for logging (hide sensitive data)
var sanitizedConnStr = System.Text.RegularExpressions.Regex.Replace(
    connectionString,
    @"Password=[^;]+",
    "Password=***",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
Console.WriteLine($"📋 Connection String: {sanitizedConnStr}");

bool isDatabaseAvailable = false;
string databaseName = "Unknown";
string serverName = "Unknown";

try
{
    using (var testConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
    {
        Console.WriteLine("🔄 Opening database connection...");
        testConn.Open();

        serverName = testConn.DataSource;
        databaseName = testConn.Database;

        Console.WriteLine($"📊 Server: {serverName}");
        Console.WriteLine($"🗄️  Database: {databaseName}");
        Console.WriteLine($"📝 Server Version: {testConn.ServerVersion}");

        using var cmd = testConn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        cmd.ExecuteScalar();
        testConn.Close();
    }

    Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
    System.Threading.Thread.Sleep(100);

    isDatabaseAvailable = true;
    Console.WriteLine("✅ Database connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    Console.WriteLine($"⚠️  Error Type: {ex.GetType().Name}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"⚠️  Inner Error: {ex.InnerException.Message}");
    }
}

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("⚙️  HANGFIRE CONFIGURATION");
Console.WriteLine("═══════════════════════════════════════════════════════");

if (isDatabaseAvailable)
{
    Console.WriteLine("💾 Storage Type: SQL Server");
    Console.WriteLine($"📊 Server: {serverName}");
    Console.WriteLine($"🗄️  Database: {databaseName}");
    Console.WriteLine("📝 Schema: Hangfire");

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
    Console.WriteLine($"👷 Worker Count: {workerCount}");
    builder.Services.AddHangfireServer(options => options.WorkerCount = workerCount);
    Console.WriteLine("✅ Hangfire server configured successfully");
}
else
{
    Console.WriteLine("❌ Hangfire disabled (database unavailable)");
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings());
}

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine();

// 5. CORS

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

// 6. JWT AUTH

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

// 7. HEALTH CHECKS (Enhanced with detailed monitoring)

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

// 8. BUILD APP

var app = builder.Build();

app.Logger.LogInformation("🌍 CORS Allowed Origins:");
foreach (var origin in builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
{
    app.Logger.LogInformation("   ✓ {Origin}", origin);
}

// 9. PIPELINE

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

// MINIMAL API: Email Verification Endpoint (Isolated to prevent Swagger conflicts)
app.MapGet("/api/v1/verify/{token}", async (
    string token,
    AppDbContext context,
    KQAlumni.Core.Interfaces.ITokenService tokenService,
    ILogger<Program> logger) =>
{
    try
    {
        // Step 1: Validate token format
        if (string.IsNullOrWhiteSpace(token) || !tokenService.ValidateTokenFormat(token))
        {
            logger.LogWarning("Invalid token format: {Token}", token);
            return Results.Problem(
                title: "Invalid Verification Token",
                detail: "The verification token format is invalid.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Step 2: Find registration by token
        var registration = await context.AlumniRegistrations
            .FirstOrDefaultAsync(r => r.EmailVerificationToken == token);

        if (registration == null)
        {
            logger.LogWarning("Token not found: {Token}", token);
            return Results.Problem(
                title: "Invalid Verification Token",
                detail: "This verification token does not exist or has already been used.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Step 3: Check if token expired
        if (registration.EmailVerificationTokenExpiry.HasValue &&
            registration.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            logger.LogWarning(
                "Token expired for registration {Id}. Expired at: {Expiry}",
                registration.Id,
                registration.EmailVerificationTokenExpiry);

            return Results.Problem(
                title: "Verification Link Expired",
                detail: "This verification link has expired. Please contact KQ.Alumni@kenya-airways.com for assistance.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Step 4: Check if already verified
        if (registration.EmailVerified)
        {
            logger.LogInformation(
                "Email already verified for registration {Id}. Redirecting to dashboard.",
                registration.Id);

            return Results.Redirect($"/dashboard?id={registration.Id}");
        }

        // Step 5: Mark as verified
        registration.EmailVerified = true;
        registration.EmailVerifiedAt = DateTime.UtcNow;
        registration.RegistrationStatus = KQAlumni.Core.Enums.RegistrationStatus.Active.ToString();
        registration.EmailVerificationToken = null; // One-time use
        registration.EmailVerificationTokenExpiry = null;
        registration.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Email verified successfully for registration {Id} ({StaffNumber})",
            registration.Id,
            registration.StaffNumber);

        // Redirect to dashboard with verification success indicator
        return Results.Redirect($"/dashboard?id={registration.Id}&verified=true");
    }
    catch (DbUpdateException dbEx)
    {
        logger.LogError(dbEx, "Database error during email verification with token: {Token}", token);
        return Results.Problem(
            title: "Database Error",
            detail: "An error occurred while verifying your email. Please try again or contact support.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error verifying email with token: {Token}", token);
        return Results.Problem(
            title: "Verification Error",
            detail: "An unexpected error occurred while verifying your email. Please try again or contact support.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("VerifyEmail")
.WithTags("Verification")
.WithOpenApi(operation =>
{
    operation.Summary = "Verify email using token from approval email";
    operation.Description = @"
FLOW:
1. Validates token format
2. Retrieves registration record
3. Checks token expiry
4. Marks email as verified
5. Updates registration status to Active
6. Clears token for one-time use
7. Redirects to dashboard";
    return operation;
})
.Produces(StatusCodes.Status302Found)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapControllers();

// 10. HEALTH CHECK ENDPOINTS

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

// 11. MIGRATIONS

if (app.Environment.IsDevelopment() && isDatabaseAvailable)
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("📦 DATABASE MIGRATIONS");
    Console.WriteLine("═══════════════════════════════════════════════════════");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        if (db.Database.CanConnect())
        {
            Console.WriteLine("🔄 Applying pending migrations...");
            var pendingMigrations = db.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"📋 Found {pendingMigrations.Count} pending migration(s):");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"   • {migration}");
                }
            }
            else
            {
                Console.WriteLine("✅ Database is up to date - no pending migrations");
            }

            db.Database.Migrate();

            var appliedMigrations = db.Database.GetAppliedMigrations().ToList();
            Console.WriteLine($"✅ Total applied migrations: {appliedMigrations.Count}");
            app.Logger.LogInformation("✅ Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
        app.Logger.LogError(ex, "❌ Failed to apply migrations");
    }

    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine();
}

// 12. SCHEDULE HANGFIRE JOBS

if (isDatabaseAvailable)
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("⏰ BACKGROUND JOB SCHEDULING");
    Console.WriteLine("═══════════════════════════════════════════════════════");

    TimeZoneInfo tz;
    try
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById(backgroundJobSettings.TimeZone);
        Console.WriteLine($"🌍 Timezone: {backgroundJobSettings.TimeZone}");
    }
    catch
    {
        app.Logger.LogWarning("⚠️ Timezone '{TimeZone}' not found. Using UTC.", backgroundJobSettings.TimeZone);
        Console.WriteLine("⚠️  Timezone not found, using UTC");
        tz = TimeZoneInfo.Utc;
    }

    if (backgroundJobSettings.EnableSmartScheduling)
    {
        Console.WriteLine("📊 Smart Scheduling: ENABLED");
        Console.WriteLine($"   • Business Hours: {backgroundJobSettings.BusinessHoursSchedule}");
        Console.WriteLine($"   • Off Hours: {backgroundJobSettings.OffHoursSchedule}");
        Console.WriteLine($"   • Weekends: {backgroundJobSettings.WeekendSchedule}");

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
        Console.WriteLine("✅ 3 recurring jobs scheduled successfully");
    }
    else
    {
        Console.WriteLine("📊 Smart Scheduling: DISABLED");
        Console.WriteLine("   • Default Schedule: Every 5 minutes (*/5 * * * *)");

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "default", job => job.ProcessPendingRegistrations(),
            "*/5 * * * *", new RecurringJobOptions { TimeZone = tz });

        app.Logger.LogInformation("✅ Hangfire jobs scheduled (Default Schedule)");
        Console.WriteLine("✅ 1 recurring job scheduled successfully");
    }

    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine();
}

// 13. RATE LIMIT CLEANUP

var window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 60));
RateLimitingMiddleware.StartCleanupTask(window);

// 14. DISPLAY ALL ENDPOINTS

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("🌐 APPLICATION URLS & ENDPOINTS");
Console.WriteLine("═══════════════════════════════════════════════════════");

var baseUrl = builder.Configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
var urls = builder.Configuration["urls"] ?? builder.Configuration["ASPNETCORE_URLS"] ?? baseUrl;
Console.WriteLine($"📍 Listening on: {urls}");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine();

Console.WriteLine("📚 DOCUMENTATION & MANAGEMENT");
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "UAT")
{
    Console.WriteLine($"   • Swagger UI: {urls}/swagger");
    Console.WriteLine($"   • OpenAPI JSON: {urls}/swagger/v1/swagger.json");
}
else
{
    Console.WriteLine("   • Swagger: Disabled (Production)");
}

if (isDatabaseAvailable && builder.Configuration.GetValue<bool>("Hangfire:DashboardEnabled", true))
{
    var dashboardPath = builder.Configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
    Console.WriteLine($"   • Hangfire Dashboard: {urls}{dashboardPath}");
}

Console.WriteLine();
Console.WriteLine("🏥 HEALTH CHECK ENDPOINTS");
Console.WriteLine($"   • Full Health Check: {urls}/health");
Console.WriteLine($"   • Readiness Probe: {urls}/health/ready");
Console.WriteLine($"   • Liveness Probe: {urls}/health/live");

if (app.Environment.IsDevelopment())
{
    Console.WriteLine($"   • Test Endpoint: {urls}/api/test");
}

Console.WriteLine();
Console.WriteLine("🔐 AUTHENTICATION ENDPOINTS");
Console.WriteLine($"   • POST   {urls}/api/auth/login");
Console.WriteLine($"   • POST   {urls}/api/auth/refresh");
Console.WriteLine($"   • POST   {urls}/api/auth/logout");

Console.WriteLine();
Console.WriteLine("📝 REGISTRATION ENDPOINTS");
Console.WriteLine($"   • POST   {urls}/api/registration/submit");
Console.WriteLine($"   • GET    {urls}/api/registration");
Console.WriteLine($"   • GET    {urls}/api/registration/{{id}}");
Console.WriteLine($"   • PUT    {urls}/api/registration/{{id}}/approve");
Console.WriteLine($"   • PUT    {urls}/api/registration/{{id}}/reject");
Console.WriteLine($"   • GET    {urls}/api/registration/stats");
Console.WriteLine($"   • POST   {urls}/api/registration/bulk-approve");

Console.WriteLine();
Console.WriteLine("👥 USER MANAGEMENT ENDPOINTS");
Console.WriteLine($"   • POST   {urls}/api/users");
Console.WriteLine($"   • GET    {urls}/api/users");
Console.WriteLine($"   • GET    {urls}/api/users/{{id}}");
Console.WriteLine($"   • PUT    {urls}/api/users/{{id}}");
Console.WriteLine($"   • DELETE {urls}/api/users/{{id}}");
Console.WriteLine($"   • PUT    {urls}/api/users/{{id}}/role");

Console.WriteLine();
Console.WriteLine("📊 REPORTING ENDPOINTS");
Console.WriteLine($"   • GET    {urls}/api/reports/registrations");
Console.WriteLine($"   • GET    {urls}/api/reports/dashboard");
Console.WriteLine($"   • GET    {urls}/api/reports/export");

Console.WriteLine();
Console.WriteLine("🔔 EMAIL & NOTIFICATION ENDPOINTS");
Console.WriteLine($"   • GET    {urls}/api/email/logs");
Console.WriteLine($"   • POST   {urls}/api/email/test");

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine();

app.Logger.LogInformation("🚀 KQ Alumni API Starting...");
app.Logger.LogInformation("🌐 Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("📍 Base URL: {BaseUrl}", baseUrl);

Console.WriteLine("✅ Application is ready to accept requests");
Console.WriteLine("Press Ctrl+C to shut down");
Console.WriteLine();

app.Run();

// HANGFIRE DASHBOARD AUTH

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
