# SQL Server Setup Guide for KQ Alumni Platform

## Problem

The application is failing to start with the error:
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
The server was not found or was not accessible.
```

This is because the default `appsettings.json` uses **LocalDB**, which is only for local development and is not available on production servers.

## Solution Overview

You need to:
1. Install SQL Server on your server (or use an existing SQL Server instance)
2. Configure the connection string in production
3. Run database migrations
4. Start the application

---

## Option 1: Using SQL Server Express (Free, for Small-Medium deployments)

### Step 1: Install SQL Server Express

1. **Download SQL Server Express**
   - Visit: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Download "SQL Server 2022 Express" (or latest version)

2. **Install with the following options:**
   - Choose "Custom" installation
   - Select "Database Engine Services"
   - **Authentication Mode**: Mixed Mode (SQL Server and Windows Authentication)
   - Set a strong SA password (save this securely!)
   - Enable TCP/IP protocol during installation

3. **Configure SQL Server:**
   ```powershell
   # Open SQL Server Configuration Manager
   # Enable TCP/IP protocol:
   # - Expand "SQL Server Network Configuration"
   # - Click "Protocols for SQLEXPRESS"
   # - Right-click "TCP/IP" → Enable
   # - Restart SQL Server service
   ```

### Step 2: Create the Database and User

```sql
-- Connect to SQL Server using SQL Server Management Studio (SSMS) or Azure Data Studio
-- Use Windows Authentication or SA account

-- 1. Create the database
CREATE DATABASE KQAlumniDB;
GO

-- 2. Create a dedicated application user
USE [master];
GO

CREATE LOGIN kqalumni_app WITH PASSWORD = 'YOUR_STRONG_PASSWORD_HERE';
GO

USE [KQAlumniDB];
GO

CREATE USER kqalumni_app FOR LOGIN kqalumni_app;
GO

-- 3. Grant permissions
ALTER ROLE db_owner ADD MEMBER kqalumni_app;
GO
```

### Step 3: Update Connection String

**For Named Instance (SQLEXPRESS):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_STRONG_PASSWORD_HERE;TrustServerCertificate=true;Encrypt=true;",
  "HangfireConnection": "Server=localhost\\SQLEXPRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_STRONG_PASSWORD_HERE;TrustServerCertificate=true;Encrypt=true;"
}
```

**For Default Instance:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_STRONG_PASSWORD_HERE;TrustServerCertificate=true;Encrypt=true;",
  "HangfireConnection": "Server=localhost;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_STRONG_PASSWORD_HERE;TrustServerCertificate=true;Encrypt=true;"
}
```

---

## Option 2: Using Existing SQL Server (Standard/Enterprise)

### If you have an existing SQL Server instance:

1. **Get the server name/address:**
   - Local: `localhost` or `SERVERNAME\INSTANCENAME`
   - Remote: `192.168.1.100` or `sql-server.yourdomain.com`

2. **Create database and user** (same SQL commands as Option 1)

3. **Update connection string:**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_ADDRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=true;",
     "HangfireConnection": "Server=YOUR_SERVER_ADDRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=true;"
   }
   ```

---

## Configuration Methods

### Method 1: Using appsettings.Production.json (Recommended)

1. The file `appsettings.Production.json` has been created at:
   ```
   KQAlumni.Backend/src/KQAlumni.API/appsettings.Production.json
   ```

2. **Update the connection strings** in this file with your actual SQL Server details

3. **Set the environment variable:**
   ```powershell
   # Windows (PowerShell)
   [System.Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Production', 'Machine')

   # Or set in IIS Application Pool settings
   ```

4. **Update other settings:**
   - `JwtSettings.SecretKey`: Use a strong, randomly generated key (minimum 32 characters)
   - `Email.*`: Configure your SMTP settings
   - `ErpApi.BaseUrl`: Your ERP server URL
   - `CorsSettings.AllowedOrigins`: Your frontend URL

### Method 2: Using Environment Variables (Most Secure)

Instead of storing credentials in config files, use environment variables:

```powershell
# Windows PowerShell (set as system variables)
[System.Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'Server=localhost\SQLEXPRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=true;', 'Machine')

[System.Environment]::SetEnvironmentVariable('ConnectionStrings__HangfireConnection', 'Server=localhost\SQLEXPRESS;Database=KQAlumniDB;User Id=kqalumni_app;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=true;', 'Machine')

[System.Environment]::SetEnvironmentVariable('JwtSettings__SecretKey', 'YOUR-STRONG-SECRET-KEY-HERE', 'Machine')
```

**Note:** Double underscores (`__`) in environment variable names represent nested JSON structure.

### Method 3: Using IIS Configuration

If deploying to IIS, set connection strings in the Application Pool:

