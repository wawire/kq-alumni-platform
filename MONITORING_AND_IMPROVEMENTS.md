# KQ Alumni Platform - Monitoring & Improvements Guide

**Date**: 2025-11-04
**Version**: 2.0.0

This document details all the monitoring, logging, and reliability improvements implemented in the KQ Alumni Platform.

---

## 📊 **Overview of Improvements**

### **1. Email Delivery Tracking** ✅
- **What**: Database logging of all email delivery attempts
- **Why**: Monitor email delivery success rates and troubleshoot failures
- **Impact**: Full visibility into email system performance

### **2. Environment Variable Validation** ✅
- **What**: Startup validation of all required configuration settings
- **Why**: Prevent deployment with invalid or missing configuration
- **Impact**: Fail-fast on startup instead of runtime errors

### **3. Enhanced Health Checks** ✅
- **What**: Comprehensive health checks for SQL Server, SMTP, and ERP
- **Why**: Proactive monitoring of all external dependencies
- **Impact**: Early detection of connectivity or performance issues

### **4. Rate Limiting Monitoring** ✅
- **What**: Background service that tracks and reports rate limiting metrics
- **Why**: Understand API usage patterns and potential abuse
- **Impact**: Data-driven rate limit tuning

---

## 🔍 **1. Email Delivery Tracking**

### **Database Schema**

**New Table**: `EmailLogs`

```sql
CREATE TABLE [dbo].[EmailLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NULL,
    [ToEmail] NVARCHAR(256) NOT NULL,
    [Subject] NVARCHAR(500) NOT NULL,
    [EmailType] NVARCHAR(50) NOT NULL,      -- Confirmation, Approval, Rejection
    [Status] NVARCHAR(50) NOT NULL,         -- Sent, Failed, MockMode
    [ErrorMessage] NVARCHAR(2000) NULL,
    [SmtpServer] NVARCHAR(256) NULL,
    [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DurationMs] INT NULL,                   -- Time taken to send
    [RetryCount] INT NOT NULL DEFAULT 0,
    [Metadata] NVARCHAR(1000) NULL,

    CONSTRAINT [FK_EmailLogs_AlumniRegistrations]
        FOREIGN KEY ([RegistrationId])
        REFERENCES [AlumniRegistrations]([Id])
        ON DELETE SET NULL
);

-- Indexes for performance
CREATE INDEX [IX_EmailLogs_ToEmail] ON [EmailLogs]([ToEmail]);
CREATE INDEX [IX_EmailLogs_Status] ON [EmailLogs]([Status]);
CREATE INDEX [IX_EmailLogs_EmailType] ON [EmailLogs]([EmailType]);
CREATE INDEX [IX_EmailLogs_SentAt] ON [EmailLogs]([SentAt] DESC);
CREATE INDEX [IX_EmailLogs_Status_SentAt] ON [EmailLogs]([Status], [SentAt] DESC);
CREATE INDEX [IX_EmailLogs_RegistrationId_SentAt] ON [EmailLogs]([RegistrationId], [SentAt] DESC);
```

### **New Service**

**File**: `EmailServiceWithTracking.cs`

