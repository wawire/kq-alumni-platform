# KQ Alumni Platform

> A modern, enterprise-grade platform for the Kenya Airways Alumni Association, connecting former employees worldwide.

**Version**: 2.1.0 | **Status**: Production Ready | **License**: Proprietary

---

## 📖 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Validation Rules](#validation-rules)
- [Monitoring](#monitoring)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## Overview

The KQ Alumni Platform is a comprehensive web application that manages registration, verification, and engagement for Kenya Airways alumni. Built with modern technologies and enterprise-grade features, it provides:

- **Multi-step registration** with real-time validation
- **ERP integration** for automatic employee verification via Oracle SOA
- **Email verification** workflow with secure token-based authentication
- **Admin dashboard** for managing registrations and approvals
- **Background job processing** with smart scheduling
- **Comprehensive monitoring** and health checks

---

## Features

### Core Functionality
- ✅ **Registration System**: Multi-step wizard with form state persistence
- ✅ **Real-time Validation**: Email and staff number duplicate detection
- ✅ **ERP Integration**: Automatic employee verification via Oracle SOA Suite
- ✅ **Email Verification**: 30-day token expiry with secure links
- ✅ **Background Jobs**: Smart scheduling with Hangfire (business hours, off-hours, weekends)

### Resilience & Reliability (v2.1.0)
- ✅ **ERP Fallback Mode**: Continue registrations when ERP is unavailable with manual review flag
- ✅ **Email Resend Feature**: User self-service page + admin dashboard action for resending verification emails
- ✅ **Password Management**: Admin password change functionality with secure API endpoint
- ✅ **Graceful Degradation**: Manual mode UI appears automatically when ERP verification fails

### Monitoring & Reliability (v2.0.0)
- ✅ **Email Delivery Tracking**: Database logging of all email attempts with status, duration, and errors
- ✅ **Environment Validation**: Startup validation that fails fast on invalid configuration
- ✅ **Enhanced Health Checks**: SQL Server, SMTP, and ERP connectivity monitoring
- ✅ **Rate Limiting Monitor**: Automated reporting every 15 minutes
- ✅ **Standardized Validation**: Frontend and backend validation patterns synchronized

### Security
- 🔒 JWT authentication for admin access
- 🔒 Rate limiting (100 requests/hour in production, 1000 in development)
- 🔒 IP whitelisting support for admin dashboard
- 🔒 CORS protection with configurable origins
- 🔒 SQL injection protection via Entity Framework Core
- 🔒 Disposable email domain blocking

### Admin Features
- 📊 Dashboard with registration statistics
- ✅ Manual review workflow for flagged registrations
- 📧 Email notification management (approval, rejection, verification)
- 📝 Audit logging for all admin actions
- ⚙️ Hangfire dashboard for background job monitoring

---

## Quick Start

### Prerequisites

Before you begin, install:

- **.NET 8.0 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js 18+**: https://nodejs.org/
- **Docker Desktop**: https://www.docker.com/products/docker-desktop/
- **Git**: https://git-scm.com/

### 1. Clone Repository

```bash
git clone https://github.com/wawire/kq-alumni-platform.git
cd kq-alumni-platform
```

### 2. Start SQL Server (Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name kq-alumni-sql \
  -d mcr.microsoft.com/mssql/server:2019-latest
```

**Manage Container**:
```bash
docker stop kq-alumni-sql    # Stop
docker start kq-alumni-sql   # Start
docker rm kq-alumni-sql      # Remove (start fresh)
```

### 3. Backend Setup

The repository includes configuration templates. Create your local config:

```bash
cd KQAlumni.Backend/src/KQAlumni.API

# appsettings.Development.json already exists locally (not in git)
# It uses: Server=localhost,1433;Database=KQAlumniDB;User Id=sa;Password=YourStrong@Passw0rd

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update --project ../KQAlumni.Infrastructure

# Start backend
dotnet run
```

**Backend URLs**:
- API: http://localhost:5295
- Swagger: http://localhost:5295/swagger
- Hangfire: http://localhost:5295/hangfire
- Health: http://localhost:5295/health

### 4. Frontend Setup

```bash
cd kq-alumni-frontend

# .env.local already exists locally (not in git)
# It uses: NEXT_PUBLIC_API_URL=http://localhost:5295

# Install dependencies
npm install

# Start development server
npm run dev
```

**Frontend URL**: http://localhost:3000

### 5. Test Registration

Mock mode is enabled in development with comprehensive test data. You can test registration using any of the following:

#### Test by Staff Number:
- `0012345` - John Kamau (Flight Operations)
- `00C5050` - Mary Wanjiku (Customer Service)
- `00A1234` - Peter Omondi (IT Department)
- `00H7890` - Sarah Akinyi (Cabin Crew)
- `00C5583` - James Kipchoge (Engineering)
- `00B9876` - Grace Nyambura (Human Resources)

#### Test by ID Number:
- `12345678` - Maps to Staff #0012345 (John Kamau)
- `87654321` - Maps to Staff #00C5050 (Mary Wanjiku)
- `11111111` - Maps to Staff #00A1234 (Peter Omondi)
- `22222222` - Maps to Staff #00C5583 (James Kipchoge)
- `33333333` - Maps to Staff #00B9876 (Grace Nyambura)

#### Test by Passport Number:
- `A1234567` - Maps to Staff #00A1234 (Peter Omondi)
- `B7654321` - Maps to Staff #00H7890 (Sarah Akinyi)
- `C9876543` - Maps to Staff #00B9876 (Grace Nyambura)

**Note**: All mock employees have pre-configured names, emails, and departments in `appsettings.Development.json`

---

## Architecture

### Technology Stack

**Backend**:
- .NET 8.0 ASP.NET Core Web API
- Entity Framework Core 8.0
- Hangfire for background jobs
- FluentValidation for input validation
- SQL Server 2019+

**Frontend**:
- Next.js 14 with App Router
- React 18 with TypeScript
- TailwindCSS for styling
- React Hook Form + Zod for validation
- Axios for API communication

**Infrastructure**:
- IIS 10+ (production hosting)
- SQL Server 2019+ (database)
- Oracle SOA Suite (ERP integration)
- Office 365 SMTP (email delivery)

### Project Structure

```
kq-alumni-platform/
├── KQAlumni.Backend/
│   ├── src/
│   │   ├── KQAlumni.API/              # API controllers, middleware, health checks
│   │   ├── KQAlumni.Core/             # Domain models, validators, interfaces
│   │   └── KQAlumni.Infrastructure/   # Services, data access, background jobs
│   └── tests/                         # Unit and integration tests
├── kq-alumni-frontend/
│   └── src/
│       ├── app/                       # Next.js App Router pages
│       ├── components/                # React components
│       ├── lib/                       # API client, utilities
│       └── constants/                 # Configuration, navigation
├── DEPLOYMENT.md                      # Production deployment guide
└── README.md                          # This file
```

### Key Design Patterns

- **Clean Architecture**: Separation of concerns (API, Core, Infrastructure)
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic encapsulation
- **Middleware Pipeline**: Request processing (error handling, rate limiting, CORS)
- **Background Jobs**: Async processing with Hangfire
- **Health Checks**: Dependency monitoring (SQL, SMTP, ERP)

---

## Validation Rules

### Staff Number Format

**Pattern**: `^00[0-9A-Z]{5}$`

**Format**: 7 characters total
- First 2 characters: `00` (required prefix)
- Next 5 characters: Any combination of digits (0-9) or uppercase letters (A-Z)

**Valid Examples**:
```
✅ 0012345   - Permanent staff (digits only)
✅ 00C5050   - Contract staff
✅ 00A1234   - Intern/apprentice
✅ 00RG002   - Regional staff
✅ 00EM004   - Engineering/Maintenance
✅ 00H1234   - HR department
```

**Invalid Examples**:
```
❌ 12345     - Missing '00' prefix
❌ 0012345678 - Too long (must be exactly 7 characters)
❌ 012345    - Missing one '0' (must start with '00')
❌ 00123ab   - Lowercase letters not allowed
❌ 00 12345  - Spaces not allowed
```

**Validation Notes**:
- Frontend and backend use identical validation patterns
- Auto-converts to uppercase
- Trims whitespace
- Backend validates with FluentValidation
- Frontend validates with Zod schemas

### Email Format

**Pattern**: Standard RFC 5322
- Maximum 255 characters
- Must contain @ symbol
- Valid domain name required
- Disposable email domains blocked
- Auto-converted to lowercase

### Name Validation

**Pattern**: `^[a-zA-Z\s'-]+$`
- Only letters, spaces, hyphens, and apostrophes
- 2-100 characters per name field
- Trims whitespace

### LinkedIn URL (Optional)

**Pattern**: `^https?:\/\/(www\.)?linkedin\.com\/in\/[a-zA-Z0-9_-]+\/?$`
- Must be valid LinkedIn profile URL
- Optional field

---

## Monitoring

### Health Endpoints

The application exposes comprehensive health checks:

**`GET /health`** - Full system health
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-04T10:30:00Z",
  "environment": "Production",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database is healthy (responded in 45ms)",
      "duration": 45.2,
      "tags": ["database", "critical", "ready"]
    },
    {
      "name": "smtp",
      "status": "Healthy",
      "description": "SMTP server connection successful",
      "duration": 120.5,
      "tags": ["email", "external"]
    },
    {
      "name": "erp_api",
      "status": "Healthy",
      "description": "ERP API is responding",
      "duration": 200.1,
      "tags": ["erp", "external"]
    }
  ],
  "totalDuration": 365.8
}
```

**`GET /health/ready`** - Kubernetes readiness probe (database check only)

**`GET /health/live`** - Kubernetes liveness probe (always returns 200 OK)

### Email Delivery Tracking

All email delivery attempts are logged to the `EmailLogs` table with:
- **ToEmail**: Recipient address
- **Subject**: Email subject line
- **EmailType**: Confirmation, Approval, or Rejection
- **Status**: Sent, Failed, or MockMode
- **ErrorMessage**: Error details (if failed)
- **SentAt**: Timestamp
- **DurationMs**: Delivery time in milliseconds
- **RetryCount**: Number of retry attempts

**Query Recent Emails**:
```sql
SELECT
    ToEmail,
    EmailType,
    Status,
    ErrorMessage,
    SentAt,
    DurationMs
