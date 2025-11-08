# KQ Alumni Platform - Configuration Templates

This document contains all configuration templates you need for the KQ Alumni Platform.

---

## 📋 Frontend Configuration

### `.env.local` (Development - Already Created)

```bash
# Backend API URL - UPDATE THIS WITH YOUR BACKEND PORT!
NEXT_PUBLIC_API_URL=http://localhost:5295

# API Timeout
NEXT_PUBLIC_API_TIMEOUT=30000

# Environment
NEXT_PUBLIC_ENV=development

# Site URL
NEXT_PUBLIC_SITE_URL=http://localhost:3000

# Application
NEXT_PUBLIC_APP_NAME=KQ Alumni Association
NEXT_PUBLIC_APP_VERSION=1.0.0

# Feature Flags
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_DEBUG_MODE=true

# Support Email
NEXT_PUBLIC_SUPPORT_EMAIL=KQ.Alumni@kenya-airways.com

# Logging
NEXT_PUBLIC_LOG_LEVEL=debug
```

---

### `.env.production` (Production)

```bash
# Backend API - Production URL
NEXT_PUBLIC_API_URL=https://api.kqalumni.kenya-airways.com

# API Timeout
NEXT_PUBLIC_API_TIMEOUT=30000

# Environment
NEXT_PUBLIC_ENV=production

# Site URL
NEXT_PUBLIC_SITE_URL=https://kqalumni.kenya-airways.com

# Application
NEXT_PUBLIC_APP_NAME=KQ Alumni Association
NEXT_PUBLIC_APP_VERSION=1.0.0

# Feature Flags
NEXT_PUBLIC_ENABLE_ANALYTICS=true
NEXT_PUBLIC_ENABLE_DEBUG_MODE=false

# Support Email
NEXT_PUBLIC_SUPPORT_EMAIL=KQ.Alumni@kenya-airways.com

# Logging
NEXT_PUBLIC_LOG_LEVEL=error
```

---

## 🔧 Backend Configuration

### `appsettings.Development.json`

Create this file in `KQAlumni.Backend/src/KQAlumni.API/` to override development settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KQAlumniDB_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },

  "ErpApi": {
    "BaseUrl": "http://10.2.131.147:7010",
    "EnableMockMode": true,
    "MockEmployees": [
      {
        "IdNumber": "12345678",
        "StaffNumber": "0012345",
        "FullName": "John Doe",
        "Department": "IT",
        "ExitDate": "2023-12-31"
      },
      {
        "IdNumber": "87654321",
        "StaffNumber": "0087654",
        "FullName": "Jane Smith",
        "Department": "HR",
        "ExitDate": "2024-01-15"
      }
    ]
  },

  "Email": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "From": "KQ.Alumni@kenya-airways.com",
    "DisplayName": "Kenya Airways Alumni Relations"
  }
}
```

---

### User Secrets (Development Credentials)

**DO NOT commit these to git!** Set via command line:

```bash
cd KQAlumni.Backend/src/KQAlumni.API

# Email Credentials
dotnet user-secrets set "Email:Username" "KQ.Alumni@kenya-airways.com"
dotnet user-secrets set "Email:Password" "YOUR_EMAIL_PASSWORD_HERE"

# JWT Secret (generate a strong random string)
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_256_BIT_SECRET_KEY_HERE_MINIMUM_32_CHARS"

# Database Connection (if using SQL Server authentication)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=KQAlumniDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
```

---

### `appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },

  "CorsSettings": {
    "AllowedOrigins": [
      "https://kqalumni.kenya-airways.com"
    ]
  },

  "ErpApi": {
    "BaseUrl": "http://10.2.131.147:7010",
    "EnableMockMode": false,
    "TimeoutSeconds": 10,
    "RetryCount": 3,
    "CircuitBreakerFailureThreshold": 5
  },

  "BackgroundJobSettings": {
    "ApprovalJobCron": "0 */2 * * * *",
    "BatchSize": 10,
    "MaxRetryAttempts": 5,
    "RetryDelayMinutes": 10
  },

  "JwtSettings": {
    "Issuer": "KQAlumni",
    "Audience": "KQAlumniUsers",
    "ExpirationMinutes": 480
  }
}
```

---

## 🔐 Production Environment Variables

Set these in your production environment (Azure App Service, IIS, Docker, etc.):

```bash
# Database
ConnectionStrings__DefaultConnection="Server=PROD_SERVER;Database=KQAlumniDB;..."

