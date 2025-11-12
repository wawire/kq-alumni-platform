# KQ Alumni Platform - Local Setup Guide

## 📥 STEP 1: Clone the Repository

Open your terminal/command prompt and run:

```bash
# Using HTTPS
git clone https://github.com/wawire/kq-alumni-platform.git

# Navigate into the project
cd kq-alumni-platform
```

## 📦 STEP 2: Frontend Setup (Next.js)

```bash
# Go to frontend directory
cd kq-alumni-frontend

# Install dependencies
npm install

# Create environment file
cp .env.local.example .env.local
```

**Edit `.env.local`** and set:
```env
NEXT_PUBLIC_API_URL=http://localhost:5295
NEXT_PUBLIC_ENV=development
NEXT_PUBLIC_SITE_URL=http://localhost:3000
NEXT_PUBLIC_API_TIMEOUT=30000
NEXT_PUBLIC_ENABLE_DEBUG_MODE=true
```

**Start the frontend:**
```bash
npm run dev
```
Frontend runs on: **http://localhost:3000**

## ⚙️ STEP 3: Backend Setup (.NET 8)

### Prerequisites:
- **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download
- **SQL Server** (LocalDB or full version)
- **Visual Studio 2022** (optional but recommended)

### Setup Commands:

```bash
# From project root, go to backend
cd KQAlumni.Backend

# Restore packages
dotnet restore

# Build the project
dotnet build
```

### Configure Database:

**Edit: `KQAlumni.API/appsettings.Development.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KQAlumniDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Run migrations:**
```bash
# Create/update database
dotnet ef database update --project KQAlumni.Infrastructure --startup-project KQAlumni.API
```

**Start the backend:**
```bash
dotnet run --project KQAlumni.API
```
Backend API runs on: **https://localhost:5295**

## 🗄️ STEP 4: Database Setup (if needed)

If you need to reset the database:

**Windows:**
```bash
.\reset-database.bat
```

**Linux/Mac:**
```bash
chmod +x reset-database.sh
./reset-database.sh
```

## 🔐 STEP 5: Admin Access

Default admin credentials (after running migrations):
- **Username**: `superadmin`
- **Password**: Check the migrations or create your own

## ✅ STEP 6: Verify Setup

1. **Backend Health Check**: http://localhost:5295/health
2. **Frontend**: http://localhost:3000
3. **API Docs** (if Swagger enabled): http://localhost:5295/swagger

## 📝 Common Issues

### Issue: "Cannot connect to database"
**Solution**:
- Check SQL Server is running
- Verify connection string in appsettings.Development.json
- Run `dotnet ef database update`

### Issue: "Port 3000 already in use"
**Solution**:
```bash
# Change port in package.json or run:
npm run dev -- -p 3001
```

### Issue: "Port 5295 already in use"
**Solution**: Edit `KQAlumni.API/Properties/launchSettings.json` and change the port

### Issue: ".NET SDK not found"
**Solution**: Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

## 🚀 Project Structure

```
kq-alumni-platform/
├── kq-alumni-frontend/          # Next.js frontend
│   ├── src/
│   ├── public/
│   ├── .env.local               # Your local config
│   └── package.json
│
├── KQAlumni.Backend/            # .NET backend
│   ├── KQAlumni.API/            # Web API project
│   ├── KQAlumni.Core/           # Domain models
│   ├── KQAlumni.Infrastructure/ # Database, ERP
│   └── KQAlumni.Tests/          # Unit tests
│
└── README.md
```

## 📚 Additional Resources

- **Full Documentation**: See README.md in project root
- **Deployment Guide**: See DEPLOYMENT.md
- **API Documentation**: Available at /swagger when running in development

## 💡 Development Tips

1. **Hot Reload**: Both frontend and backend support hot reload
2. **Debugging**: Use VS Code or Visual Studio for .NET debugging
3. **Database**: Use SQL Server Management Studio or Azure Data Studio to view data
4. **Logs**: Check console output for both frontend and backend

## 🔗 Important URLs (Local Development)

- Frontend: http://localhost:3000
- Backend API: http://localhost:5295
- Health Check: http://localhost:5295/health
- Admin Login: http://localhost:3000/admin/login
- Registration: http://localhost:3000/register

---

**Need Help?**
- Check the main README.md for detailed documentation
- Review DEPLOYMENT.md for production setup
- Check console logs for error messages
