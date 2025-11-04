# KQ Alumni Platform - Environment Configuration Guide

**CRITICAL**: This guide explains how to properly configure environment variables for deployment. Missing or incorrect environment configuration is the #1 cause of deployment issues, especially for email notifications.

---

## 🚨 Common Issues Caused by Missing Environment Config

### Issue 1: Email Verification Links Not Working
**Symptom**: Users receive emails with `http://localhost:3000/verify/{token}` links that don't work
**Root Cause**: `AppSettings.BaseUrl` in backend is set to localhost
**Fix**: Create `appsettings.Production.json` with production BaseUrl (see below)

### Issue 2: Frontend Can't Connect to Backend API
**Symptom**: Registration form fails to submit, API calls return 404 or CORS errors
**Root Cause**: `.env.production.local` missing or has `http://localhost:5295`
**Fix**: Create `.env.production.local` with production API URL (see below)

### Issue 3: Environment Variables Not Loaded
**Symptom**: Application uses default values instead of production settings
**Root Cause**: Files not created or not named correctly (`.local` suffix matters!)
**Fix**: Follow exact naming conventions below

---

## 📁 Environment Files Overview

### Backend (.NET API)

| File | Purpose | Git Tracked? | When Used |
|------|---------|-------------|-----------|
| `appsettings.json` | Base config for local development | ✅ Yes | Always loaded first |
| `appsettings.Development.json` | Dev overrides | ❌ No | When `ASPNETCORE_ENVIRONMENT=Development` |
| `appsettings.Production.json` | **Production settings (REQUIRED)** | ❌ No | When `ASPNETCORE_ENVIRONMENT=Production` |
| `appsettings.*.template.json` | Templates for creating env files | ✅ Yes | Never used directly |

### Frontend (Next.js)

| File | Purpose | Git Tracked? | When Used |
|------|---------|-------------|-----------|
| `.env.example` | Documentation of all env vars | ✅ Yes | Never used directly |
| `.env.local.example` | Template for local dev | ✅ Yes | Never used directly |
| `.env.production` | Production defaults | ✅ Yes | Production builds |
| `.env.production.local` | **Production overrides (REQUIRED)** | ❌ No | Production builds (highest priority) |

**Priority Order**: `.env.production.local` > `.env.production` > `.env`

---

## 🔧 Step 1: Backend Configuration

### Create `appsettings.Production.json`

```bash
cd KQAlumni.Backend/src/KQAlumni.API
```

**File**: `appsettings.Production.json` (already created in this fix)

**Critical Settings to Update**:

```json
{
  "AppSettings": {
    "BaseUrl": "https://kqalumni-dev.kenya-airways.com"  // ⚠️ CRITICAL for email links!
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=10.2.150.23;Database=KQAlumniDB;User Id=kqalumni_user;Password=YOUR_ACTUAL_PASSWORD;TrustServerCertificate=true;Encrypt=true;"
  },

  "Email": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "KQ.Alumni@kenya-airways.com",
    "Password": "Syntonin&carses@",
    "EnableEmailSending": true,    // ✅ Must be true for production
    "UseMockEmailService": false   // ✅ Must be false for production
  },

  "ErpApi": {
    "EnableMockMode": false  // ⚠️ Set to false to use real ERP validation
  },

  "JwtSettings": {
    "SecretKey": "CHANGE-THIS-TO-STRONG-PRODUCTION-SECRET-KEY-MINIMUM-64-CHARACTERS-LONG-FOR-SECURITY"
  }
}
```

**Important**:
- Update `ConnectionStrings.DefaultConnection` with actual SQL Server password
- Update `JwtSettings.SecretKey` with a strong random key (64+ characters)
- Verify `AppSettings.BaseUrl` matches your frontend URL
- Set `ErpApi.EnableMockMode` to `false` for real ERP validation

---

## 🌐 Step 2: Frontend Configuration

### Create `.env.production.local`

```bash
cd kq-alumni-frontend
```

**File**: `.env.production.local` (already created in this fix)

```bash
# PRODUCTION ENVIRONMENT - DEV SERVER
NEXT_PUBLIC_API_URL=https://kqalumniapi-dev.kenya-airways.com
NEXT_PUBLIC_API_TIMEOUT=30000
NEXT_PUBLIC_ENV=production
NEXT_PUBLIC_APP_NAME=KQ Alumni Association
NEXT_PUBLIC_APP_VERSION=1.0.0
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_DEBUG_MODE=false
NEXT_PUBLIC_SUPPORT_EMAIL=KQ.Alumni@kenya-airways.com
NEXT_PUBLIC_LOG_LEVEL=info
```

**Important**:
- `NEXT_PUBLIC_API_URL` must match your backend API URL
- For UAT: `https://kqalumniapi-uat.kenya-airways.com`
- For PROD: `https://kqalumniapi.kenya-airways.com`

---

## ✅ Step 3: Verify Configuration

### Backend Verification

```powershell
# 1. Check file exists
Test-Path "KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json"

# 2. Verify BaseUrl is correct
Get-Content "KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json" | Select-String "BaseUrl"
# Expected: "BaseUrl": "https://kqalumni-dev.kenya-airways.com"

# 3. Start backend and check health endpoint
dotnet run --environment Production
# Visit: https://kqalumniapi-dev.kenya-airways.com/health
```

### Frontend Verification

