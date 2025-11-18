# Deployment Guide

Production deployment instructions for KQ Alumni Platform.

---

## Prerequisites

- Windows Server or Linux server
- SQL Server 2019+
- IIS or Nginx (for hosting)
- .NET 8.0 Runtime
- Node.js 18+ (for frontend build)
- Valid SSL certificate
- SMTP access (Office 365)
- ERP API access

---

## Backend Deployment

### 1. Build Backend
```bash
cd KQAlumni.Backend/src/KQAlumni.API
dotnet publish -c Release -o ./publish
```

### 2. Configure Production Settings

Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=KQAlumni;User Id=sa;Password=***;TrustServerCertificate=true"
  },
  "Email": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "KQ.Alumni@kenya-airways.com",
    "Password": "***",
    "EnableEmailSending": true
  },
  "ErpSettings": {
    "Endpoint": "http://10.2.131.147:7010/soa-infra/...",
    "Username": "***",
    "Password": "***",
    "EnableMockMode": false
  },
  "Jwt": {
    "SecretKey": "*** generate-strong-key ***",
    "Issuer": "https://alumni.kenya-airways.com",
    "Audience": "https://alumni.kenya-airways.com"
  }
}
```

### 3. Database Migration
```bash
dotnet ef database update --project ../KQAlumni.Infrastructure
```

### 4. Deploy to IIS

1. Create IIS site pointing to `publish` folder
2. Set Application Pool to `.NET CLR Version: No Managed Code`
3. Configure bindings (HTTPS on port 443)
4. Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`

### 5. Verify Backend
```
https://alumni-api.kenya-airways.com/health
```
Should return: `Healthy`

---

## Frontend Deployment

### 1. Configure Environment
Create `.env.production`:
```
NEXT_PUBLIC_API_URL=https://alumni-api.kenya-airways.com
```

### 2. Build Frontend
```bash
cd kq-alumni-frontend
npm install
npm run build
```

### 3. Deploy Static Files

**Option A: IIS**
- Copy contents of `.next` and `public` folders to IIS site
- Install IIS URL Rewrite module
- Configure web.config for Next.js routing

**Option B: Node.js Server**
```bash
npm run start
```
Runs on port 3000

### 4. Verify Frontend
```
https://alumni.kenya-airways.com
```

---

## Post-Deployment Checklist

### ✅ Backend Health Checks
- [ ] `/health` returns "Healthy"
- [ ] Database connection working
- [ ] SMTP connection working
- [ ] ERP connection working

### ✅ Background Jobs
- [ ] Access `/hangfire` dashboard
- [ ] Verify recurring jobs scheduled
- [ ] Check job execution logs

### ✅ Email Testing
- [ ] Test registration confirmation email
- [ ] Test approval email
- [ ] Test rejection email
- [ ] Check spam folder delivery

### ✅ ERP Integration
- [ ] Test ID verification
- [ ] Verify name normalization
- [ ] Check staff number auto-fill
- [ ] Test auto-approval flow

### ✅ Admin Dashboard
- [ ] Login at `/admin/login`
- [ ] View registrations list
- [ ] Test manual approval
- [ ] Test manual rejection
- [ ] Verify audit logs

### ✅ Security
- [ ] HTTPS enabled
- [ ] CORS configured correctly
- [ ] Rate limiting active
- [ ] JWT tokens working
- [ ] Admin authentication working

---

## Monitoring

### Application Logs
Check logs in:
- Windows: `C:\inetpub\logs\` or Event Viewer
- Linux: `/var/log/` or systemd journal

### Hangfire Dashboard
```
https://alumni-api.kenya-airways.com/hangfire
```
Requires admin authentication

### Health Endpoint
```
https://alumni-api.kenya-airways.com/health
```

### Database Queries
```sql
-- Check recent registrations
SELECT TOP 10 * FROM AlumniRegistrations
ORDER BY CreatedAt DESC;

-- Check email sending status
SELECT RegistrationNumber, Email, ApprovalEmailSent, ApprovalEmailSentAt
FROM AlumniRegistrations
WHERE RegistrationStatus = 'Approved';

-- Check background job status
SELECT * FROM Hangfire.Job
ORDER BY CreatedAt DESC;
```

---

## Troubleshooting

### Emails Not Sending

**Check 1: Configuration**
```json
"EnableEmailSending": true,
"UseMockEmailService": false
```

**Check 2: SMTP Credentials**
- Verify Office 365 credentials
- Check if app password needed
- Test SMTP connection: `telnet smtp.office365.com 587`

**Check 3: Logs**
Look for:
```
[EMAIL] [EMAIL 2/3] Sending APPROVAL email
[SUCCESS] [EMAIL 2/3] Approval email sent
```

### Auto-Approval Not Working

**Check 1: Background Jobs**
- Access `/hangfire` dashboard
- Check if `ApprovalProcessingJob` is running
- Look for failed jobs

**Check 2: ERP Connection**
```
GET /health
```
Should show ERP as "Healthy"

**Check 3: Name Validation**
Check logs for:
```
Name normalization: Provided 'X' → 'Y', ERP 'A' → 'B'
Name validation passed: Similarity: 100%
```

### Database Issues

**Connection Errors:**
- Verify connection string
- Check SQL Server is running
- Test connectivity from app server
- Verify firewall rules

**Migration Errors:**
```bash
dotnet ef database update --project ../KQAlumni.Infrastructure --verbose
```

---

## Environment Variables

### Required for Backend
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=***
Email__Password=***
ErpSettings__Password=***
Jwt__SecretKey=***
```

### Required for Frontend
```
NEXT_PUBLIC_API_URL=https://alumni-api.kenya-airways.com
NODE_ENV=production
```

---

## Backup & Recovery

### Database Backup
```sql
BACKUP DATABASE KQAlumni
TO DISK = 'C:\Backups\KQAlumni_backup.bak'
WITH FORMAT, COMPRESSION;
```

### Schedule Daily Backups
Create SQL Server Agent job or use Windows Task Scheduler

### Configuration Backup
- Backup `appsettings.Production.json`
- Backup `.env.production`
- Store in secure location (not in git)

---

## Scaling Considerations

### Database
- Enable connection pooling
- Add read replicas for reporting
- Regular index maintenance

### Background Jobs
- Increase Hangfire worker count
- Adjust smart scheduling intervals
- Monitor job processing times

### Caching
- Enable Redis for session caching
- Cache ERP responses (already implemented)
- Add CDN for frontend assets

---

## Support

**Technical Issues**: KQ.Alumni@kenya-airways.com
**ERP Integration**: Contact IT Infrastructure team
**Database Issues**: Contact DBA team