# Email
Email__Username="KQ.Alumni@kenya-airways.com"
Email__Password="PRODUCTION_PASSWORD"

# JWT
JwtSettings__SecretKey="PRODUCTION_SECRET_KEY_256_BITS_MINIMUM"

# ERP (if using authentication)
ErpApi__Username="ERP_USERNAME"
ErpApi__Password="ERP_PASSWORD"
```

---

## 📊 Mock Data for Testing

### Mock ERP Employees (for development)

Add to `appsettings.Development.json` under `ErpApi.MockEmployees`:

```json
{
  "MockEmployees": [
    {
      "IdNumber": "12345678",
      "PassportNumber": "A1234567",
      "StaffNumber": "0012345",
      "FullName": "John Doe",
      "Department": "Information Technology",
      "ExitDate": "2023-12-31"
    },
    {
      "IdNumber": "87654321",
      "PassportNumber": "B7654321",
      "StaffNumber": "0087654",
      "FullName": "Jane Smith",
      "Department": "Human Resources",
      "ExitDate": "2024-01-15"
    },
    {
      "IdNumber": "11111111",
      "PassportNumber": "C1111111",
      "StaffNumber": "0011111",
      "FullName": "Bob Johnson",
      "Department": "Finance",
      "ExitDate": "2024-06-30"
    },
    {
      "IdNumber": "22222222",
      "PassportNumber": "D2222222",
      "StaffNumber": "0022222",
      "FullName": "Alice Williams",
      "Department": "Operations",
      "ExitDate": "2023-09-15"
    }
  ]
}
```

---

## 🧪 Testing Configuration

### Test User IDs for Manual Review Testing

Use these IDs to trigger manual review mode:

```
# Valid IDs (will verify successfully):
- 12345678
- 87654321

# Invalid IDs (will fail verification, allowing manual mode):
- 99999999
- INVALID123
```

---

## 🚀 Quick Start Commands

```bash
# 1. Frontend setup
cd kq-alumni-frontend
cp .env.local.example .env.local  # Already done for you!
npm install
npm run dev

# 2. Backend setup
cd KQAlumni.Backend/src/KQAlumni.API

# Set secrets
dotnet user-secrets set "Email:Username" "your-email@kenya-airways.com"
dotnet user-secrets set "Email:Password" "your-password"
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-min-32-chars"

# Run migrations
cd ../..
dotnet ef database update --project src/KQAlumni.Infrastructure --startup-project src/KQAlumni.API

# Run backend
cd src/KQAlumni.API
dotnet run
```

---

## 📍 Important URLs

| Service | Development | Production |
|---------|-------------|------------|
| Frontend | http://localhost:3000 | https://kqalumni.kenya-airways.com |
| Backend API | http://localhost:5295 | https://api.kqalumni.kenya-airways.com |
| Hangfire Dashboard | http://localhost:5295/hangfire | https://api.kqalumni.kenya-airways.com/hangfire |
| Swagger Docs | http://localhost:5295/swagger | N/A (disabled in prod) |

---

## 🔑 Default Admin Credentials

**IMPORTANT:** Change these after first login!

```
Username: admin
Password: Admin@123
```

Change password at: `/admin/settings`

---

## ✅ Configuration Checklist

Before going live:

- [ ] Frontend `.env.local` created with correct API URL
- [ ] Backend user secrets set (Email, JWT)
- [ ] Database migration run successfully
- [ ] Email credentials tested
- [ ] ERP connection tested (or mock mode enabled)
- [ ] Admin password changed from default
- [ ] CORS origins configured for production
- [ ] JWT secret key is strong (>32 chars)
- [ ] Production connection strings secured
- [ ] Hangfire dashboard access secured

---

## 🆘 Troubleshooting

### Frontend can't connect to backend
```bash
# Check NEXT_PUBLIC_API_URL in .env.local
# Verify backend is running: curl http://localhost:5295/health
```

### Email not sending
```bash
# Verify credentials in user secrets
dotnet user-secrets list

# Check SMTP settings in appsettings.json
```

### ERP connection failing
```bash
# Enable mock mode in appsettings.Development.json
"ErpApi": {
  "EnableMockMode": true
}
```

### Database migration errors
```bash
# Reset database (development only!)
dotnet ef database drop --project src/KQAlumni.Infrastructure --startup-project src/KQAlumni.API
dotnet ef database update --project src/KQAlumni.Infrastructure --startup-project src/KQAlumni.API
```
