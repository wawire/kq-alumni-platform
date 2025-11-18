# KQ Alumni Platform

Kenya Airways Alumni Association registration and management platform.

**Version**: 2.2.0 | **Status**: Production Ready

---

## Overview

Web application for Kenya Airways alumni registration with:
- Multi-step registration with ERP verification
- Email verification workflow
- Admin dashboard for approvals
- Background job processing

---

## Tech Stack

**Frontend**: Next.js 14, React 18, TypeScript, TailwindCSS
**Backend**: .NET 8, ASP.NET Core, Entity Framework Core
**Database**: SQL Server 2019+
**Background Jobs**: Hangfire
**Email**: SMTP (Office 365)

---

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- SQL Server (Docker or local)

### 1. Clone & Setup
```bash
git clone https://github.com/wawire/kq-alumni-platform.git
cd kq-alumni-platform
```

### 2. Backend Setup
```bash
cd KQAlumni.Backend/src/KQAlumni.API

# Update appsettings.json with your configuration
# - Database connection string
# - SMTP credentials
# - ERP endpoint

dotnet restore
dotnet ef database update --project ../KQAlumni.Infrastructure
dotnet run
```
Backend runs at: `http://localhost:5000`

### 3. Frontend Setup
```bash
cd kq-alumni-frontend

# Create .env.local
echo "NEXT_PUBLIC_API_URL=http://localhost:5000" > .env.local

npm install
npm run dev
```
Frontend runs at: `http://localhost:3000`

---

## Configuration

### Backend (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KQAlumni;..."
  },
  "Email": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "Username": "your-email@kenya-airways.com",
    "Password": "your-password"
  },
  "ErpSettings": {
    "Endpoint": "http://your-erp-endpoint",
    "EnableMockMode": false
  }
}
```

### Frontend (`.env.local`)
```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Key Features

### Registration Flow
1. User enters ID/Passport
2. ERP verifies employment history
3. Name and staff number auto-populated
4. User completes multi-step form
5. Confirmation email sent
6. Background job validates and approves
7. Approval email with verification link
8. User verifies email → Account active

### Admin Dashboard
- View all registrations
- Manual approval/rejection
- Email resend functionality
- Audit logs
- Hangfire job monitoring at `/hangfire`

---

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for production deployment instructions.

---

## API Endpoints

### Public
- `POST /api/v1/registrations` - Create registration
- `GET /api/v1/registrations/verify-id/{id}` - Verify ID with ERP
- `POST /api/v1/registrations/verify-email` - Verify email

### Admin (Authenticated)
- `POST /api/v1/admin/login` - Admin login
- `GET /api/v1/admin/registrations` - List registrations
- `POST /api/v1/admin/registrations/{id}/approve` - Approve
- `POST /api/v1/admin/registrations/{id}/reject` - Reject

---

## Monitoring

- **Hangfire Dashboard**: `/hangfire` (admin auth required)
- **Health Checks**: `/health` (database, SMTP, ERP)
- **Application Logs**: Check console or configured log files

---

## Support

For issues or questions, contact: **KQ.Alumni@kenya-airways.com**

---

## License

Proprietary - Kenya Airways Alumni Association