FROM EmailLogs
ORDER BY SentAt DESC
OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY;
```

**Email Delivery Statistics**:
```sql
SELECT
    EmailType,
    Status,
    COUNT(*) as Total,
    AVG(DurationMs) as AvgDurationMs,
    MAX(DurationMs) as MaxDurationMs
FROM EmailLogs
WHERE SentAt >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY EmailType, Status
ORDER BY EmailType, Status;
```

**Failed Emails in Last 24 Hours**:
```sql
SELECT
    ToEmail,
    Subject,
    ErrorMessage,
    SentAt,
    RetryCount
FROM EmailLogs
WHERE Status = 'Failed'
  AND SentAt >= DATEADD(HOUR, -24, GETUTCDATE())
ORDER BY SentAt DESC;
```

### Rate Limiting Monitor

The `RateLimitMonitor` service runs in the background and logs statistics every 15 minutes:

```
╔════════════════════════════════════════╗
║  RATE LIMITING STATISTICS REPORT      ║
║  Total Requests: 5,423                ║
║  Rate Limited:   12                   ║
║  Hit Rate:       0.22%                ║
║  Period:         15 minutes           ║
╚════════════════════════════════════════╝
```

Check application logs for these reports to monitor rate limiting effectiveness.

### Configuration Validation

On startup, the application validates all required configuration:
- ✅ Database connection string
- ✅ JWT settings (secret key length, issuer, audience)
- ✅ Email settings (SMTP server, credentials)
- ✅ ERP API settings (base URL, endpoint)
- ✅ App settings (BaseUrl not localhost in production)
- ✅ CORS settings (allowed origins)

**If validation fails**, the application logs detailed errors and **exits immediately** (fail-fast).

**Successful validation** displays:
```
╔══════════════════════════════════════════════════════════╗
║          CONFIGURATION VALIDATION SUCCESSFUL             ║
╠══════════════════════════════════════════════════════════╣
║  Environment:        Production                          ║
║  Database:           ✓ Connected                         ║
║  Email SMTP:         smtp.office365.com:587              ║
║  ERP API:            http://10.2.131.147:7010           ║
║  Base URL:           https://kqalumni-dev.kenya-airways.com ║
║  JWT Configured:     ✓ Valid                            ║
║  CORS Origins:       1 configured                       ║
╚══════════════════════════════════════════════════════════╝
```

---

## Deployment

For **production deployment**, see **[DEPLOYMENT.md](DEPLOYMENT.md)**.

Quick deployment checklist:
1. ✅ Install prerequisites (.NET 8, Node.js 18+, IIS, SQL Server)
2. ✅ Create `appsettings.Production.json` with production settings
3. ✅ Update database connection string
4. ✅ Configure email SMTP credentials
5. ✅ Set `AppSettings.BaseUrl` to production URL
6. ✅ Apply database migrations
7. ✅ Build backend: `dotnet publish -c Release`
8. ✅ Build frontend: `npm run build`
9. ✅ Deploy to IIS (separate app pools for backend and frontend)
10. ✅ Verify health check: `https://your-api-domain.com/health`

