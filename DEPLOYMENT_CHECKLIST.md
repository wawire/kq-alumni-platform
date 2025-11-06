# KQ Alumni Platform - Deployment Checklist

## CRITICAL Security Tasks

### 1. Rotate Exposed Credentials [URGENT]
- [ ] **Change Office 365 password** for `KQ.Alumni@kenya-airways.com`
  - Current password was exposed in git: `Syntonin&carses@`
  - Contact IT/Email admin to rotate immediately
  - See `SECURITY_NOTICE.md` for details

### 2. Configure Environment Variables [REQUIRED]

**Production Server (Windows/IIS)**:
```powershell
# Email Password (MUST be rotated first!)
[System.Environment]::SetEnvironmentVariable("EMAIL_PASSWORD", "YOUR_NEW_PASSWORD", "Machine")

# JWT Secret Key (Generate with: openssl rand -base64 64)
[System.Environment]::SetEnvironmentVariable("JWT_SECRET_KEY", "YOUR_64_CHAR_SECRET", "Machine")

# SQL Password
[System.Environment]::SetEnvironmentVariable("SQL_PASSWORD", "YOUR_SQL_PASSWORD", "Machine")

# Restart IIS
iisreset
```

**Docker/Linux**:
```bash
export EMAIL_PASSWORD="YOUR_NEW_PASSWORD"
export JWT_SECRET_KEY="YOUR_64_CHAR_SECRET"
export SQL_PASSWORD="YOUR_SQL_PASSWORD"
```

### 3. Update Application Configuration

The application now reads these values from environment variables. Verify in `Program.cs`:
```csharp
// Email password from environment
builder.Configuration["Email:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

// JWT secret from environment
builder.Configuration["JwtSettings:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
```

---

## Pre-Deployment Verification

### Backend Checks
- [ ] All configuration passwords removed from `appsettings.json`
- [ ] All configuration passwords removed from `appsettings.Production.json`
- [ ] Environment variables configured on production server
- [ ] SQL Server connection string updated with correct server
- [ ] JWT secret key is 64+ characters and random
- [ ] Email sending tested with new password
- [ ] ERP Mock Mode disabled in production (`EnableMockMode: false`)
- [ ] Database migrations applied (`dotnet ef database update`)
- [ ] Hangfire dashboard secured (authentication enabled)

### Frontend Checks
- [ ] `.env.production` configured with production API URL
- [ ] CORS origins updated in backend for production domain
- [ ] Build process tested (`npm run build`)
- [ ] No console.log statements in production code
- [ ] Environment variables validated on startup

### Database Checks
- [ ] Connection to SQL Server 10.2.150.23 verified
- [ ] Database `KQAlumniDB` exists
- [ ] User `kqalumni_user` has correct permissions
- [ ] All migrations applied successfully
- [ ] EmailLogs table exists
- [ ] Initial admin user seeded (if needed)

### Network & Infrastructure
- [ ] Backend server is inside KQ network (can reach ERP at 10.2.131.147:7010)
- [ ] Firewall rules allow SMTP to smtp.office365.com:587
- [ ] HTTPS certificate installed for production domain
- [ ] Load balancer/reverse proxy configured (if applicable)
- [ ] Backup strategy in place

---

## Deployment Steps

### Phase 1: Pre-Deployment (1 day before)
1. **Rotate all exposed credentials**
   - Office 365 email password
   - Generate new JWT secret key
   - Update SQL password if needed

2. **Configure production server**
   - Set all environment variables
   - Install .NET 8.0 runtime
   - Install IIS with ASP.NET Core hosting bundle
   - Configure application pool

3. **Prepare database**
   - Run migrations on production database
   - Seed initial admin user
   - Verify database connectivity

### Phase 2: Backend Deployment
```bash
# Build backend
cd KQAlumni.Backend/src/KQAlumni.API
dotnet publish -c Release -o ./publish

# Deploy to IIS
xcopy ./publish "C:\inetpub\wwwroot\kq-alumni-api" /E /Y

# Restart application pool
Restart-WebAppPool -Name "KQAlumniPool"

# Verify
curl https://kqalumniapi-dev.kenya-airways.com/health
curl https://kqalumniapi-dev.kenya-airways.com/swagger
```

### Phase 3: Frontend Deployment
```bash
# Build frontend
cd kq-alumni-frontend
npm install
npm run build

# Deploy to web server (adjust path as needed)
xcopy .next/standalone "C:\inetpub\wwwroot\kq-alumni-web" /E /Y

# Or deploy to Azure/CDN
az webapp deploy --name kq-alumni-web --resource-group kq-alumni-rg --src-path ./build
```

### Phase 4: Post-Deployment Verification
1. **Health checks**
   - [ ] API health endpoint returns 200
   - [ ] Swagger UI loads successfully
   - [ ] Frontend loads without errors

2. **Functionality tests**
   - [ ] Complete registration flow (end-to-end)
   - [ ] Email sending (confirmation, approval, rejection)
   - [ ] Admin login
   - [ ] Admin dashboard loads
   - [ ] Background jobs running (check Hangfire dashboard)
   - [ ] ERP validation working (or mock mode if ERP unavailable)

3. **Monitoring setup**
   - [ ] Application Insights configured (if using Azure)
   - [ ] Log aggregation setup
   - [ ] Alerts configured for errors
   - [ ] Uptime monitoring enabled

---

## Post-Deployment Tasks

### Immediate (Day 1)
- [ ] Monitor logs for errors
- [ ] Test email sending with real registrations
- [ ] Verify background jobs executing
- [ ] Check rate limiting is working
- [ ] Test admin dashboard functionality

### Week 1
- [ ] Monitor ERP integration success rates
- [ ] Review email delivery logs
- [ ] Check for any security issues
- [ ] Gather user feedback

### Ongoing
- [ ] Regular security updates
- [ ] Database backups
- [ ] Log rotation
- [ ] Performance monitoring

---

## Rollback Plan

If deployment fails:

1. **Backend rollback**
   ```bash
   # Restore previous version
   xcopy "C:\inetpub\wwwroot\kq-alumni-api-backup" "C:\inetpub\wwwroot\kq-alumni-api" /E /Y
   Restart-WebAppPool -Name "KQAlumniPool"
   ```

2. **Database rollback**
   ```bash
   # Rollback to specific migration
   dotnet ef database update PreviousMigrationName
   ```

3. **Frontend rollback**
   - Deploy previous build artifact
   - Clear CDN cache if applicable

---

## Troubleshooting

### Email Not Sending
1. Check environment variable is set: `echo $env:EMAIL_PASSWORD`
2. Verify SMTP connectivity: `Test-NetConnection smtp.office365.com -Port 587`
3. Check email logs in database: `SELECT * FROM EmailLogs ORDER BY SentAt DESC`
4. Enable mock mode temporarily to isolate issue

### ERP Validation Failing
1. Verify server is on KQ network
2. Test ERP connectivity: `curl http://10.2.131.147:7010/health`
3. Enable ERP mock mode temporarily
4. Check retry attempts in logs

### Database Connection Issues
1. Verify SQL Server reachable: `Test-NetConnection 10.2.150.23 -Port 1433`
2. Check connection string format
3. Verify user permissions: `EXEC sp_helprotect @username = 'kqalumni_user'`

---

## Support Contacts

- **DevOps Team**: [contact info]
- **Database Admin**: [contact info]
- **Network Team**: [contact info]
- **Security Team**: [contact info]

---

**Last Updated**: 2025-11-06
**Version**: 1.0
**Status**: Ready for deployment after credential rotation
