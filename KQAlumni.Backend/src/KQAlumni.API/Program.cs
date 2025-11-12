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

// LOAD MOCK DATA IN DEVELOPMENT
if (builder.Environment.IsDevelopment())
{
    var mockDataPath = Path.Combine(builder.Environment.ContentRootPath, "MockData", "mock-employees.json");
    if (File.Exists(mockDataPath))
    {
        builder.Configuration.AddJsonFile(mockDataPath, optional: true, reloadOnChange: true);
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
        Console.WriteLine($"[CONFIG] Loaded mock employee data from: {mockDataPath}");
    }
    else
    {
        Console.WriteLine($"[CONFIG] Mock employee data file not found at: {mockDataPath}");
    }
}

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

        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("[CACHE] Redis distributed cache configured successfully.");
        }
    }
    catch (Exception ex)
    {
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine($"[CACHE] Redis configuration failed, falling back to memory cache: {ex.Message}");
        }
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("[CACHE] Redis disabled, using in-memory cache.");
    }
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

// Configure JwtSettings with validation
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
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

bool isDatabaseAvailable = false;
string databaseName = "Unknown";
string serverName = "Unknown";

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("DATABASE CONNECTION TEST");
    Console.WriteLine("═══════════════════════════════════════════════════════");

    // Sanitize connection string for logging (hide sensitive data)
    var sanitizedConnStr = System.Text.RegularExpressions.Regex.Replace(
        connectionString,
        @"Password=[^;]+",
        "Password=***",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    Console.WriteLine($"Connection String: {sanitizedConnStr}");
}

try
{
    using (var testConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
    {
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("Opening database connection...");
        }
        testConn.Open();

        serverName = testConn.DataSource;
        databaseName = testConn.Database;

        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine($"Server: {serverName}");
            Console.WriteLine($"Database: {databaseName}");
            Console.WriteLine($"Server Version: {testConn.ServerVersion}");
        }

        using var cmd = testConn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        cmd.ExecuteScalar();
        testConn.Close();
    }

    Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
    System.Threading.Thread.Sleep(100);

    isDatabaseAvailable = true;

    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("[SUCCESS] Database connection successful");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Database connection failed: {ex.Message}");
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine($"Error Type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
        }
    }
}

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine();
}

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("HANGFIRE CONFIGURATION");
    Console.WriteLine("═══════════════════════════════════════════════════════");
}

if (isDatabaseAvailable)
{
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("Storage Type: SQL Server");
        Console.WriteLine($"Server: {serverName}");
        Console.WriteLine($"Database: {databaseName}");
        Console.WriteLine("Schema: Hangfire");
    }

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
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine($"Worker Count: {workerCount}");
    }
    builder.Services.AddHangfireServer(options => options.WorkerCount = workerCount);

    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("[SUCCESS] Hangfire server configured");
    }
}
else
{
    Console.WriteLine("[WARNING] Hangfire disabled (database unavailable)");
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings());
}

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine();
}

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
    // Configuration validation check (runs on startup)
    .AddCheck<ConfigurationHealthCheck>("configuration",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "configuration", "critical", "startup" })

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

// Enable endpoint routing (required for controllers to work)
app.UseRouting();

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

// 11. MIGRATIONS & SEEDING (Production-Ready - Works in ALL Environments)

