# KQ Alumni Platform - IIS Deployment Guide

**Environment**: DEV
**Last Updated**: 2025-10-30

---

## Quick Reference

| Component | URL |
|-----------|-----|
| **Frontend** | https://kqalumni-dev.kenya-airways.com |
| **Backend API** | https://kqalumniapi-dev.kenya-airways.com |
| **Database** | 10.2.150.23 (SQL Server) |
| **Swagger** | https://kqalumniapi-dev.kenya-airways.com/swagger |
| **Hangfire** | https://kqalumniapi-dev.kenya-airways.com/hangfire |
| **Health Check** | https://kqalumniapi-dev.kenya-airways.com/health |

---

## Prerequisites

### Required Software on Windows Server

1. **IIS 10+** with:
   - ASP.NET Core Module (ANCM)
   - URL Rewrite Module
   - Application Request Routing (ARR)

2. **.NET 8.0 Hosting Bundle**
   Download: https://dotnet.microsoft.com/download/dotnet/8.0
   After install: Run `iisreset`

3. **Node.js 18.17+**
   Download: https://nodejs.org/

4. **iisnode**
   Download: https://github.com/Azure/iisnode/releases

5. **SQL Server 2019+** (on 10.2.150.23)

---

## Step 1: Configuration

### Backend Configuration

**File**: `KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json`

✅ **Already Configured**:
- Email: `KQ.Alumni@kenya-airways.com` / `Syntonin&carses@`
- SMTP: `smtp.office365.com:587`
- URLs: DEV environment

⚠️ **Update SQL Server Password**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=10.2.150.23;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=true;Encrypt=true;",
  "HangfireConnection": "Server=10.2.150.23;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=true;Encrypt=true;"
}
```

### Frontend Configuration

**File**: `kq-alumni-frontend/.env.production.local`

✅ **Already Configured**:
```bash
NEXT_PUBLIC_API_URL=https://kqalumniapi-dev.kenya-airways.com
NEXT_PUBLIC_API_TIMEOUT=30000
NEXT_PUBLIC_ENV=production
```

---

## Step 2: Database Setup

Connect to SQL Server at `10.2.150.23` and run:

```sql
-- Create database
CREATE DATABASE KQAlumniDB;
GO

-- Create login and user
CREATE LOGIN kqalumni_user WITH PASSWORD = 'YourSecurePassword123!';
GO

USE KQAlumniDB;
GO

CREATE USER kqalumni_user FOR LOGIN kqalumni_user;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER kqalumni_user;
GO
```

**Important**: Update the password in `appsettings.Production.json` to match.

---

## Step 3: Build Projects

### Backend Build

```powershell
cd KQAlumni.Backend/src/KQAlumni.API

# Restore dependencies
dotnet restore

# Publish to output folder
dotnet publish -c Release -o ./publish
```

**Output**: `KQAlumni.Backend/src/KQAlumni.API/publish/`

### Frontend Build

```bash
cd kq-alumni-frontend

# Install dependencies
npm install

# Build for production
npm run build
```

**Outputs**:
- `.next/standalone/` - Server files
- `.next/static/` - Static assets
- `public/` - Public assets

---

## Step 4: Deploy to IIS

### Deploy Backend API

```powershell
# 1. Create directory
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-api"
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-api\logs"

# 2. Copy published files
Copy-Item ".\publish\*" "C:\inetpub\kqalumni-api\" -Recurse -Force

# 3. Set permissions
icacls "C:\inetpub\kqalumni-api" /grant "IIS_IUSRS:(OI)(CI)F" /T

# 4. Create Application Pool
New-WebAppPool -Name "KQAlumniAPI"
Set-ItemProperty IIS:\AppPools\KQAlumniAPI -Name managedRuntimeVersion -Value ""

# 5. Create IIS Site
New-Website -Name "KQAlumni-API" `
  -PhysicalPath "C:\inetpub\kqalumni-api" `
  -ApplicationPool "KQAlumniAPI" `
  -Port 80 `
  -HostHeader "kqalumniapi-dev.kenya-airways.com"

# 6. Start the site
Start-Website -Name "KQAlumni-API"
```

### Deploy Frontend

```powershell
# 1. Create directory
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend"
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend\logs"

# 2. Copy built files
# Copy .next/standalone/* to C:\inetpub\kqalumni-frontend\
# Copy .next/static to C:\inetpub\kqalumni-frontend\.next\static\
# Copy public/* to C:\inetpub\kqalumni-frontend\public\
# Copy web.config to C:\inetpub\kqalumni-frontend\

# Example:
Copy-Item ".next\standalone\*" "C:\inetpub\kqalumni-frontend\" -Recurse -Force
New-Item -ItemType Directory -Path "C:\inetpub\kqalumni-frontend\.next"
Copy-Item ".next\static" "C:\inetpub\kqalumni-frontend\.next\static\" -Recurse -Force
Copy-Item "public\*" "C:\inetpub\kqalumni-frontend\public\" -Recurse -Force
Copy-Item "web.config" "C:\inetpub\kqalumni-frontend\"

# 3. Install production dependencies
cd C:\inetpub\kqalumni-frontend
npm ci --production

# 4. Set permissions
icacls "C:\inetpub\kqalumni-frontend" /grant "IIS_IUSRS:(OI)(CI)F" /T

# 5. Create Application Pool
New-WebAppPool -Name "KQAlumniFrontend"
Set-ItemProperty IIS:\AppPools\KQAlumniFrontend -Name managedRuntimeVersion -Value ""

# 6. Create IIS Site
New-Website -Name "KQAlumni-Frontend" `
  -PhysicalPath "C:\inetpub\kqalumni-frontend" `
  -ApplicationPool "KQAlumniFrontend" `
  -Port 80 `
  -HostHeader "kqalumni-dev.kenya-airways.com"

