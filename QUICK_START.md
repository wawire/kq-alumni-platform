# Quick Start Guide - Local Development Setup

This guide will get you up and running with the KQ Alumni Platform on your local machine.

## Prerequisites

Before you begin, ensure you have installed:

- **.NET 8.0 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js 18+**: https://nodejs.org/
- **Docker Desktop**: https://www.docker.com/products/docker-desktop/ (for SQL Server)
- **Git**: https://git-scm.com/downloads

## Step 1: Clone the Repository

```bash
# Clone the repository
git clone https://github.com/wawire/kq-alumni-platform.git
cd kq-alumni-platform

# Checkout the latest development branch
git checkout claude/fix-email-notifications-deployment-011CUnGTXC3h5bdcYRBLKVRD
```

## Step 2: Start SQL Server (Docker)

The easiest way to run SQL Server locally is using Docker:

```bash
# Start SQL Server 2019 in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name kq-alumni-sql \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Verify it's running
docker ps | grep kq-alumni-sql
```

**Note**: The password `YourStrong@Passw0rd` matches the one in `appsettings.Development.json`. Change both if you want a different password.

### Managing Docker SQL Server

```bash
# Stop the container
docker stop kq-alumni-sql

# Start the container (after first run)
docker start kq-alumni-sql

# Remove the container (if you need to start fresh)
docker stop kq-alumni-sql
docker rm kq-alumni-sql
```

## Step 3: Backend Setup

The configuration files are already created for you:

```bash
# Navigate to the backend API
cd KQAlumni.Backend/src/KQAlumni.API

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update --project ../KQAlumni.Infrastructure

# Start the backend API
dotnet run
```

The backend will start on: **http://localhost:5295**

### Verify Backend is Running

- **API Test**: http://localhost:5295/api/test
- **Health Check**: http://localhost:5295/health
- **Swagger Documentation**: http://localhost:5295/swagger
- **Hangfire Dashboard**: http://localhost:5295/hangfire

## Step 4: Frontend Setup

The frontend configuration is also ready:

```bash
# Open a NEW terminal window and navigate to frontend
cd kq-alumni-frontend

# Install dependencies
npm install

# Start the development server
npm run dev
```

The frontend will start on: **http://localhost:3000**

## Step 5: Test the Application

### Test User Registration

1. Open http://localhost:3000 in your browser
2. Navigate to the registration page
3. Use any of these test staff numbers (mock mode is enabled):
   - `0012345`
   - `00C5050`
   - `00A1234`
   - `00RG002`
   - `00EM004`
   - `00H1234`

### Test Staff Number Validation

The validation now accepts: **00** + **any 5 alphanumeric characters**

**Valid Examples**:
- ✅ `0012345` (digits only)
- ✅ `00C5050` (contract)
- ✅ `00A1234` (intern)
- ✅ `00RG002` (regional)
- ✅ `00EM004` (emergency)
- ✅ `00H1234` (any format)

**Invalid Examples**:
- ❌ `12345` (too short)
- ❌ `0012345678` (too long)
- ❌ `012345` (doesn't start with 00)
- ❌ `00123ab` (lowercase not allowed)

### Test Email Functionality

In development mode, emails are **mocked** (not actually sent). You can:

1. Check the **EmailLogs** table in the database to see all email delivery attempts:
   ```sql
   SELECT * FROM EmailLogs ORDER BY SentAt DESC;
   ```

2. Check the backend console logs for email simulation messages

## Configuration Files Explained

### Backend Configuration

**`appsettings.json`** (Base configuration - checked into git)
- Contains default settings for all environments
- Uses LocalDB connection string by default
- Has production email credentials (should be overridden in production)

**`appsettings.Development.json`** (Local development - NOT in git)
- Created automatically for local development
- Uses Docker SQL Server connection string
- Email mock mode: **enabled**
- ERP mock mode: **enabled**
- Single connection string: **DefaultConnection** (used for both database and Hangfire)

**`appsettings.Production.json`** (Production - NOT in git)
- Created for production deployment
- Uses production database and URLs
- Email mock mode: **disabled**
- ERP mock mode: **disabled**

### Frontend Configuration

**`.env.example`** (Example file - checked into git)
- Template showing all available environment variables

**`.env.local`** (Local development - NOT in git)
- Created automatically for local development
- Points to http://localhost:5295
- Debug mode enabled

**`.env.production`** (Production - checked into git)
- Points to production API URL

## Database Connection Options

The `appsettings.Development.json` uses Docker SQL Server by default:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=KQAlumniDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;Encrypt=true;"
}
```

### Alternative: Windows LocalDB

If you're on Windows and prefer LocalDB, change the connection string to:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(LocalDB)\\MSSQLLocalDB;Database=KQAlumniDB;Integrated Security=true;TrustServerCertificate=true;"
}
```