**IMPORTANT**: The application performs configuration validation on startup and will **fail fast** if settings are invalid or contain placeholder values.

---

## Troubleshooting

### Configuration Issues

**Problem**: Configuration validation fails on startup

**Solution**:
1. Check `appsettings.Production.json` exists and has correct values
2. Ensure `AppSettings.BaseUrl` is NOT `localhost` in production
3. Verify JWT `SecretKey` is at least 32 characters
4. Confirm database connection string is valid
5. Test SMTP credentials: `telnet smtp.office365.com 587`

**Problem**: "Connection string 'DefaultConnection' not found"

**Solution**:
- Verify `appsettings.Development.json` or `appsettings.Production.json` exists
- Check `ConnectionStrings.DefaultConnection` is defined
- Ensure file is in `KQAlumni.Backend/src/KQAlumni.API/` directory

### Database Issues

**Problem**: Database connection fails

**Solution**:
```bash
# Test SQL Server connection
docker ps | grep kq-alumni-sql   # Check container is running
docker start kq-alumni-sql       # Start if stopped

# Test connection with sqlcmd
sqlcmd -S localhost,1433 -U sa -P 'YourStrong@Passw0rd'
```

**Problem**: Migrations fail

**Solution**:
```bash
# Ensure you're in the API project directory
cd KQAlumni.Backend/src/KQAlumni.API

# Run migrations with explicit connection
dotnet ef database update --project ../KQAlumni.Infrastructure --verbose
```