1. Open IIS Manager
2. Select your Application Pool
3. Advanced Settings → Environment Variables
4. Add:
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: `Server=...;Database=...`

---

## Running Database Migrations

After SQL Server is configured, run migrations to create tables:

```powershell
# Navigate to the API project
cd KQAlumni.Backend/src/KQAlumni.API

# Set environment to Production (if using appsettings.Production.json)
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Run the application - it will automatically apply migrations
dotnet run

# Or if you have EF Core tools installed:
dotnet ef database update
```

The application uses **automatic migrations** on startup (configured in `Program.cs`), so the database schema will be created automatically when the app starts.

---

## Verifying SQL Server Installation

### Check if SQL Server is running:

```powershell
# PowerShell
Get-Service | Where-Object {$_.DisplayName -like '*SQL Server*'}

# Should show services like:
# - SQL Server (SQLEXPRESS) - Status: Running
# - SQL Server Agent (SQLEXPRESS) - Status: Running (optional)
```

### Test connection:

```powershell
# Using sqlcmd (installed with SQL Server)
sqlcmd -S localhost\SQLEXPRESS -U kqalumni_app -P YOUR_PASSWORD -Q "SELECT @@VERSION"

# Should return SQL Server version information
```

---

## Firewall Configuration (If using remote SQL Server)

If SQL Server is on a different machine:

```powershell
# Windows Firewall - Allow SQL Server (default port 1433)
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

Also enable remote connections in SQL Server:
1. Open SQL Server Configuration Manager
2. SQL Server Network Configuration → Protocols for [INSTANCE]
3. Enable TCP/IP
4. Restart SQL Server service

---

## Troubleshooting

### Error: "Login failed for user 'kqalumni_app'"
- **Solution:** Verify the username/password in your connection string
- Check the user was created correctly in SQL Server
- Ensure the user has permissions on the KQAlumniDB database

### Error: "Cannot open database 'KQAlumniDB'"
- **Solution:** Create the database first using the SQL commands above
- Or allow the application to create it on first run (requires elevated permissions)

### Error: "A network-related or instance-specific error"
- **Solution:**
  - Verify SQL Server service is running
  - Check the server name/instance name in connection string
  - Verify TCP/IP protocol is enabled
  - Check firewall settings

### Error: "Certificate chain not trusted"
- **Solution:** Add `TrustServerCertificate=true` to your connection string (already included above)

---

## Security Best Practices

1. **Never commit production passwords to Git**
   - Use environment variables or Azure Key Vault
   - `appsettings.Production.json` should be in `.gitignore`

2. **Use strong passwords**
   - Minimum 16 characters
   - Mix of uppercase, lowercase, numbers, symbols

3. **Restrict database user permissions**
   - Don't use SA account for the application
   - Use dedicated application user with minimal required permissions

4. **Enable SQL Server encryption**
   - Force encrypted connections
   - Use SSL certificates in production

5. **Regular backups**
   ```sql
   -- Example backup script
   BACKUP DATABASE KQAlumniDB
   TO DISK = 'C:\Backups\KQAlumniDB.bak'
   WITH FORMAT, COMPRESSION;
   ```

---

## Next Steps

After SQL Server is configured:

1. ✅ Update `appsettings.Production.json` with your connection string
2. ✅ Set `ASPNETCORE_ENVIRONMENT=Production`
3. ✅ Run the application
4. ✅ Verify database tables are created
5. ✅ Access the application and test registration
6. ✅ Monitor logs for any issues

---

## Quick Reference: Connection String Formats

**LocalDB (Development only):**
```
Server=(LocalDB)\\MSSQLLocalDB;Database=KQAlumniDB;Integrated Security=true;TrustServerCertificate=true;
```

**SQL Server Express (Local, SQL Auth):**
```
Server=localhost\\SQLEXPRESS;Database=KQAlumniDB;User Id=username;Password=password;TrustServerCertificate=true;Encrypt=true;
```

**SQL Server Express (Local, Windows Auth):**
```
Server=localhost\\SQLEXPRESS;Database=KQAlumniDB;Integrated Security=true;TrustServerCertificate=true;
```

**SQL Server Standard/Enterprise (Remote):**
```
Server=192.168.1.100;Database=KQAlumniDB;User Id=username;Password=password;TrustServerCertificate=true;Encrypt=true;
```

**SQL Server with specific port:**
```
Server=192.168.1.100,1433;Database=KQAlumniDB;User Id=username;Password=password;TrustServerCertificate=true;Encrypt=true;
```

---

## Support

If you continue to experience issues:
1. Check the application logs for detailed error messages
2. Verify SQL Server is accessible: `telnet localhost 1433`
3. Test connection with SQL Server Management Studio (SSMS)
4. Review SQL Server error logs in Event Viewer