if (isDatabaseAvailable)
{
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("DATABASE MIGRATIONS");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        if (db.Database.CanConnect())
        {
            var pendingMigrations = db.Database.GetPendingMigrations().ToList();

            if (app.Environment.IsDevelopment())
            {
                Console.WriteLine("Applying pending migrations...");

                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Found {pendingMigrations.Count} pending migration(s):");
                    foreach (var migration in pendingMigrations)
                    {
                        Console.WriteLine($"  - {migration}");
                    }
                }
                else
                {
                    Console.WriteLine("Database is up to date - no pending migrations");
                }
            }

            db.Database.Migrate();

            if (app.Environment.IsDevelopment())
            {
                var appliedMigrations = db.Database.GetAppliedMigrations().ToList();
                Console.WriteLine($"Total applied migrations: {appliedMigrations.Count}");
            }

            app.Logger.LogInformation("Database migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Migration failed: {ex.Message}");
        app.Logger.LogError(ex, "Failed to apply migrations");
    }

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    // SEED ADMIN USERS
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("ADMIN USER SEEDING");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    try
    {
        await DbSeeder.SeedInitialAdminUsersAsync(app.Services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Admin seeding failed: {ex.Message}");
        app.Logger.LogError(ex, "Failed to seed admin users");
    }

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    // VERIFY EMAIL TEMPLATES (Migration seeds them, this verifies)
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("EMAIL TEMPLATE VERIFICATION");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    try
    {
        using var templateScope = app.Services.CreateScope();
        var templateService = templateScope.ServiceProvider.GetRequiredService<KQAlumni.Core.Interfaces.IEmailTemplateService>();

        var existingTemplates = await templateService.GetAllTemplatesAsync();

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Found {existingTemplates.Count} email template(s) in database");
        }

        if (existingTemplates.Count == 0)
        {
            if (app.Environment.IsDevelopment())
            {
                Console.WriteLine("[WARNING] No templates found - attempting to seed...");
                Console.WriteLine("NOTE: Templates should be seeded by migration automatically");
            }

            await templateService.SeedDefaultTemplatesAsync();

            var recheck = await templateService.GetAllTemplatesAsync();
            if (app.Environment.IsDevelopment())
            {
                if (recheck.Count > 0)
                {
                    Console.WriteLine($"[SUCCESS] Seeded {recheck.Count} email template(s)");
                }
                else
                {
                    Console.WriteLine("[ERROR] Seeding failed - no templates created");
                }
            }
        }
        else if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("[SUCCESS] Email templates verified:");
            foreach (var template in existingTemplates.OrderBy(t => t.TemplateKey))
            {
                var status = template.IsActive ? "Active" : "Inactive";
                var systemDefault = template.IsSystemDefault ? "[System]" : "";
                Console.WriteLine($"  ✓ {template.TemplateKey}: {template.Name} ({status}) {systemDefault}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Email template verification failed: {ex.Message}");
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");
            }
        }
        app.Logger.LogError(ex, "Failed to verify email templates");
    }

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
    }
}

// 12. SCHEDULE HANGFIRE JOBS

if (isDatabaseAvailable)
{
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("BACKGROUND JOB SCHEDULING");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    TimeZoneInfo tz;
    try
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById(backgroundJobSettings.TimeZone);
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine($"Timezone: {backgroundJobSettings.TimeZone}");
        }
    }
    catch
    {
        app.Logger.LogWarning("Timezone '{TimeZone}' not found. Using UTC.", backgroundJobSettings.TimeZone);
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("[WARNING] Timezone not found, using UTC");
        }
        tz = TimeZoneInfo.Utc;
    }

    if (backgroundJobSettings.EnableSmartScheduling)
    {
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("Smart Scheduling: ENABLED");
            Console.WriteLine($"  Business Hours: {backgroundJobSettings.BusinessHoursSchedule}");
            Console.WriteLine($"  Off Hours: {backgroundJobSettings.OffHoursSchedule}");
            Console.WriteLine($"  Weekends: {backgroundJobSettings.WeekendSchedule}");
        }

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "business-hours", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.BusinessHoursSchedule, new RecurringJobOptions { TimeZone = tz });

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "off-hours", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.OffHoursSchedule, new RecurringJobOptions { TimeZone = tz });

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "weekends", job => job.ProcessPendingRegistrations(),
            backgroundJobSettings.WeekendSchedule, new RecurringJobOptions { TimeZone = tz });

        app.Logger.LogInformation("Hangfire jobs scheduled (Smart Scheduling)");

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("[SUCCESS] 3 recurring jobs scheduled");
        }
    }
    else
    {
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("Smart Scheduling: DISABLED");
            Console.WriteLine("  Default Schedule: Every 5 minutes (*/5 * * * *)");
        }

        RecurringJob.AddOrUpdate<ApprovalProcessingJob>(
            "default", job => job.ProcessPendingRegistrations(),
            "*/5 * * * *", new RecurringJobOptions { TimeZone = tz });

        app.Logger.LogInformation("Hangfire jobs scheduled (Default Schedule)");

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("[SUCCESS] 1 recurring job scheduled");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
    }
}