### Email Issues

**Problem**: Emails not being sent

**Solution**:
1. Check `EmailLogs` table for error messages:
   ```sql
   SELECT TOP 10 * FROM EmailLogs
   WHERE Status = 'Failed'
   ORDER BY SentAt DESC;
   ```
2. Verify SMTP settings in appsettings
3. Ensure `EnableEmailSending = true`
4. Ensure `UseMockEmailService = false` (in production)
5. Test SMTP connectivity: `telnet smtp.office365.com 587`
6. Check firewall rules for port 587 (SMTP TLS)

**Problem**: Email links point to localhost

**Solution**:
- Update `AppSettings.BaseUrl` in `appsettings.Production.json`
- Restart the application
- Configuration validator will catch this on next startup

### Frontend Issues

**Problem**: Frontend can't connect to backend

**Solution**:
1. Check `.env.local` (dev) or `.env.production.local` (prod) has correct API URL
2. Verify backend is running: `curl http://localhost:5295/health`
3. Check CORS settings in backend allow frontend origin
4. Inspect browser console for CORS errors

**Problem**: Staff number validation fails

**Solution**:
- Ensure staff number is UPPERCASE
- Verify format: `00` + 5 alphanumeric characters (e.g., `0012345`, `00C5050`)
- Check backend logs for validation errors
- Frontend and backend validation now synchronized (v2.0.0)

### Performance Issues

**Problem**: Application running slowly

**Solution**:
1. Check health endpoint for slow dependencies: `/health`
2. Query EmailLogs for slow email deliveries:
   ```sql
   SELECT * FROM EmailLogs
   WHERE DurationMs > 5000
   ORDER BY SentAt DESC;
   ```
3. Check Hangfire dashboard for stuck jobs: `/hangfire`
4. Monitor SQL Server performance
5. Check ERP API response times

---

## Mock Data Configuration

### Adding Test Employees

Mock employee data is stored in a separate file for easier management:

