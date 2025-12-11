# 🚀 KQ Alumni Platform - Deployment Guide

## 📦 Deployment Packages

Two deployment packages have been created:

1. **kq-alumni-deployment-YYYYMMDD-HHMMSS.tar.gz** (461 KB)
   - Recommended for Linux/Unix servers
   - Compressed with gzip

2. **kq-alumni-deployment-YYYYMMDD-HHMMSS.zip** (574 KB)
   - Recommended for Windows servers
   - Compatible with all platforms

## 📋 What's Included

Both packages contain:
- ✅ **KQAlumni.Backend** - Complete .NET 8 backend application
- ✅ **kq-alumni-frontend** - Complete Next.js frontend application
- ✅ **PR_SUMMARY.md** - Detailed change documentation
- ✅ **README.md** - Project documentation

**Excluded (will need to be generated/installed):**
- ❌ node_modules (run `npm install`)
- ❌ bin/obj folders (run `dotnet build`)
- ❌ .git folder (version control metadata)
- ❌ Build artifacts and logs

## 🔧 Deployment Steps

### Option 1: Extract and Deploy

#### For Linux/Unix (tar.gz):
```bash
# Extract the package
tar -xzf kq-alumni-deployment-20251210-*.tar.gz

# Navigate to backend
cd KQAlumni.Backend

# Restore dependencies and build
dotnet restore
dotnet build --configuration Release

# Navigate to frontend
cd ../kq-alumni-frontend

# Install dependencies and build
npm install
npm run build
```

#### For Windows (zip):
```bash
# Extract using PowerShell
Expand-Archive -Path kq-alumni-deployment-20251210-*.zip -DestinationPath ./

# Navigate to backend
cd KQAlumni.Backend

# Restore dependencies and build
dotnet restore
dotnet build --configuration Release

# Navigate to frontend
cd ..\kq-alumni-frontend

# Install dependencies and build
npm install
npm run build
```

### Option 2: Deploy from Git Repository

```bash
# Clone the repository
git clone [your-repo-url]
cd kq-alumni-platform

# Checkout the feature branch
git checkout claude/alumni-approval-template-01KYtb4VMdReLb7Cmkj89Md8

# Pull latest changes
git pull origin claude/alumni-approval-template-01KYtb4VMdReLb7Cmkj89Md8

# Build backend
cd KQAlumni.Backend
dotnet restore
dotnet build --configuration Release

# Build frontend
cd ../kq-alumni-frontend
npm install
npm run build
```

## ⚙️ Configuration

### Backend Configuration

1. **Database Connection:**
   - Update `appsettings.Production.json` with production database connection string

2. **Email Settings:**
   - Verify SMTP credentials in `appsettings.Production.json`
   - Ensure `EnableEmailSending: true` and `UseMockEmailService: false`

3. **JWT Settings:**
   - Update secret keys for production
   - Set appropriate token expiration times

### Frontend Configuration

1. **Environment Variables:**
   - Copy `.env.example` to `.env.production.local`
   - Update `NEXT_PUBLIC_API_URL` to point to production backend

## 🗄️ Database Migration

```bash
cd KQAlumni.Backend

# Apply migrations to production database
dotnet ef database update --connection "your-production-connection-string"
```

## 🧪 Post-Deployment Testing

1. **Backend Health Check:**
```bash
curl https://your-api-domain.com/health
```

2. **Test Registration:**
   - Register a new user
   - Verify confirmation email is received

3. **Test Admin Panel:**
   - Log in to admin panel
   - Approve a registration (single)
   - Approve multiple registrations (bulk)
   - Verify all users receive approval emails

4. **Email Verification:**
   - Check approval emails have new welcome message
   - Verify NO verification link in approval emails
   - Verify consistent font sizes across all emails

## 📝 Important Changes in This Release

### ⚠️ CRITICAL: Bulk Approve Email Fix
The bulk approve function now automatically sends approval emails.
**Test this thoroughly after deployment!**

### Email Template Changes
- Approval emails no longer have verification links
- All email templates now have standardized font sizes
- Welcome message format updated

### Configuration Changes
- Email sending is now enabled by default in Development
- Mock email service is disabled by default

## 🔐 Security Checklist

- [ ] Update all secret keys in production
- [ ] Enable HTTPS for API and frontend
- [ ] Configure firewall rules for SMTP (port 587/465)
- [ ] Verify CORS settings for production domains
- [ ] Enable rate limiting in production
- [ ] Review IP whitelist settings
- [ ] Update connection strings

## 📊 Monitoring

After deployment, monitor:
1. **Email Sending Logs**
   - Check for "Approval email sent successfully" messages
   - Monitor email delivery rates

2. **Database Logs**
   - Verify ApprovalEmailSent and ApprovalEmailSentAt are being set

3. **Error Logs**
   - Watch for any SMTP connection errors
   - Check for email sending failures

## 🆘 Troubleshooting

### Emails Not Sending
1. Check `EnableEmailSending` is `true`
2. Check `UseMockEmailService` is `false`
3. Verify SMTP credentials
4. Check firewall allows SMTP traffic
5. Review email service logs

### Bulk Approve Not Sending Emails
1. Verify you're on the latest version (commit 28a88af or later)
2. Check backend logs for "Approval email sent successfully" messages
3. Verify ApprovalEmailSent flag is being set in database

## 📞 Support

For issues or questions:
- Review `PR_SUMMARY.md` for detailed changes
- Check backend logs: `/var/log/kqalumni/` (or your log path)
- Review email logs in database: `SELECT * FROM EmailLogs ORDER BY SentAt DESC`

## 📅 Deployment Date

**Prepared:** 2025-12-10
**Branch:** claude/alumni-approval-template-01KYtb4VMdReLb7Cmkj89Md8
**Version:** Post-approval-template-update

---

**✅ Ready for Deployment!**