**Features**:
- ✅ Logs every email attempt (success or failure) to database
- ✅ Tracks email delivery duration in milliseconds
- ✅ Records error messages for failed deliveries
- ✅ Links emails to registration records
- ✅ Supports mock mode tracking
- ✅ Non-blocking logging (doesn't fail email sending if logging fails)

**Usage Example**:
```csharp
// Automatically logs to database
await _emailService.SendConfirmationEmailAsync(
    "John Doe",
    "john@example.com",
    registrationId);
```

**Querying Email Logs**:
```sql
-- Get delivery success rate for last 24 hours
SELECT
    EmailType,
    COUNT(*) as Total,
    SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Successful,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    CAST(SUM(CASE WHEN Status = 'Sent' THEN 1.0 ELSE 0 END) / COUNT(*) * 100 AS DECIMAL(5,2)) as SuccessRate,
    AVG(DurationMs) as AvgDurationMs
FROM EmailLogs
WHERE SentAt >= DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY EmailType;

-- Find failed emails for investigation
SELECT
    ToEmail,
    Subject,
    EmailType,
    ErrorMessage,
    SentAt
FROM EmailLogs
WHERE Status = 'Failed'
  AND SentAt >= DATEADD(DAY, -7, GETUTCDATE())
ORDER BY SentAt DESC;

-- Get emails for a specific registration
SELECT
    EmailType,
    Status,
    SentAt,
    DurationMs,
    ErrorMessage
FROM EmailLogs
WHERE RegistrationId = 'YOUR-GUID-HERE'
ORDER BY SentAt;
```

---

## 🔐 **2. Environment Variable Validation**

### **Configuration Validator Service**

**File**: `ConfigurationValidator.cs`

**Validates on Startup**:

| Configuration | Validation | Severity |
|---------------|-----------|----------|
| **Database Connection String** | ✅ Not empty<br>✅ No placeholder passwords<br>✅ Contains Server & Database | ❌ CRITICAL - Stops startup |
| **JWT Settings** | ✅ SecretKey present<br>✅ SecretKey >= 32 chars<br>⚠️ Not placeholder in Production<br>✅ Issuer & Audience present | ❌ CRITICAL - Stops startup |
| **AppSettings.BaseUrl** | ✅ Not empty<br>❌ Not localhost in Production<br>✅ Valid URL format<br>⚠️ HTTPS in Production | ❌ CRITICAL - Stops startup |
| **Email Settings** | ✅ SMTP server configured<br>✅ Credentials present | ⚠️ WARNING - Logs only |
| **ERP Settings** | ✅ BaseURL present<br>⚠️ Mock mode off in Production | ⚠️ WARNING - Logs only |
| **CORS Settings** | ✅ Origins configured<br>⚠️ No localhost in Production | ⚠️ WARNING - Logs only |

**Startup Behavior**:
```
✅ PASS → Application starts normally with configuration summary
❌ FAIL → Application stops with detailed error messages
```

**Example Output** (Success):
```
╔════════════════════════════════════════════════════════════════╗
║          KQ ALUMNI PLATFORM - CONFIGURATION SUMMARY           ║
╠════════════════════════════════════════════════════════════════╣
║ Environment:        Production                                 ║
║ Base URL:           https://kqalumni-dev.kenya-airways.com     ║
║                                                                ║
║ DATABASE:                                                      ║
║   Server:           10.2.150.23                                ║
║   Database:         KQAlumniDB                                 ║
║                                                                ║
║ EMAIL:                                                         ║
║   SMTP Server:      smtp.office365.com                         ║
║   Sending Enabled:  ✅ Yes                                     ║
║   Mock Mode:        ✅ No (Real Sending)                       ║
║                                                                ║
║ ERP:                                                           ║
║   Mock Mode:        ✅ No (Real API)                           ║
╚════════════════════════════════════════════════════════════════╝
```

**Example Output** (Failure):
```
❌ CONFIGURATION VALIDATION FAILED:
The application cannot start due to missing or invalid configuration.
Please fix the following errors:

1. ❌ AppSettings:BaseUrl is set to localhost in Production environment. Email verification links will not work. Update to production URL (e.g., https://kqalumni-dev.kenya-airways.com)
2. ❌ JwtSettings:SecretKey is too short (16 characters). Must be at least 32 characters (recommended: 64+ characters)
3. ❌ Database connection string contains placeholder password. Update 'YOUR_SQL_PASSWORD_HERE' with actual password in appsettings.Production.json

See ENVIRONMENT_SETUP.md for configuration instructions.
```

**When to Expect Failures**:
- First deployment without proper configuration
- Deploying to new environment
- After updating configuration templates
- Using placeholder values in production

**How to Fix**:
1. Read error messages carefully
2. Refer to `ENVIRONMENT_SETUP.md` for configuration instructions
3. Update `appsettings.Production.json` with correct values
4. Restart the application

---

## 🏥 **3. Enhanced Health Checks**

### **New Health Checks**

#### **A. SQL Server Health Check**

**File**: `SqlServerHealthCheck.cs`

**What it checks**:
- ✅ Database connectivity
- ✅ Query execution (`SELECT 1`)
- ✅ Response time monitoring

**Response**:
```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "status": "Healthy",
      "description": "Database is healthy (responded in 45ms)",
      "data": {
        "server": "10.2.150.23",
        "database": "KQAlumniDB",
        "responseTime": "45ms",
        "status": "Connected"
      }
    }
  }
}
```

**Status Levels**:
- **Healthy**: Connected, responds in < 1000ms
- **Degraded**: Connected, responds in > 1000ms (slow)
- **Unhealthy**: Cannot connect or query fails

#### **B. SMTP Health Check**

**File**: `SmtpHealthCheck.cs`

**What it checks**:
- ✅ SMTP server TCP connectivity
- ✅ Port accessibility
- ✅ Connection time
- ✅ Configuration status

**Response**:
```json
{
  "smtp": {
    "status": "Healthy",
    "description": "SMTP server is reachable (connected in 120ms)",
    "data": {
      "smtpServer": "smtp.office365.com",
      "smtpPort": 587,
      "sslEnabled": true,
      "responseTime": "120ms",
      "status": "Connected"
    }
  }
}
```

**Status Levels**:
- **Healthy**: Connected successfully
- **Degraded**: Timeout (>5s) OR mock mode enabled OR sending disabled
- **Unhealthy**: Connection failed or not configured

#### **C. Enhanced ERP Health Check**

**File**: `ErpApiHealthCheck.cs` (Updated)

**New features**:
- ✅ Response time tracking
- ✅ Slow response warning (>2s)
- ✅ Better error categorization

**Response**:
```json
{
  "erp_api": {
    "status": "Healthy",
    "description": "ERP API is reachable (responded in 350ms)",
    "data": {
      "baseUrl": "http://10.2.131.147:7010",
      "mockMode": false,
      "statusCode": 200,
      "responseTime": "350ms"
    }
  }
}
```

### **Health Check Endpoints**

| Endpoint | Purpose | Tags |
|----------|---------|------|
| `/health` | **Full health report** - All checks | all |
| `/health/ready` | **Readiness probe** - Database only | ready |
| `/health/live` | **Liveness probe** - Always returns | live |

**Usage Examples**:

```bash
# Full health check
curl http://localhost:5295/health

# Just database (for Kubernetes readiness)
curl http://localhost:5295/health/ready

# Specific tags
curl http://localhost:5295/health?tags=external
```

**Monitoring Integration**:
```powershell
# PowerShell script to monitor health
$response = Invoke-RestMethod -Uri "http://localhost:5295/health"
if ($response.status -ne "Healthy") {
    Send-MailMessage -To "admin@kenya-airways.com" `
                    -From "monitoring@kenya-airways.com" `
                    -Subject "KQ Alumni API Health Alert" `
                    -Body "Status: $($response.status)"
}
```

---

## 📈 **4. Rate Limiting Monitoring**

### **Rate Limit Monitor Service**

**File**: `RateLimitMonitor.cs`

**What it does**:
- ✅ Runs as background service
- ✅ Tracks all rate limit hits (429 responses)
- ✅ Tracks successful requests
- ✅ Reports statistics every 15 minutes
- ✅ Identifies top offenders
- ✅ Auto-cleanup of old stats

**Metrics Tracked**:
- Total requests
- Successful requests
- Rate limited requests (429)
- Hit rate percentage
- Per-IP statistics
- Per-endpoint statistics

**Example Report**:
```
╔════════════════════════════════════════════════════════════════╗
║              RATE LIMITING STATISTICS REPORT                  ║
╠════════════════════════════════════════════════════════════════╣
║ Reporting Period: Last 15 minutes                             ║
║                                                                ║
║ SUMMARY:                                                       ║
║   Total Requests:        1,245                                 ║
║   Successful:            1,198                                 ║
║   Rate Limited (429):    47                                    ║
║   Hit Rate:              3.78%                                 ║
╚════════════════════════════════════════════════════════════════╝

