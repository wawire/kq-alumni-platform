# KQ Alumni Platform

A comprehensive web platform for the Kenya Airways Alumni Association, enabling former employees to register, stay connected, and engage with the Kenya Airways community.

## Overview

The KQ Alumni Platform is a full-stack application built with modern technologies:

- **Backend**: .NET 8.0 ASP.NET Core Web API with Entity Framework Core
- **Frontend**: Next.js 14 with React 18 and TypeScript
- **Database**: SQL Server
- **Background Jobs**: Hangfire
- **ERP Integration**: Oracle SOA Suite integration for employee validation

## Features

### Registration System
- **Multi-step Registration Wizard**
  - Step 1: Personal Information (name, email, phone, location)
  - Step 2: Employment Information (staff number, department, dates, certifications)
  - Step 3: Engagement Preferences (volunteer interests, events, mentorship, newsletter)
  - Real-time form validation with Zod schemas
  - Form state persistence across page refreshes (Zustand + localStorage)

- **Real-time Validation**
  - Duplicate email detection (debounced API checks)
  - Duplicate phone number detection
  - ERP integration for automatic staff validation
  - Inline error messages and validation feedback

- **Email Verification Flow**
  - Automated email confirmation with verification token
  - Token-based email verification endpoint
  - Success/error screens with user guidance

### Backend Features
- Background job processing for approvals (Hangfire)
- Email notifications (confirmation and approval/rejection)
- Admin dashboard for job monitoring (Hangfire UI)
- Rate limiting and security features
- Mock services for local development (Email & ERP)
- Health check endpoints
- Comprehensive API documentation (Swagger)

