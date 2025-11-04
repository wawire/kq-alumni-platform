# KQ Alumni Platform

> A modern web platform for the Kenya Airways Alumni Association, connecting former employees worldwide.

**Version**: 2.0.0 | **License**: Proprietary | **Status**: Production Ready

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18.17+
- SQL Server (LocalDB or Docker)

### Automated Setup (Recommended)
```bash
# Windows
.\start-dev.ps1

# macOS/Linux
./start-dev.sh
```

**Access Points**:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5295/swagger
- **Hangfire Dashboard**: http://localhost:5295/hangfire

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| **[DEPLOYMENT.md](DEPLOYMENT.md)** | Complete deployment guide for production |
| **[MONITORING_AND_IMPROVEMENTS.md](MONITORING_AND_IMPROVEMENTS.md)** | Monitoring, logging, and reliability features |
| **[SECURITY_PERFORMANCE_IMPROVEMENTS.md](SECURITY_PERFORMANCE_IMPROVEMENTS.md)** | Security enhancements and performance optimizations |
| **[CHANGELOG.md](CHANGELOG.md)** | Version history and updates |

---

## 🏗️ Architecture

### Technology Stack
- **Backend**: .NET 8.0 ASP.NET Core Web API + Entity Framework Core
- **Frontend**: Next.js 14 + React 18 + TypeScript
- **Database**: SQL Server 2019+
- **Background Jobs**: Hangfire
- **ERP Integration**: Oracle SOA Suite

### Project Structure
```
kq-alumni-platform/
├── KQAlumni.Backend/          # .NET Web API
│   ├── src/KQAlumni.API/      # API controllers, middleware
│   ├── src/KQAlumni.Core/     # Domain models, interfaces
│   └── src/KQAlumni.Infrastructure/  # Services, data access
├── kq-alumni-frontend/        # Next.js application
│   └── src/                   # React components, hooks, services
└── Documentation files        # Deployment guides, changelogs
```

---

## ✨ Key Features

### Registration System
- **Multi-step wizard** with form state persistence
- **Real-time validation** for email and staff number duplicates
- **ERP integration** for automatic employee verification
- **Email verification** workflow with 30-day token expiry

### Monitoring & Reliability
- **Email delivery tracking** with database logging
- **Comprehensive health checks** for SQL, SMTP, and ERP
- **Environment validation** on startup (fail-fast on misconfigurations)
- **Rate limiting** with automated monitoring and reporting

### Admin Features
- **Dashboard** for registration management
- **Manual review** workflow for flagged registrations
- **Background jobs** with smart scheduling (Hangfire)
- **Audit logging** for all admin actions

---

## 🔒 Security Features

- JWT authentication for admin access
- Rate limiting (100 requests/hour in production)
- IP whitelisting support
- CORS protection with configurable origins
- SQL injection protection (EF Core)
- Email disposable domain blocking

---

## 🧪 Validation Rules

### Staff Number Format
**Pattern**: `00[0-9A-Z]{5}` (7 characters)

**Valid Examples**:
- `0012345` - Permanent staff (5 digits)
- `00C5050` - Contract staff
- `00RG002` - Various departments
- `00EM004` - Engineering/Maintenance

**Rules**:
- Must start with `00`
- Followed by exactly 5 alphanumeric characters (0-9, A-Z)
- Must be UPPERCASE
- Length: Exactly 7 characters

### Email Format
- Standard RFC 5322 format
- Maximum 255 characters
- Disposable email domains blocked
- Case-insensitive (auto-converted to lowercase)

---

## 🚀 Deployment

For production deployment to IIS:

1. **Read the deployment guide**: See [DEPLOYMENT.md](DEPLOYMENT.md)
2. **Configure environment**: Set up `appsettings.Production.json` and `.env.production.local`
3. **Apply database migrations**: `dotnet ef database update`
4. **Deploy to IIS**: Follow steps in DEPLOYMENT.md
5. **Verify deployment**: Check `/health` endpoint

**Important**: The application validates all configuration on startup and will fail fast if settings are invalid.

---

## 🔧 Development

### Manual Setup

**Backend**:
```bash
cd KQAlumni.Backend/src/KQAlumni.API
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend**:
```bash
cd kq-alumni-frontend
npm install
npm run dev
```

### Environment Configuration

**Backend**: Create `appsettings.Development.json` from template
**Frontend**: Create `.env.local` from `.env.local.example`

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed configuration instructions.

---

## 📊 Monitoring

### Health Endpoints
- **`/health`** - Full system health check
- **`/health/ready`** - Database readiness probe
- **`/health/live`** - Liveness probe

### Email Delivery Tracking
All email delivery attempts are logged to the `EmailLogs` table:
```sql
SELECT EmailType, Status, COUNT(*) as Total,
       AVG(DurationMs) as AvgDuration
FROM EmailLogs
WHERE SentAt >= DATEADD(DAY, -7, GETUTCDATE())
GROUP BY EmailType, Status;
```

### Rate Limiting Reports
Check application logs every 15 minutes for automated rate limiting statistics.

See [MONITORING_AND_IMPROVEMENTS.md](MONITORING_AND_IMPROVEMENTS.md) for complete monitoring guide.

---

## 🐛 Troubleshooting

### Common Issues

**Configuration Validation Fails**:
- Check `appsettings.Production.json` for placeholder values
- Ensure `AppSettings.BaseUrl` is not localhost in production
- Verify JWT secret key is 32+ characters

**Email Not Sending**:
- Check `EmailLogs` table for error messages
- Verify SMTP credentials and server accessibility
- Ensure `EnableEmailSending = true` and `UseMockEmailService = false`

**Database Connection Fails**:
- Verify SQL Server is running
- Check connection string in appsettings
- Test with: `sqlcmd -S [server] -U [user] -P [password]`

For detailed troubleshooting, see [DEPLOYMENT.md](DEPLOYMENT.md).

---

## 🤝 Contributing

1. Create a feature branch from `main`
2. Follow existing code style and conventions
3. Test thoroughly (both backend and frontend)
4. Update documentation if needed
5. Create a pull request with detailed description

---

## 📞 Support

**For deployment issues**: See [DEPLOYMENT.md](DEPLOYMENT.md)
**For monitoring**: See [MONITORING_AND_IMPROVEMENTS.md](MONITORING_AND_IMPROVEMENTS.md)
**Email**: KQ.Alumni@kenya-airways.com

---

## 📝 Version History

**v2.0.0** (2025-11-04)
- ✅ Email delivery tracking with database logging
- ✅ Environment variable validation on startup
- ✅ Enhanced health checks (SQL, SMTP, ERP)
- ✅ Rate limiting monitoring service
- ✅ Standardized frontend/backend validation

**v1.0.0** (2025-10-31)
- 🚀 Initial release
- ✅ Multi-step registration wizard
- ✅ ERP integration for staff validation
- ✅ Email verification workflow
- ✅ Admin dashboard with Hangfire

See [CHANGELOG.md](CHANGELOG.md) for complete history.

---

**Built with ❤️ for the Kenya Airways Alumni Community**