## Common Issues & Solutions

### Issue: "Connection string 'DefaultConnection' not found"

**Solution**: Make sure you're in the correct directory and `appsettings.Development.json` exists:
```bash
cd KQAlumni.Backend/src/KQAlumni.API
ls -la appsettings.Development.json
```

### Issue: "Database connection failed"

**Solution**: Verify Docker SQL Server is running:
```bash
docker ps | grep kq-alumni-sql
# If not running, start it:
docker start kq-alumni-sql
```

### Issue: "Port 5295 already in use"

**Solution**: Kill the process using port 5295:
```bash
# Linux/Mac
lsof -ti:5295 | xargs kill -9

# Windows
netstat -ano | findstr :5295
taskkill /PID <PID> /F
```

### Issue: "Port 3000 already in use"

**Solution**: Use a different port:
```bash
PORT=3001 npm run dev
```

### Issue: "Frontend can't connect to backend"

**Solution**: Verify backend is running and check the URL in `.env.local`:
```bash
# Should be:
NEXT_PUBLIC_API_URL=http://localhost:5295
```

## Monitoring & Debugging

### Backend Logs

The backend logs are displayed in the terminal where you ran `dotnet run`. Look for:
- ✅ Database connection successful
- ✅ Hangfire jobs scheduled
- ✅ Configuration validation passed

### Email Delivery Tracking

All email attempts are logged to the `EmailLogs` table:

```sql
-- View all email logs
SELECT
    ToEmail,
    Subject,
    EmailType,
    Status,
    ErrorMessage,
    SentAt,
    DurationMs
FROM EmailLogs
ORDER BY SentAt DESC;

-- Count emails by status
SELECT Status, COUNT(*) as Count
FROM EmailLogs
GROUP BY Status;
```

### Health Check Monitoring

Visit http://localhost:5295/health for detailed health status:
- Database connectivity
- SMTP connectivity (if enabled)
- ERP API connectivity (if enabled)

## Next Steps

1. **Read the full documentation**: Check out `README.md` and `DEPLOYMENT.md`
2. **Explore the API**: Visit http://localhost:5295/swagger
3. **Test registrations**: Try different staff number formats
4. **Monitor background jobs**: Visit http://localhost:5295/hangfire
5. **Check email logs**: Query the EmailLogs table

## Need Help?

- Check `README.md` for architecture and features overview
- Check `DEPLOYMENT.md` for production deployment guide
- Check `MONITORING_AND_IMPROVEMENTS.md` for monitoring details
- Review the health check endpoint: http://localhost:5295/health

## Summary of Changes in v2.0.0

✅ Email notifications fixed with proper production URLs
✅ Email delivery tracking with database logging
✅ Environment variable validation on startup
✅ Enhanced health checks for all dependencies
✅ Staff number validation standardized (frontend ↔ backend)
✅ Documentation consolidated and improved
✅ Rate limiting monitoring
✅ Single connection string for database and Hangfire

Happy coding! 🚀