### UI/UX Features
- Fully responsive design (mobile-first)
- Loading states and optimistic updates
- Error boundaries for graceful error handling
- Toast notifications for user feedback
- Accessible form components (ARIA labels, keyboard navigation)
- Kenya Airways branding (KQ Red #E30613, Cabrito & Roboto fonts)

## Project Structure

```
kq-alumni-platform/
├── KQAlumni.Backend/                 # Backend .NET solution
│   ├── src/
│   │   ├── KQAlumni.API/             # Web API project
│   │   ├── KQAlumni.Core/            # Domain models and interfaces
│   │   └── KQAlumni.Infrastructure/  # Data access and services
│   └── tests/                        # Unit and integration tests
│
├── kq-alumni-frontend/               # Frontend Next.js application
│   ├── src/
│   │   ├── app/                      # Next.js app router pages
│   │   ├── components/               # React components (ui, forms, registration)
│   │   ├── services/                 # API services
│   │   ├── store/                    # Zustand state management
│   │   ├── hooks/                    # Custom hooks
│   │   ├── types/                    # TypeScript definitions
│   │   ├── utils/                    # Validation schemas and helpers
│   │   └── constants/                # App constants
│   └── public/                       # Static assets
│
├── DEPLOYMENT_GUIDE.md               # IIS deployment guide
├── SQL_SERVER_SETUP.md               # Database setup guide
├── CHANGELOG.md                      # Version history
├── docker-compose.yml                # SQL Server Docker setup
├── start-dev.ps1                     # Windows startup script
└── start-dev.sh                      # macOS/Linux startup script
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js 18.17+ - [Download](https://nodejs.org/)
- SQL Server (choose one):
  - **LocalDB** (Windows) - [Download](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) or included with Visual Studio
  - **Docker** (All platforms) - [Download](https://www.docker.com/products/docker-desktop)
- Git - [Download](https://git-scm.com/downloads)

### Clone Repository

```bash
git clone https://github.com/wawire/kq-alumni-platform.git
cd kq-alumni-platform
```

### Automated Start (Recommended)

**Windows (PowerShell):**
```powershell
.\start-dev.ps1
```

**macOS/Linux:**
```bash
./start-dev.sh
```

The script will:
- ✅ Check prerequisites
- ✅ Install frontend dependencies
- ✅ Start backend in a new window
- ✅ Start frontend in a new window
- ✅ Verify backend health

Access the application:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5295
- **Swagger Docs**: http://localhost:5295/swagger
- **Hangfire Dashboard**: http://localhost:5295/hangfire

### Manual Setup

<details>
<summary>Click to expand manual setup instructions</summary>

#### Step 1: Database Setup

**Option A: Docker SQL Server (Recommended for Linux/Mac)**

```bash
# Start SQL Server container
docker-compose up -d

# Verify it's running
docker ps
# You should see: kqalumni-sqlserver running on port 1433
```

Connection Details:
- Server: `localhost,1433`
- Username: `sa`
- Password: `YourStrong@Passw0rd`
- Database: `KQAlumniDB` (will be created automatically)

**Option B: LocalDB (Windows Only)**

```powershell
# Verify LocalDB is installed
SqlLocalDB.exe info

# Create and start instance
SqlLocalDB.exe create MSSQLLocalDB
SqlLocalDB.exe start MSSQLLocalDB
```

Connection string uses: `Server=(LocalDB)\\MSSQLLocalDB;Database=KQAlumniDB;Integrated Security=true;TrustServerCertificate=true;`

For detailed database setup instructions, see [SQL_SERVER_SETUP.md](SQL_SERVER_SETUP.md).

#### Step 2: Backend Setup

```bash
cd KQAlumni.Backend/src/KQAlumni.API

# Restore dependencies
dotnet restore

# Apply database migrations (creates database and tables)
dotnet ef database update

# If dotnet-ef is not installed:
# dotnet tool install --global dotnet-ef

# Start the backend
dotnet run
```

The API will be available at `http://localhost:5295`

#### Step 3: Frontend Setup

```bash
# In a new terminal
cd kq-alumni-frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:3000`

</details>

### First Time Testing

1. **Test Registration**: Go to http://localhost:3000 and click "Register Now"
2. **Use Mock Staff Numbers**: `0012345`, `00C5050`, or `00A1234` (ERP is in mock mode)
3. **Access Admin**: http://localhost:3000/admin/login (create admin user via API first)

## Development

### Backend Development

**Commands:**
- **Build**: `dotnet build`
- **Run**: `dotnet run`
- **Test**: `dotnet test`
- **Publish**: `dotnet publish -c Release`

**Mock Services:**

For local development, the backend includes mock services:

- **Mock Email Service**: Logs emails to console instead of sending via SMTP
  - Enable in `appsettings.Development.json`: `"UseMockEmailService": true`

- **Mock ERP Service**: Simulates ERP validation without network connection
  - Enable in `appsettings.Development.json`: `"EnableMockMode": true`
  - Configure test staff numbers in `MockStaffNumbers` array

**Hangfire Dashboard:**

Access the background job dashboard at: `http://localhost:5295/hangfire`

### Frontend Development

**Commands:**
- **Dev server**: `npm run dev`
- **Build**: `npm run build`
- **Production**: `npm run start`
- **Lint**: `npm run lint`
- **Type check**: `npm run type-check`
- **Format**: `npm run format`

**Environment Variables:**

- `.env.local` - Local development (git-ignored)
- `.env.development.local` - Development environment (git-ignored)
- `.env.production.local` - Production environment (git-ignored)
- `.env.local.example` - Template for local development
- `.env.production.local.example` - Template for production

## Configuration

### Backend Configuration Files

- `appsettings.json` - Base configuration (localhost settings)
- `appsettings.template.json` - Template for creating environment-specific configs
- `appsettings.Development.template.json` - Development template
- `appsettings.Production.template.json` - Production template
- `appsettings.Development.json` - Development overrides (git-ignored, create from template)
- `appsettings.Production.json` - Production settings (git-ignored, create from template)

### Creating Environment-Specific Configs

**For Development:**
```bash
cd KQAlumni.Backend/src/KQAlumni.API
cp appsettings.Development.template.json appsettings.Development.json
# Edit appsettings.Development.json with your connection strings and settings
```

**For Production:**
```bash
cd KQAlumni.Backend/src/KQAlumni.API
cp appsettings.Production.template.json appsettings.Production.json
# Edit appsettings.Production.json with production settings
# IMPORTANT: Change JWT SecretKey, connection strings, and SMTP credentials!
```

### Key Configuration Sections

**Database Connection:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(LocalDB)\\MSSQLLocalDB;Database=KQAlumniDB;Integrated Security=true;TrustServerCertificate=true;"
}
```

**Email Service:**
```json
"Email": {
  "SmtpServer": "your-smtp-server",
  "SmtpPort": 587,
  "EnableSsl": true,
  "Username": "your-smtp-username",
  "Password": "your-smtp-password",
  "From": "KQ.Alumni@kenya-airways.com",
  "DisplayName": "Kenya Airways Alumni Relations",
  "EnableEmailSending": false,
  "UseMockEmailService": true
}
```

**ERP Integration:**
```json
"ErpApi": {
  "BaseUrl": "http://your-erp-server:port",
  "Endpoint": "/soa-infra/resources/default/HR_Leavers/RestService/Leavers",
  "Timeout": 30,
  "RetryCount": 3,
  "EnableMockMode": true,
  "MockStaffNumbers": ["0012345", "00C5050", "00A1234"]
}
```

## Troubleshooting

### Database Connection Errors

**Docker SQL Server not ready:**
```bash
# Check Docker is running
docker ps

# Check SQL Server logs
docker logs kqalumni-sqlserver
# Wait until you see: "SQL Server is now ready for client connections"

