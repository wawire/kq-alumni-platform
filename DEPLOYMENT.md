# KQ Alumni Platform - Deployment Guide

**Version**: 2.2.0 | **Last Updated**: 2025-11-08 | **Environment**: Production

This guide covers complete deployment to IIS on Windows Server with SQL Server.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Environment Configuration](#environment-configuration)
3. [Database Setup](#database-setup)
4. [Build & Publish](#build--publish)
5. [IIS Deployment](#iis-deployment)
6. [Post-Deployment Verification](#post-deployment-verification)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Component | Version | Download Link |
|-----------|---------|---------------|
| **Windows Server** | 2019+ | - |
| **IIS** | 10+ | Built-in (Enable via Server Manager) |
| **.NET 8.0 Hosting Bundle** | Latest | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Node.js** | 18.17+ | https://nodejs.org/ |
| **SQL Server** | 2019+ | Already at 10.2.155.150 |
| **IIS URL Rewrite Module** | 2.1+ | https://www.iis.net/downloads/microsoft/url-rewrite |
| **IIS Application Request Routing** | 3.0+ | https://www.iis.net/downloads/microsoft/application-request-routing |

### IIS Features Required

Enable these features in Server Manager:

```powershell
# Enable IIS features
Install-WindowsFeature Web-Server, Web-Asp-Net45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Net-Ext45, Web-Mgmt-Console
```

**After installing .NET 8.0 Hosting Bundle**:
```powershell
iisreset
```

---

## Environment Configuration

### 🔴 CRITICAL: Configuration Files

The application **will not start** without properly configured environment files.

### Backend Configuration

**File**: `KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json`

Create this file (it's in .gitignore) with production settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },

  "CorsSettings": {
    "AllowedOrigins": [
      "https://kqalumni.kenya-airways.com"
    ]
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=10.2.155.150;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_ACTUAL_PASSWORD;TrustServerCertificate=true;Encrypt=true;",
    "HangfireConnection": "Server=10.2.155.150;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_ACTUAL_PASSWORD;TrustServerCertificate=true;Encrypt=true;"
  },

  "ErpApi": {
    "BaseUrl": "http://10.2.131.147:7010",
    "Endpoint": "/soa-infra/resources/default/HR_Leavers/RestService/Leavers",
    "Timeout": 30,
    "RetryCount": 3,
    "RetryDelaySeconds": 2,
    "EnableMockMode": false,
    "MockStaffNumbers": []
  },

  "Email": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "KQ.Alumni@kenya-airways.com",
    "Password": "Syntonin&carses@",
    "From": "KQ.Alumni@kenya-airways.com",
    "DisplayName": "Kenya Airways Alumni Relations",
    "TimeoutSeconds": 30,
    "EnableEmailSending": true,
    "UseMockEmailService": false
  },

  "Hangfire": {
    "DashboardPath": "/hangfire",
    "DashboardEnabled": true,
    "WorkerCount": 5,
    "PollingIntervalSeconds": 15
  },

  "RateLimiting": {
    "RequestsPerHour": 100,
    "WindowMinutes": 60,
    "CleanupIntervalMinutes": 60,
    "MaxLoginAttempts": 5,
    "LoginWindowMinutes": 15
  },

  "AppSettings": {
    "BaseUrl": "https://kqalumni.kenya-airways.com"
  },

  "JwtSettings": {
    "SecretKey": "REPLACE-WITH-STRONG-64-CHARACTER-RANDOM-SECRET-KEY-FOR-PRODUCTION-USE",
    "Issuer": "KQAlumniAPI",
    "Audience": "KQAlumniAdmin",
    "ExpirationMinutes": 480
  },

  "BackgroundJobs": {
    "ApprovalProcessing": {
      "BusinessHoursSchedule": "*/2 8-17 * * 1-5",
      "OffHoursSchedule": "*/15 18-23,0-7 * * 1-5",
      "WeekendSchedule": "*/30 * * * 0,6",
      "TimeZone": "E. Africa Standard Time",
      "BatchSize": 100,
      "MaxRetryAttempts": 5,
      "RetryDelayMinutes": 10,
      "EnableSmartScheduling": true
    }
  }
}
```

**⚠️ MUST UPDATE**:
1. `ConnectionStrings` - Replace `YOUR_ACTUAL_PASSWORD` with SQL Server password
2. `JwtSettings.SecretKey` - Generate a strong 64+ character random key
3. `AppSettings.BaseUrl` - Set to production frontend URL (CRITICAL for email links!)

### Frontend Configuration

**File**: `kq-alumni-frontend/.env.production.local`

```bash
# Backend API URL (REQUIRED)
NEXT_PUBLIC_API_URL=https://api-kqalumni.kenya-airways.com

# API Request Timeout (milliseconds)
NEXT_PUBLIC_API_TIMEOUT=30000

# Environment
NEXT_PUBLIC_ENV=production

# Application
NEXT_PUBLIC_APP_NAME=KQ Alumni Association
NEXT_PUBLIC_APP_VERSION=2.2.0

# Feature Flags (disabled for production)
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_DEBUG_MODE=false

# Support Email
NEXT_PUBLIC_SUPPORT_EMAIL=KQ.Alumni@kenya-airways.com

# Logging Level
NEXT_PUBLIC_LOG_LEVEL=info
```

### Configuration Validation

The application will validate configuration on startup. If any required settings are missing or invalid, it will:

✅ **PASS** → Application starts with summary:
```
╔════════════════════════════════════════════════════════════════╗
║          KQ ALUMNI PLATFORM - CONFIGURATION SUMMARY           ║
╠════════════════════════════════════════════════════════════════╣
║ Environment:        Production                                 ║
║ Base URL:           https://kqalumni.kenya-airways.com         ║
║ Database:           10.2.155.150 / KQAlumniDB                  ║
║ Email Sending:      ✅ Enabled                                 ║
╚════════════════════════════════════════════════════════════════╝
```

❌ **FAIL** → Application stops with errors:
```
❌ CONFIGURATION VALIDATION FAILED:
1. AppSettings.BaseUrl is localhost in Production
2. JWT SecretKey is too short (need 32+ characters)
3. Database password contains placeholder value
```

---

## Database Setup

### Step 1: Create Database and User

Connect to SQL Server at `10.2.155.150` with SQL Server Management Studio or `sqlcmd`:

```sql
-- Create database
CREATE DATABASE KQAlumniDB;
GO

-- Create login
CREATE LOGIN kqalumni_user WITH PASSWORD = 'YourSecurePassword123!';
GO

-- Create user in database
USE KQAlumniDB;
GO

CREATE USER kqalumni_user FOR LOGIN kqalumni_user;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER kqalumni_user;
GO
```

**IMPORTANT**: Update the password in `appsettings.Production.json` to match!

### Step 2: Apply Migrations

From your development machine:

```bash
cd KQAlumni.Backend/src/KQAlumni.API

# Build first
dotnet build

# Apply all migrations
dotnet ef database update --connection "Server=10.2.155.150;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=true;"
```

This creates all tables including:
- `AlumniRegistrations`
- `AdminUsers`
- `AuditLogs`
- `EmailLogs` (new in v2.0.0)
- Hangfire tables

### Step 3: Verify Database

```sql
-- Check tables created
USE KQAlumniDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check Hangfire schema
SELECT name FROM sys.schemas WHERE name = 'Hangfire';
```

---

## New Features in v2.2.0

This release includes performance optimizations:

### ERP Verification Optimization
- **Feature**: Eliminated redundant ERP API calls (50% reduction)
- **How it works**: Frontend ERP validation data is saved and reused by backend
- **Benefits**:
  - Faster approval processing (~1-2 seconds improvement)
  - Reduced load on ERP infrastructure
  - Lower network overhead
- **Configuration**: No changes needed - works automatically

---

## Features in v2.1.0

This release includes important resilience improvements:

### ERP Fallback Mode
- **Feature**: Users can now complete registration even when ERP service is unavailable
- **Configuration**: Already enabled - no additional setup required
- **Behavior**: If ERP verification fails, users see "Continue with Manual Review" button
- **Admin Impact**: Flagged registrations appear in admin dashboard with "RequiresManualReview" flag

### Email Verification Resend
- **User Self-Service**: Available at `/resend-verification` page
- **Admin Dashboard**: Resend button available for approved registrations
- **Endpoints**:
  - `POST /api/v1/registrations/resend-verification` (public - by email)
  - `POST /api/v1/admin/registrations/{id}/resend-verification` (admin - by ID)

### Password Change API
- **Endpoint**: `POST /api/v1/admin/change-password`
- **Frontend**: Connected in admin settings page
- **Authentication**: Requires JWT token

**No configuration changes needed** - these features work with existing settings.

---

## Build & Publish

### Backend Build

```powershell
cd KQAlumni.Backend/src/KQAlumni.API

# Restore NuGet packages
dotnet restore

# Publish to output folder
dotnet publish -c Release -o ./publish

# Verify output
dir ./publish
```

Expected output folder contains:
- `KQAlumni.API.dll`
- `appsettings.json`
- `web.config`
- All dependencies

### Frontend Build

```bash
cd kq-alumni-frontend

# Install dependencies
npm ci --production

# Build for production
npm run build
```

Expected output:
- `.next/standalone/` - Server files
- `.next/static/` - Static assets
- `public/` - Public assets

---

## IIS Deployment

### Backend Deployment

#### 1. Create Directory Structure

```powershell
# Create directories
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-api"
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-api\logs"
```

#### 2. Copy Published Files

```powershell
# Copy all files from publish folder
Copy-Item -Path ".\KQAlumni.Backend\src\KQAlumni.API\publish\*" -Destination "C:\inetpub\kqalumni-api\" -Recurse -Force

# Copy production config (CRITICAL!)
Copy-Item -Path ".\KQAlumni.Backend\src\KQAlumni.API\appsettings.Production.json" -Destination "C:\inetpub\kqalumni-api\"
```

#### 3. Set Permissions

```powershell
icacls "C:\inetpub\kqalumni-api" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

#### 4. Create Application Pool

```powershell
# Create app pool
New-WebAppPool -Name "KQAlumniAPI"

# Set to No Managed Code (.NET Core uses Kestrel)
Set-ItemProperty "IIS:\AppPools\KQAlumniAPI" -Name managedRuntimeVersion -Value ""

# Set identity
Set-ItemProperty "IIS:\AppPools\KQAlumniAPI" -Name processModel.identityType -Value "ApplicationPoolIdentity"
```

#### 5. Create IIS Website

```powershell
# Create site
New-Website -Name "KQAlumni-API" `
  -PhysicalPath "C:\inetpub\kqalumni-api" `
  -ApplicationPool "KQAlumniAPI" `
  -Port 80 `
  -HostHeader "api-kqalumni.kenya-airways.com"

# Start the site
Start-Website -Name "KQAlumni-API"
```

#### 6. Set Environment Variable

**CRITICAL**: Tell .NET to use Production settings

```powershell
# Via Application Pool Environment Variables (IIS Manager)
# Application Pool > KQAlumniAPI > Advanced Settings > Environment Variables
# Add: ASPNETCORE_ENVIRONMENT = Production
```

Or via PowerShell (requires IIS Administration PowerShell module):

```powershell
Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST' `
  -filter "system.applicationHost/applicationPools/add[@name='KQAlumniAPI']/environmentVariables" `
  -name "." `
  -value @{name='ASPNETCORE_ENVIRONMENT';value='Production'}
```

---

### Frontend Deployment

#### 1. Create Directory Structure

```powershell
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend"
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend\logs"
```

#### 2. Copy Built Files

```powershell
# Copy standalone server
Copy-Item -Path ".\kq-alumni-frontend\.next\standalone\*" -Destination "C:\inetpub\kqalumni-frontend\" -Recurse -Force

# Copy static files
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend\.next"
Copy-Item -Path ".\kq-alumni-frontend\.next\static" -Destination "C:\inetpub\kqalumni-frontend\.next\static\" -Recurse -Force

# Copy public assets
Copy-Item -Path ".\kq-alumni-frontend\public\*" -Destination "C:\inetpub\kqalumni-frontend\public\" -Recurse -Force

# Copy web.config
Copy-Item -Path ".\kq-alumni-frontend\web.config" -Destination "C:\inetpub\kqalumni-frontend\"

# Copy production environment file
Copy-Item -Path ".\kq-alumni-frontend\.env.production.local" -Destination "C:\inetpub\kqalumni-frontend\"
```

#### 3. Install Node Modules

```powershell
cd C:\inetpub\kqalumni-frontend
npm ci --production
```

#### 4. Set Permissions

```powershell
icacls "C:\inetpub\kqalumni-frontend" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

#### 5. Create Application Pool & Website

```powershell
# Create app pool
New-WebAppPool -Name "KQAlumniFrontend"
Set-ItemProperty "IIS:\AppPools\KQAlumniFrontend" -Name managedRuntimeVersion -Value ""

# Create website
New-Website -Name "KQAlumni-Frontend" `
  -PhysicalPath "C:\inetpub\kqalumni-frontend" `
  -ApplicationPool "KQAlumniFrontend" `
  -Port 80 `
  -HostHeader "kqalumni.kenya-airways.com"

# Start
Start-Website -Name "KQAlumni-Frontend"
```

---

## Post-Deployment Verification

### 1. Backend Health Check

```bash
curl https://api-kqalumni.kenya-airways.com/health
```

**Expected Response**:
```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "status": "Healthy",
      "description": "Database is healthy (responded in 45ms)"
    },
    "smtp": {
      "status": "Healthy",
      "description": "SMTP server is reachable (connected in 120ms)"
    },
    "erp_api": {
      "status": "Healthy",
      "description": "ERP API is reachable (responded in 350ms)"
    }
  },
  "totalDuration": "00:00:00.5234567"
}
```

### 2. Check Application Logs

```powershell
# Backend logs
Get-Content "C:\inetpub\kqalumni-api\logs\stdout_*.log" -Tail 50

# Look for:
# "✅ Configuration validation passed successfully"
# "📧 Email Service Initialized"
# "🔍 Starting configuration validation..."
```

### 3. Test Swagger API

Visit: `https://api-kqalumni.kenya-airways.com/swagger`

Should display API documentation.

### 4. Test Frontend

Visit: `https://kqalumni.kenya-airways.com`

Should display homepage.

### 5. End-to-End Test

1. **Register**: Fill out registration form
2. **Check Email Logs**:
   ```sql
   SELECT * FROM EmailLogs ORDER BY SentAt DESC;
   ```
3. **Verify Hangfire**: Visit `/hangfire` dashboard
4. **Check Email**: Verify email received with CORRECT verification link
5. **Click Link**: Verify email verification works

---

## Troubleshooting

### Configuration Validation Fails

**Symptoms**: Application won't start, IIS shows 500 error

**Diagnosis**:
```powershell
# Check logs
Get-Content "C:\inetpub\kqalumni-api\logs\stdout_*.log" -Tail 100
```

**Common Causes**:
- `appsettings.Production.json` missing
- Placeholder values not updated (YOUR_ACTUAL_PASSWORD, etc.)
- JWT SecretKey too short (<32 characters)
- AppSettings.BaseUrl is localhost

**Solution**: Fix configuration values and restart app pool

---

### Database Connection Fails

**Symptoms**: Health check shows database unhealthy

**Diagnosis**:
```powershell
# Test connection from server
sqlcmd -S 10.2.155.150 -U kqalumni_user -P "YourPassword" -Q "SELECT 1"
```

**Common Causes**:
- SQL Server not allowing remote connections
- Firewall blocking port 1433
- Wrong password in connection string
- User not granted permissions

**Solution**:
```sql
-- Verify user exists
USE KQAlumniDB;
SELECT name FROM sys.database_principals WHERE name = 'kqalumni_user';

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER kqalumni_user;
```

---

### Email Not Sending

**Symptoms**: No emails received, EmailLogs shows Failed status

**Diagnosis**:
```sql
SELECT TOP 10 * FROM EmailLogs WHERE Status = 'Failed' ORDER BY SentAt DESC;
```

**Common Causes**:
- SMTP credentials incorrect
- SMTP server unreachable (port 587 blocked)
- AppSettings.BaseUrl is localhost (email links broken)

**Solution**:
1. Test SMTP from server: Use Telnet to port 587
2. Verify credentials in `appsettings.Production.json`
3. Check EmailLogs table for specific error messages

---

### Frontend Shows "API Connection Failed"

**Symptoms**: Registration form can't submit

**Diagnosis**:
- Open browser console (F12)
- Look for CORS errors or 404s

**Common Causes**:
- `.env.production.local` not copied to server
- NEXT_PUBLIC_API_URL incorrect
- Backend not running
- CORS not allowing frontend domain

**Solution**:
1. Verify `.env.production.local` exists in deployment folder
2. Restart frontend app pool
3. Check backend CORS settings in `appsettings.Production.json`

---

### High CPU or Memory Usage

**Backend**:
```powershell
# Check Hangfire worker count
# Reduce in appsettings.Production.json if needed
"Hangfire": {
  "WorkerCount": 3  // Reduce from 5
}
```

**Frontend**:
```powershell
# Increase Node.js memory limit in web.config or start script
node --max_old_space_size=4096 server.js
```

---

## Security Checklist

Before going to production:

- [ ] HTTPS configured with valid SSL certificates
- [ ] Strong SQL Server password set
- [ ] JWT secret key is 64+ characters and unique
- [ ] SMTP password secured (consider using App Password)
- [ ] Firewall configured (only ports 80, 443 open)
- [ ] Swagger disabled in production (`DashboardEnabled = false`)
- [ ] Hangfire dashboard authentication configured
- [ ] Regular database backups scheduled
- [ ] Monitoring and alerting set up
- [ ] Rate limiting configured appropriately
- [ ] AppSettings.BaseUrl uses production URL (not localhost!)

---

## Maintenance

### Update Backend

```powershell
# Stop app pool
Stop-WebAppPool -Name "KQAlumniAPI"

# Backup
Copy-Item "C:\inetpub\kqalumni-api" "C:\backups\kqalumni-api-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# Deploy new version
Copy-Item ".\publish\*" "C:\inetpub\kqalumni-api\" -Recurse -Force

# Start app pool
Start-WebAppPool -Name "KQAlumniAPI"
```

### Update Frontend

```powershell
# Stop website
Stop-Website -Name "KQAlumni-Frontend"

# Backup
Copy-Item "C:\inetpub\kqalumni-frontend" "C:\backups\kqalumni-frontend-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# Deploy new version
Copy-Item ".\build-output\*" "C:\inetpub\kqalumni-frontend\" -Recurse -Force

# Reinstall dependencies
cd C:\inetpub\kqalumni-frontend
npm ci --production

# Start website
Start-Website -Name "KQAlumni-Frontend"
```

### Database Backups

```sql
-- Full backup
BACKUP DATABASE KQAlumniDB
TO DISK = 'C:\Backups\KQAlumniDB.bak'
WITH FORMAT, INIT, COMPRESSION;
```

---

## Useful Commands

```powershell
# Restart everything
iisreset

# Restart specific app pool
Restart-WebAppPool -Name "KQAlumniAPI"

# View app pool status
Get-WebAppPoolState -Name "KQAlumniAPI"

# View all websites
Get-Website

# Check .NET Runtime
dotnet --list-runtimes

# Check Node.js
node --version

# View IIS logs
Get-EventLog -LogName Application -Source "IIS*" -Newest 20

# Monitor Hangfire jobs
# Visit: https://api-kqalumni.kenya-airways.com/hangfire
```

---

## Support

**Configuration Issues**: Review configuration validation errors in logs
**Monitoring**: See [MONITORING_AND_IMPROVEMENTS.md](MONITORING_AND_IMPROVEMENTS.md)
**Email**: KQ.Alumni@kenya-airways.com

---

**Last Updated**: 2025-11-08 | **Version**: 2.2.0