# 7. Start the site
Start-Website -Name "KQAlumni-Frontend"
```

---

## Step 5: Verify Deployment

### 1. Backend Health Check

Open browser: https://kqalumniapi-dev.kenya-airways.com/health

**Expected Response**:
```json
{
  "status": "healthy",
  "timestamp": "2025-10-30T...",
  "environment": "Production",
  "version": "1.0.0"
}
```

### 2. Swagger API Documentation

Open: https://kqalumniapi-dev.kenya-airways.com/swagger

Should display API documentation.

### 3. Hangfire Dashboard

Open: https://kqalumniapi-dev.kenya-airways.com/hangfire

Should display background jobs dashboard.

### 4. Frontend Application

Open: https://kqalumni-dev.kenya-airways.com

Should display homepage and registration form.

### 5. End-to-End Test

1. Fill out registration form
2. Submit registration
3. Verify email is sent
4. Check Hangfire dashboard for job processing
5. Check database for registration record

---

## Troubleshooting

### Backend Issues

#### Application fails to start (500.30 Error)

**Check**:
```powershell
# 1. Check logs
Get-Content C:\inetpub\kqalumni-api\logs\stdout_*.log -Tail 50

# 2. Verify .NET Runtime
dotnet --list-runtimes

# 3. Verify appsettings exists
Test-Path C:\inetpub\kqalumni-api\appsettings.Production.json

# 4. Test SQL connection
sqlcmd -S 10.2.150.23 -U kqalumni_user -P YourPassword
```

#### Database connection fails

**Solutions**:
- Verify SQL Server allows remote connections
- Check firewall rules on database server (port 1433)
- Verify connection string in appsettings.Production.json
- Test connection with SQL Management Studio

#### CORS errors in browser

**Solution**:
```powershell
# Verify frontend URL is in CORS settings
# Restart application pool
Restart-WebAppPool -Name "KQAlumniAPI"
```

### Frontend Issues

#### 502.3 Error (Bad Gateway)

**Check**:
```powershell
# 1. Verify server.js exists
Test-Path C:\inetpub\kqalumni-frontend\server.js

# 2. Check Node.js installed
node --version

# 3. Check iisnode logs
Get-Content C:\inetpub\kqalumni-frontend\iisnode\*.log -Tail 50

# 4. Verify .next/static folder
Test-Path C:\inetpub\kqalumni-frontend\.next\static
```

#### Static files not loading (404 errors)

**Solutions**:
- Verify `.next/static/` folder exists with correct structure
- Check URL Rewrite Module is installed: `Get-WindowsFeature Web-Url-Rewrite`
- Review web.config URL rewrite rules
- Check browser console for specific 404 URLs

#### High memory usage

**Solution**:
```powershell
# Adjust Node.js memory in web.config or start script
node --max_old_space_size=4096 server.js
```

### General Issues

#### Application pool crashes

**Solution**:
```powershell
# Check Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 20

# Increase failure thresholds
Set-ItemProperty IIS:\AppPools\KQAlumniAPI -Name failure.rapidFailProtectionInterval -Value "00:10:00"
Set-ItemProperty IIS:\AppPools\KQAlumniAPI -Name failure.rapidFailProtectionMaxCrashes -Value 10
```

---

## Maintenance

### Update Backend

```powershell
# Stop application pool
Stop-WebAppPool -Name "KQAlumniAPI"

# Backup current version
Copy-Item "C:\inetpub\kqalumni-api" "C:\backups\kqalumni-api-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# Deploy new version (copy new published files)
Copy-Item ".\publish\*" "C:\inetpub\kqalumni-api\" -Recurse -Force

# Start application pool
Start-WebAppPool -Name "KQAlumniAPI"
```

### Update Frontend

```powershell
# Stop site
Stop-Website -Name "KQAlumni-Frontend"

# Backup current version
Copy-Item "C:\inetpub\kqalumni-frontend" "C:\backups\kqalumni-frontend-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# Deploy new version (copy new built files)
# ... copy new files ...

# Reinstall dependencies
cd C:\inetpub\kqalumni-frontend
npm ci --production

# Start site
Start-Website -Name "KQAlumni-Frontend"
```

### Database Backups

```sql
-- Create backup
BACKUP DATABASE KQAlumniDB
TO DISK = 'C:\Backups\KQAlumniDB.bak'
WITH FORMAT, INIT, COMPRESSION;
```

---

## Security Checklist

Before going to production:

- [ ] HTTPS configured with valid SSL certificates
- [ ] Strong database password set
- [ ] Email password secured (consider App Password)
- [ ] Firewall configured (ports 80, 443)
- [ ] Disable Swagger in production
- [ ] Configure Hangfire authentication
- [ ] Regular backups scheduled
- [ ] Monitoring and alerting setup

---

## Useful Commands

```powershell
# Restart IIS
iisreset

# Restart specific application pool
Restart-WebAppPool -Name "KQAlumniAPI"

# View application pool status
Get-WebAppPoolState -Name "KQAlumniAPI"

# View all websites
Get-Website

# Check .NET Runtime
dotnet --list-runtimes

# Check Node.js
node --version

# View recent IIS logs
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```

---

## Support

**Configuration Files**:
- Backend: `KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json`
- Frontend: `kq-alumni-frontend/.env.production.local`

**Log Locations**:
- Backend: `C:\inetpub\kqalumni-api\logs\`
- Frontend: `C:\inetpub\kqalumni-frontend\iisnode\`

**For Issues**:
1. Check application logs
2. Review IIS Event Viewer
3. Verify connectivity (SQL, SMTP)
4. Test endpoints individually

---

**Last Updated**: 2025-10-30
**Version**: 1.0