⚠️ Top 10 Rate Limited IP Addresses:
    1. 192.168.1.105   | /api/v1/registrations              | Hits:   23 | Last: 14:32:15
    2. 10.2.150.89     | /api/v1/admin/login                | Hits:   12 | Last: 14:31:45
    3. 172.16.0.44     | /api/v1/registrations              | Hits:    8 | Last: 14:30:22
```

**Integration with Middleware**:
```csharp
// In RateLimitingMiddleware.cs - record hits
_rateLimitMonitor.RecordRateLimitHit(ipAddress, endpoint);

// Record successful requests
_rateLimitMonitor.RecordSuccessfulRequest(ipAddress, endpoint);
```

**Querying Statistics**:
```csharp
// Get current stats programmatically
var stats = _rateLimitMonitor.GetStats();

foreach (var stat in stats.Values.OrderByDescending(s => s.HitCount))
{
    Console.WriteLine($"{stat.IpAddress}: {stat.HitCount} hits");
}
```

**Tuning Rate Limits**:

Based on monitoring data, adjust limits in `appsettings.json`:

```json
{
  "RateLimiting": {
    "RequestsPerHour": 100,          // Adjust based on hit rate
    "WindowMinutes": 60,
    "MaxLoginAttempts": 5,           // Adjust based on login patterns
    "LoginWindowMinutes": 15
  }
}
```

**When to adjust**:
- **High hit rate (>10%)**: Limits might be too restrictive
- **Low hit rate (<1%)**: Limits might be too permissive
- **Specific IPs with high hits**: Possible abuse or legitimate heavy use
- **Time-based patterns**: Different limits for business hours vs off-hours

---

## 🚀 **Deployment Instructions**

### **Step 1: Database Migration**

**Create migration**:
```bash
cd KQAlumni.Backend/src/KQAlumni.API
dotnet ef migrations add AddEmailLogging
```

**Apply migration**:
```bash
# Development
dotnet ef database update