# Test the connection
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"

# Restart if needed
docker-compose restart sqlserver
```

**LocalDB issues (Windows):**
```powershell
# Check if LocalDB is running
SqlLocalDB.exe info MSSQLLocalDB

# Start LocalDB
SqlLocalDB.exe start MSSQLLocalDB

# Verify database exists
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -E -Q "SELECT name FROM sys.databases WHERE name = 'KQAlumniDB'"
```

### Backend Won't Start

- **Check SQL Server connection**: Verify database is running and connection string is correct
- **Verify .NET 8.0 SDK**: Run `dotnet --version`
- **Check logs**: Review console output for error messages
- **Apply migrations**: Run `dotnet ef database update` from `KQAlumni.Backend/src/KQAlumni.API`

### Frontend Can't Connect to Backend

- **Verify backend is running**: Check `http://localhost:5295/health`
- **Check API URL**: Verify `NEXT_PUBLIC_API_URL` in `.env.local`
- **Check CORS settings**: Backend must allow `http://localhost:3000`

### Email Not Sending

- **Enable mock mode**: Set `"UseMockEmailService": true` in appsettings
- **Check SMTP credentials**: Verify username and password
- **Verify network connectivity**: Ensure SMTP server is reachable

### ERP Integration Failing

- **Enable mock mode**: Set `"EnableMockMode": true` in ErpApi settings
- **Verify network connectivity**: Ensure ERP server is reachable
- **Check ERP endpoint URL**: Verify URL and credentials

### Common Error Messages

**"The term 'dotnet-ef' is not recognized"**
```bash
dotnet tool install --global dotnet-ef
```

**"Build failed" when running migrations**
```bash
# Ensure the build succeeds first
dotnet build
# Then run migrations
dotnet ef database update
```

**"Cannot open database"**
- Verify SQL Server/LocalDB is running
- Check connection string is correct
- Verify SQL Server allows connections

## Deployment

For deploying to production environments (IIS on Windows Server), see the comprehensive deployment guide:

**📖 [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)** - Complete IIS deployment instructions

The deployment guide covers:
- Server prerequisites
- Backend and frontend deployment
- IIS configuration
- SSL/TLS setup
- Troubleshooting

## Architecture

### Backend Architecture

**Clean/Layered Architecture**:

- **API Layer** (`KQAlumni.API`)
  - Controllers, middleware, configuration
  - Swagger/OpenAPI documentation
  - Health checks

- **Core Layer** (`KQAlumni.Core`)
  - Domain models
  - Repository interfaces
  - Business logic interfaces

- **Infrastructure Layer** (`KQAlumni.Infrastructure`)
  - Entity Framework DbContext
  - Repository implementations
  - External service integrations (Email, ERP)
  - Background jobs (Hangfire)

### Frontend Architecture

**Next.js 14 App Router with Modern React Patterns**:

- **Pages** (`src/app/`) - Server and client components with app router
- **Components** (`src/components/`) - UI, form, and registration components
- **State Management** - Zustand for client state, React Query for server state
- **Services** (`src/services/`) - API client with axios
- **Types** (`src/types/`) - TypeScript definitions
- **Hooks** (`src/hooks/`) - Custom React hooks (useDebounce, useDuplicateCheck)
- **Utils** (`src/utils/`) - Validation schemas with Zod

## API Documentation

### Swagger UI

When running locally, access the API documentation at: `http://localhost:5295/swagger`

### Main Endpoints

- `POST /api/v1/registrations` - Submit registration
- `GET /api/v1/registrations/{id}` - Get registration by ID
- `GET /api/v1/registrations/verify/{token}` - Verify email
- `GET /api/health` - Health check endpoint

## Testing

### Backend Tests

```bash
cd KQAlumni.Backend
dotnet test
```

### Frontend Tests

```bash
cd kq-alumni-frontend
npm test
```

## Technology Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- SQL Server
- Hangfire (background jobs)
- FluentValidation
- Polly (resilience)
- Serilog (logging)
- Swashbuckle (Swagger/OpenAPI)

### Frontend
- Next.js 14
- React 18
- TypeScript 5.3+
- Tailwind CSS v3.4
- React Hook Form 7.65 + Zod 3.25
- Axios 1.12
- TanStack React Query 5.90
- Zustand 4.4
- React Select 5.10
- React Phone Input 2
- Country State City 3.2
- Sonner 2.0 (toast notifications)
- Heroicons 2.2

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Test thoroughly (both backend and frontend)
4. Create a pull request

## License

Proprietary - Kenya Airways Alumni Association

## Support

For support and questions, please contact:
- Development Team: [Contact details]
- System Administrator: [Contact details]

---

**Version**: 1.0.0
**Last Updated**: 2025-10-31