// 13. RATE LIMIT CLEANUP

var window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 60));
RateLimitingMiddleware.StartCleanupTask(window);

// 14. DISPLAY ALL ENDPOINTS

var baseUrl = builder.Configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
var urlsConfig = builder.Configuration["urls"] ?? builder.Configuration["ASPNETCORE_URLS"] ?? baseUrl;
var urlList = urlsConfig.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("APPLICATION URLS & ENDPOINTS");
    Console.WriteLine("═══════════════════════════════════════════════════════");

    Console.WriteLine("Listening on:");
    foreach (var url in urlList)
    {
        Console.WriteLine($"  - {url}");
    }
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine();

    Console.WriteLine("DOCUMENTATION & MANAGEMENT");
    Console.WriteLine($"  Swagger UI:    {urlList[0]}/swagger");
    Console.WriteLine($"  OpenAPI JSON:  {urlList[0]}/swagger/v1/swagger.json");

    if (isDatabaseAvailable && builder.Configuration.GetValue<bool>("Hangfire:DashboardEnabled", true))
    {
        var dashboardPath = builder.Configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
        Console.WriteLine($"  Hangfire:      {urlList[0]}{dashboardPath}");
    }

    Console.WriteLine();
    Console.WriteLine("HEALTH CHECK ENDPOINTS");
    Console.WriteLine($"  GET  {urlList[0]}/health         (Full health check)");
    Console.WriteLine($"  GET  {urlList[0]}/health/ready   (Readiness probe)");
    Console.WriteLine($"  GET  {urlList[0]}/health/live    (Liveness probe)");
    Console.WriteLine($"  GET  {urlList[0]}/api/test       (Test endpoint)");

    Console.WriteLine();
    Console.WriteLine("API ENDPOINTS");
    Console.WriteLine("  Authentication:");
    Console.WriteLine($"    POST   /api/v1/admin/login");
    Console.WriteLine();
    Console.WriteLine("  Registrations:");
    Console.WriteLine($"    POST   /api/v1/registrations");
    Console.WriteLine($"    GET    /api/v1/registrations/status?email={{email}}");
    Console.WriteLine($"    GET    /api/v1/registrations/{{id}}");
    Console.WriteLine($"    GET    /api/v1/registrations/check/staff-number/{{staffNumber}}");
    Console.WriteLine($"    GET    /api/v1/registrations/check/email/{{email}}");
    Console.WriteLine($"    GET    /api/v1/registrations/verify/{{token}}");
    Console.WriteLine();
    Console.WriteLine("  Admin Registrations:");
    Console.WriteLine($"    GET    /api/v1/admin/registrations");
    Console.WriteLine($"    GET    /api/v1/admin/registrations/{{id}}");
    Console.WriteLine($"    PUT    /api/v1/admin/registrations/{{id}}/approve");
    Console.WriteLine($"    PUT    /api/v1/admin/registrations/{{id}}/reject");
    Console.WriteLine($"    GET    /api/v1/admin/statistics");
    Console.WriteLine($"    GET    /api/v1/admin/audit-logs?registrationId={{id}}");

    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine();
}

app.Logger.LogInformation("KQ Alumni API Starting");
app.Logger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Primary URL: {BaseUrl}", urlList[0]);

if (!app.Environment.IsDevelopment())
{
    Console.WriteLine($"KQ Alumni API started - {app.Environment.EnvironmentName}");
    Console.WriteLine($"Listening on: {urlList[0]}");
}

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