**Location**: `KQAlumni.Backend/src/KQAlumni.API/MockData/mock-employees.json`

```json
{
  "ErpApi": {
    "MockEmployees": [
      {
        "StaffNumber": "0012345",
        "IdNumber": "12345678",
        "PassportNumber": "A1234567",
        "FullName": "John Kamau Mwangi",
        "Email": "john.kamau@alumni.kenya-airways.com",
        "Department": "Flight Operations",
        "ExitDate": "2024-01-15"
      }
    ]
  }
}
```

**Note**: This file is automatically loaded in Development environment. In `appsettings.Development.json`, you only need:
```json
{
  "ErpApi": {
    "EnableMockMode": true
  }
}
```

### Mock Employee Fields:

- **StaffNumber** (required): 7-character staff number (e.g., "0012345")
- **IdNumber** (optional): National ID number for ID-based lookup
- **PassportNumber** (optional): Passport number for passport-based lookup
- **FullName** (required): Full name of the employee
- **Email** (optional): Email address
- **Department** (required): Department name
- **ExitDate** (optional): Exit date in ISO format (defaults to 6 months ago)

### Testing Registration Flows:

1. **Test by Staff Number**: Use any `StaffNumber` from `MockEmployees`
2. **Test by ID Number**: Use any `IdNumber` from `MockEmployees`
3. **Test by Passport**: Use any `PassportNumber` from `MockEmployees`
4. **Test not found**: Use values not in `MockEmployees` to test error handling

**Production**: Set `EnableMockMode: false` and configure real ERP API endpoints.

---

## Configuration Management

### .NET Configuration Standard

This project follows **.NET best practices** for configuration and secrets management:

**Configuration Hierarchy** (later sources override earlier ones):
1. `appsettings.json` - Base configuration (committed to git)
2. `appsettings.{Environment}.json` - Environment-specific overrides (committed to git)
3. **User Secrets** - Development secrets (NEVER committed)
4. **Environment Variables** - Production secrets (set on server)

### Secrets Management

**IMPORTANT**: Never commit sensitive data (passwords, API keys, connection strings with credentials) to git.

#### Development (Local Machine)

Use **.NET User Secrets** for sensitive configuration:

```bash
cd KQAlumni.Backend/src/KQAlumni.API

# Set email password
dotnet user-secrets set "Email:Password" "your-dev-email-password"

# Set SQL password (if using SQL auth instead of Windows auth)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=KQAlumniDB;User Id=sa;Password=YourPassword"

# Set JWT secret
dotnet user-secrets set "JwtSettings:SecretKey" "your-strong-secret-key-at-least-32-characters"

# Set ERP API key
dotnet user-secrets set "ErpApi:ApiKey" "your-erp-api-key"

# List all secrets
dotnet user-secrets list
```

User secrets are stored in:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **Linux/macOS**: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

#### Production (Server)

Use **Environment Variables** to override configuration:

```bash
# Windows (PowerShell)
$env:Email__Password = "production-email-password"
$env:ConnectionStrings__DefaultConnection = "Server=prodserver;Database=KQAlumniDB;User Id=produser;Password=prodpass"
$env:JwtSettings__SecretKey = "production-jwt-secret-64-chars-minimum"

# Linux (bash)
export Email__Password="production-email-password"
export ConnectionStrings__DefaultConnection="Server=prodserver;Database=KQAlumniDB;User Id=produser;Password=prodpass"
export JwtSettings__SecretKey="production-jwt-secret-64-chars-minimum"
```

**IIS Configuration** (recommended for production):
1. Open IIS Manager
2. Select Application Pool → Advanced Settings
3. Add environment variables in "Environment Variables" section
4. Restart the application pool

### Configuration Validation

The application performs **comprehensive validation on startup** to ensure all settings are correct before accepting requests.

#### Automatic Validation Features:

1. **DataAnnotations Validation**
   - All configuration classes use `[Required]`, `[Range]`, `[EmailAddress]`, etc.
   - Validates field types, lengths, and formats
   - Fails fast if configuration is invalid

2. **Custom Business Rules**
   - ERP Mock Mode must be disabled in Production
   - Email Mock Service must be disabled in Production
   - JWT SecretKey must be at least 32 characters (64 recommended for production)
   - Connection strings cannot contain placeholder values
   - BaseUrl cannot be localhost in Production