# Production (via deployment script)
dotnet ef database update --connection "YOUR_PRODUCTION_CONNECTION_STRING"
```

### **Step 2: Verify Configuration**

**Check required settings in `appsettings.Production.json`**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=10.2.150.23;Database=KQAlumniDB;User Id=kqalumni_user;Password=ACTUAL_PASSWORD;..."
  },
  "JwtSettings": {
    "SecretKey": "STRONG_64_CHAR_RANDOM_KEY_NOT_PLACEHOLDER"
  },
  "AppSettings": {
    "BaseUrl": "https://kqalumni-dev.kenya-airways.com"
  },
  "Email": {
    "EnableEmailSending": true,
    "UseMockEmailService": false
  }
}
```

### **Step 3: Deploy & Monitor**

**After deployment**:
```bash
# 1. Check health endpoints
curl https://kqalumniapi-dev.kenya-airways.com/health

# 2. Check application logs for configuration summary
# Look for: "✅ Configuration validation passed successfully"

# 3. Monitor first hour for rate limiting reports
# Look for: "RATE LIMITING STATISTICS REPORT"

# 4. Verify email logging
# Query: SELECT TOP 10 * FROM EmailLogs ORDER BY SentAt DESC
```

---

## 📊 **Monitoring Dashboard Queries**

### **Email Delivery Metrics**

```sql
-- Daily email delivery success rate
SELECT
    CAST(SentAt AS DATE) as Date,
    EmailType,
    COUNT(*) as Total,
    SUM(CASE WHEN Status = 'Sent' THEN 1 ELSE 0 END) as Successful,
    SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as Failed,
    CAST(AVG(DurationMs) AS INT) as AvgDurationMs
FROM EmailLogs
WHERE SentAt >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY CAST(SentAt AS DATE), EmailType
ORDER BY Date DESC, EmailType;

-- Current hour email statistics
SELECT
    EmailType,
    Status,
    COUNT(*) as Count,
    AVG(DurationMs) as AvgDuration,
    MAX(DurationMs) as MaxDuration
FROM EmailLogs
WHERE SentAt >= DATEADD(HOUR, -1, GETUTCDATE())
GROUP BY EmailType, Status;
```

### **System Health Summary**

Check these periodically:

1. **Database Health**: Response time should be < 100ms
2. **SMTP Health**: Connection time should be < 500ms
3. **ERP Health**: Response time should be < 1000ms
4. **Email Delivery**: Success rate should be > 95%
5. **Rate Limiting**: Hit rate should be < 5%

---

## 🔧 **Troubleshooting**

### **Issue: Configuration Validation Fails on Startup**

**Symptoms**: Application won't start, shows configuration errors

**Solution**:
1. Read error messages carefully
2. Check `appsettings.Production.json` for placeholder values
3. Verify all required settings are present
4. See `ENVIRONMENT_SETUP.md` for detailed instructions

### **Issue: High Email Failure Rate**

**Symptoms**: Many emails in `Failed` status

**Diagnosis**:
```sql
SELECT TOP 20
    ErrorMessage,
    COUNT(*) as Occurrences
FROM EmailLogs
WHERE Status = 'Failed'
  AND SentAt >= DATEADD(DAY, -1, GETUTCDATE())
GROUP BY ErrorMessage
ORDER BY COUNT(*) DESC;
```

**Common Causes**:
- SMTP credentials incorrect
- SMTP server unreachable
- Recipient email invalid
- Network issues

### **Issue: Rate Limiting Too Aggressive**

**Symptoms**: Legitimate users getting 429 errors

**Diagnosis**:
- Check rate limit monitor reports
- Identify patterns in hit statistics

**Solution**:
1. Increase limits in `appsettings.json`
2. Adjust window duration
3. Consider IP whitelist for trusted sources

### **Issue: Slow Health Check Response**

**Symptoms**: `/health` endpoint takes >5 seconds

**Diagnosis**:
- Check individual health check timings
- Look for degraded status

**Common Causes**:
- Database connection slow
- SMTP server timeout
- ERP API slow/unavailable

**Solution**:
- Investigate the specific degraded service
- Consider increasing timeout values
- Check network connectivity

---

## 📚 **Additional Resources**

- **Environment Setup**: See `ENVIRONMENT_SETUP.md`
- **Deployment Guide**: See `DEPLOYMENT_GUIDE.md`
- **API Documentation**: https://kqalumniapi-dev.kenya-airways.com/swagger
- **Health Endpoint**: https://kqalumniapi-dev.kenya-airways.com/health

---

## 🎯 **Success Metrics**

After implementing these improvements, monitor these KPIs:

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Email Delivery Success Rate | > 98% | < 95% |
| Email Send Duration (avg) | < 1000ms | > 3000ms |
| Database Health Response | < 100ms | > 1000ms |
| SMTP Connection Time | < 500ms | > 2000ms |
| ERP API Response Time | < 1000ms | > 3000ms |
| Rate Limit Hit Rate | < 5% | > 15% |
| Configuration Validation | 100% pass | Any failure |

---

**Last Updated**: 2025-11-04
**Version**: 2.0.0
**Implemented By**: Claude Code