```bash
# 1. Check file exists
ls -la .env.production.local

# 2. Verify API URL is correct
cat .env.production.local | grep NEXT_PUBLIC_API_URL
# Expected: NEXT_PUBLIC_API_URL=https://kqalumniapi-dev.kenya-airways.com

# 3. Build and verify environment variables are loaded
npm run build
# Check build output for API URL
```

### Test Email Notifications

1. Register a test account through the frontend
2. Check email for confirmation (should arrive immediately)
3. Wait for background job to process (check Hangfire dashboard)
4. Check email for approval with verification link
5. **CRITICAL**: Verify the verification link uses `https://kqalumni-dev.kenya-airways.com/verify/{token}`
   - NOT `http://localhost:3000/verify/{token}`

---

## 🔒 Security Best Practices

### DO ✅
- Keep `.env.production.local` and `appsettings.Production.json` out of git
- Use strong passwords for SQL Server and JWT secret
- Rotate SMTP password regularly
- Use HTTPS for all production URLs
- Enable CORS only for trusted domains
- Disable Swagger and Hangfire dashboard in production (or add authentication)

### DON'T ❌
- Don't commit production credentials to git
- Don't use localhost URLs in production config
- Don't use the same JWT secret for dev and prod
- Don't share SMTP credentials in code
- Don't enable debug mode in production

---

## 🌍 Multi-Environment Setup

### DEV Environment

**Backend**:
```json
{
  "AppSettings": { "BaseUrl": "https://kqalumni-dev.kenya-airways.com" },
  "CorsSettings": { "AllowedOrigins": ["https://kqalumni-dev.kenya-airways.com"] }
}
```

**Frontend**:
```
NEXT_PUBLIC_API_URL=https://kqalumniapi-dev.kenya-airways.com
```

### UAT Environment

**Backend**:
```json
{
  "AppSettings": { "BaseUrl": "https://kqalumni-uat.kenya-airways.com" },
  "CorsSettings": { "AllowedOrigins": ["https://kqalumni-uat.kenya-airways.com"] }
}
```

**Frontend**:
```
NEXT_PUBLIC_API_URL=https://kqalumniapi-uat.kenya-airways.com
```

### PROD Environment

**Backend**:
```json
{
  "AppSettings": { "BaseUrl": "https://kqalumni.kenya-airways.com" },
  "CorsSettings": { "AllowedOrigins": ["https://kqalumni.kenya-airways.com"] }
}
```

**Frontend**:
```
NEXT_PUBLIC_API_URL=https://kqalumniapi.kenya-airways.com
```

---

## 🐛 Troubleshooting

### Email Links Point to Localhost

**Symptom**: Email verification links are `http://localhost:3000/verify/{token}`

**Diagnosis**:
```powershell
# Check if Production config exists
Test-Path "KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json"

# Check BaseUrl value
Get-Content "KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json" | Select-String "BaseUrl"

# Check which environment is running
# In IIS: Application Pool > Advanced Settings > Environment Variables
# Should see: ASPNETCORE_ENVIRONMENT=Production
```

**Fix**:
1. Create `appsettings.Production.json` (see Step 1)
2. Set `AppSettings.BaseUrl` to production frontend URL
3. Set IIS environment variable: `ASPNETCORE_ENVIRONMENT=Production`
4. Restart IIS application pool

### Frontend Shows "API Connection Failed"

**Symptom**: Console errors like `net::ERR_CONNECTION_REFUSED` or CORS errors

**Diagnosis**:
```bash
# Check environment file
cat .env.production.local

# Check build output
npm run build
# Look for API URL in build output

# Check browser console Network tab
# Should see requests to: https://kqalumniapi-dev.kenya-airways.com
# NOT: http://localhost:5295
```

**Fix**:
1. Create `.env.production.local` (see Step 2)
2. Rebuild: `npm run build`
3. Verify API URL in browser Network tab
4. Check backend CORS settings include frontend URL

### Environment Variables Not Loading

**Symptom**: Application uses default values

**Common Causes**:
- File not named exactly (`.local` suffix required)
- File in wrong directory
- Environment not set correctly (`ASPNETCORE_ENVIRONMENT` for backend)
- Need to rebuild frontend after changing env files

**Fix**:
```bash
# Backend: Verify file location and name
ls -la KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json

# Frontend: Verify file location and name
ls -la kq-alumni-frontend/.env.production.local

# Rebuild to pick up changes
npm run build
```

---

## 📋 Pre-Deployment Checklist

Before deploying to production:

- [ ] `appsettings.Production.json` created with correct BaseUrl
- [ ] `.env.production.local` created with correct API URL
- [ ] SQL Server password updated in appsettings
- [ ] JWT SecretKey changed to strong random value
- [ ] SMTP credentials verified and working
- [ ] ERP `EnableMockMode` set to `false` for production
- [ ] CORS settings include only production domains
- [ ] Test registration and verify email links are correct
- [ ] Test email verification flow end-to-end
- [ ] Check Hangfire dashboard shows jobs processing
- [ ] Verify health endpoint returns correct environment

---

## 📞 Support

If you encounter issues:

1. Check this guide's troubleshooting section
2. Verify all files exist and have correct values
3. Check application logs:
   - Backend: `C:\inetpub\kqalumni-api\logs\`
   - Frontend: Browser console
4. Test endpoints individually:
   - Backend health: `https://kqalumniapi-dev.kenya-airways.com/health`
   - Frontend: `https://kqalumni-dev.kenya-airways.com`

---

**Last Updated**: 2025-11-04
**Related Docs**:
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) - IIS deployment instructions
- [README.md](README.md) - General project documentation