3. **Health Check Integration**
   - Configuration health check runs automatically
   - Available at `/health` endpoint
   - Shows detailed validation errors and warnings

#### Validation Examples:

**Invalid Configuration** (app will not start):
```json
{
  "JwtSettings": {
    "SecretKey": "short",  // ❌ Too short (minimum 32 characters)
    "Issuer": "",          // ❌ Required field missing
    "ExpirationMinutes": 100000  // ❌ Out of range (max 43200)
  }
}
```

**Valid Configuration**:
```json
{
  "JwtSettings": {
    "SecretKey": "your-strong-secret-key-at-least-32-characters-long",  // ✅ Valid
    "Issuer": "KQAlumniAPI",  // ✅ Present
    "Audience": "KQAlumniAdmin",  // ✅ Present
    "ExpirationMinutes": 480  // ✅ Within range
  }
}
```

#### Checking Configuration Health:

```bash
# Check all configuration
curl http://localhost:5295/health

# Response example:
{
  "status": "Healthy",
  "checks": [
    {
      "name": "configuration",
      "status": "Healthy",
      "description": "All configuration settings are valid"
    }
  ]
}
```

If configuration is invalid, the health check will return `Unhealthy` with detailed error messages.

### Configuration Files

#### Backend Configuration

**`appsettings.json`** (Base - committed to git)
- Default settings for all environments
- Contains non-sensitive shared configuration
- No passwords or API keys

**`appsettings.Development.json`** (committed to git)
- Development-specific settings (mock modes, debug logging)
- Uses Windows Authentication for SQL (no password needed)
- Email/ERP passwords loaded from User Secrets

**`appsettings.Production.template.json`** (committed to git)
- Template showing production structure
- Placeholders like `USE_ENVIRONMENT_VARIABLE`
- Copy to `appsettings.Production.json` and customize

**`appsettings.Production.json`** (NOT committed - .gitignore)
- Production-specific settings
- Secrets loaded from environment variables
- Created manually on production server

#### Frontend Configuration

**`.env.example`** (Template - in git)
- Template showing all available variables
- Copy to `.env.local` for development

**`.env.local`** (Local dev - NOT in git)
- Points to `http://localhost:5295`
- Debug mode enabled

**`.env.production`** (Production - in git)
- Production API URL
- Non-sensitive production settings

**`.env.production.local`** (Production override - NOT in git)
- Override production settings if needed
- Takes precedence over `.env.production`

---

## Support

**For Production Deployment**: See [DEPLOYMENT.md](DEPLOYMENT.md)

**For Issues**:
- Email: KQ.Alumni@kenya-airways.com
- Check health endpoint: `/health`
- Review application logs
- Query EmailLogs table for email issues

---

## Version History

**v2.1.0** (2025-11-08)
- ✅ **ERP Fallback Mode**: Allow registrations when ERP is unavailable with manual review workflow
- ✅ **Email Verification Resend**: Dual methods for resending verification emails (user self-service + admin dashboard)
- ✅ **Password Change API**: Connected frontend settings page to backend password change endpoint
- ✅ **Method Overloading**: ResendVerificationEmailAsync supports both Guid (admin) and string email (user) lookups
- ✅ **Code Cleanup**: Removed debug console.log statements from production code
- ✅ **Enhanced UX**: Manual mode UI with clear warnings and "Continue with Manual Review" option

**v2.0.0** (2025-11-04)
- ✅ Email delivery tracking with database logging
- ✅ Environment variable validation on startup (fail-fast)
- ✅ Enhanced health checks (SQL Server, SMTP, ERP)
- ✅ Rate limiting monitoring service with automated reporting
- ✅ Standardized frontend/backend staff number validation
- ✅ Documentation consolidated into professional structure
- ✅ Single connection string for database and Hangfire

**v1.0.0** (2025-10-31)
- 🚀 Initial production release
- ✅ Multi-step registration wizard
- ✅ ERP integration for staff validation
- ✅ Email verification workflow
- ✅ Admin dashboard with Hangfire background jobs
- ✅ Rate limiting and security features

---

**Built with ❤️ for the Kenya Airways Alumni Community**
