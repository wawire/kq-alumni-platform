# Admin User Seeding Guide

This guide explains how to create the initial SuperAdmin user for the KQ Alumni Platform admin dashboard.

## Table of Contents

- [Quick Start (Development)](#quick-start-development)
- [Production Setup](#production-setup)
- [Manual Database Seeding](#manual-database-seeding)
- [Programmatic Seeding](#programmatic-seeding)
- [Troubleshooting](#troubleshooting)

---

## Quick Start (Development)

### Method 1: Using the Seeding API Endpoint (Recommended for Dev)

**Prerequisites:**
- Backend API must be running
- Database migrations must be applied

**Steps:**

1. **Run the seeding script:**
   ```bash
   cd KQAlumni.Backend/scripts
   ./seed-admin-user.sh dev
   ```

2. **Or use curl directly:**
   ```bash
   curl -X POST http://localhost:5000/api/v1/admin/seed-initial-admin
   ```

3. **Login with the default credentials:**
   ```
   Username: admin
   Password: Admin@123456
   Email: admin@kenya-airways.com
   ```

4. **⚠️ IMPORTANT:** Change the password immediately after first login!

---

### Method 2: Using Postman/API Client

**Request:**
```http
POST http://localhost:5000/api/v1/admin/seed-initial-admin
Content-Type: application/json
```

**Expected Response (201 Created):**
```json
{
  "message": "Initial SuperAdmin user created successfully",
  "username": "admin",
  "password": "Admin@123456",
  "email": "admin@kenya-airways.com",
  "warning": "⚠️ CHANGE THIS PASSWORD IMMEDIATELY AFTER FIRST LOGIN!",
  "instructions": [
    "1. Login at /api/v1/admin/login with these credentials",
    "2. Create additional admin users via /api/v1/admin/users",
    "3. Change this default password immediately"
  ]
}
```

---

## Production Setup

### Option 1: Manual Password Hash Generation (Recommended)

**Prerequisites:**
- Access to SQL Server Management Studio or Azure Data Studio
- BCrypt password hashing tool

**Steps:**

1. **Generate BCrypt password hash:**

   **Using C# (dotnet script):**
   ```bash
   cd KQAlumni.Backend/src/KQAlumni.API
   dotnet script -c "Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(\"YourSecurePassword123\", 12));"
   ```

   **Using online BCrypt generator (not recommended for production):**
   - Visit: https://bcrypt-generator.com/
   - Enter your password
   - Select rounds: 12
   - Copy the generated hash

2. **Insert admin user into database:**
   ```sql
   -- Replace YOUR_BCRYPT_HASH_HERE with the hash from step 1
   DECLARE @PasswordHash NVARCHAR(255) = 'YOUR_BCRYPT_HASH_HERE';

   INSERT INTO AdminUsers (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
   VALUES (
       'admin',
       'admin@kenya-airways.com',
       @PasswordHash,
       'System Administrator',
       'SuperAdmin',
       1,
       GETUTCDATE()
   );

   -- Verify the insert
   SELECT Id, Username, Email, FullName, Role, IsActive, CreatedAt
   FROM AdminUsers
   WHERE Username = 'admin';
   ```

3. **Test login:**
   ```bash
   curl -X POST https://kqalumniapi-dev.kenya-airways.com/api/v1/admin/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "admin",
       "password": "YourSecurePassword123"
     }'
   ```

---

### Option 2: Using DbSeeder in Code (Production Startup)

**Modify `Program.cs` to seed on startup (one-time only):**

```csharp
// In Program.cs, after app.Build() but before app.Run()

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();

    // Seed initial SuperAdmin (only runs if no admin users exist)
    await DbSeeder.SeedInitialAdminUserAsync(
        scope.ServiceProvider,
        username: "admin",
        email: "admin@kenya-airways.com",
        password: "ChangeThisSecurePassword123!",  // Set strong password
        fullName: "System Administrator"
    );
}
```

**⚠️ Important:** Remove this code after initial deployment!

---

## Manual Database Seeding

### Seeding Multiple Admin Users

Create a custom seeding method in your deployment script:

```csharp
using KQAlumni.Infrastructure.Data;

var adminUsers = new List<AdminUserSeedData>
{
    new AdminUserSeedData
    {
        Username = "admin",
        Email = "admin@kenya-airways.com",
        Password = "SecurePassword123!",
        FullName = "System Administrator",
        Role = "SuperAdmin"
    },
    new AdminUserSeedData
    {
        Username = "hr.manager",
        Email = "hr.manager@kenya-airways.com",
        Password = "SecurePassword123!",
        FullName = "HR Manager",
        Role = "HRManager"
    },
    new AdminUserSeedData
    {
        Username = "hr.officer",
        Email = "hr.officer@kenya-airways.com",
        Password = "SecurePassword123!",
        FullName = "HR Officer",
        Role = "HROfficer"
    }
};

await DbSeeder.SeedAdminUsersAsync(serviceProvider, adminUsers);
```

---

## Programmatic Seeding

### Using IAuthService Directly

```csharp
// In a controller or startup code
var adminUser = await _authService.CreateAdminUserAsync(
    username: "john.doe",
    email: "john.doe@kenya-airways.com",
    password: "SecurePassword123!",
    fullName: "John Doe",
    role: "HRManager"
);
```

---

## Troubleshooting

### Error: "Admin user already exists"

**Cause:** An admin user has already been created.

**Solution:** Use the login endpoint with existing credentials, or reset the admin user in the database:

```sql
-- View existing admin users
SELECT Id, Username, Email, FullName, Role, IsActive
FROM AdminUsers;

-- Delete all admin users (⚠️ CAUTION!)
DELETE FROM AdminUsers;
```

---

### Error: "This endpoint is only available in Development environment"

**Cause:** You're trying to use `/api/v1/admin/seed-initial-admin` in Production.

**Solution:** Use one of the production setup methods instead (manual SQL insert or programmatic seeding).

---

### Error: "Invalid username or password"

**Cause:** Incorrect BCrypt hash or password mismatch.

**Solution:**
1. Verify the password hash was generated correctly (work factor: 12)
2. Ensure there are no extra spaces in the password
3. Check the password meets minimum requirements (8+ characters)

---

### Error: "Violation of UNIQUE KEY constraint 'UQ_AdminUsers_Username'"

**Cause:** An admin user with that username already exists.

**Solution:** Choose a different username or delete the existing user:

```sql
-- Check existing usernames
SELECT Username FROM AdminUsers;

-- Delete specific user
DELETE FROM AdminUsers WHERE Username = 'admin';
```

---

## Security Best Practices

1. **Never use default passwords in production**
   - Change `Admin@123456` immediately
   - Use strong passwords (12+ characters, mix of upper/lower/numbers/symbols)

2. **Limit SuperAdmin accounts**
   - Create only 1-2 SuperAdmin accounts
   - Use HRManager/HROfficer roles for regular staff

3. **Disable seeding endpoint in production**
   - The `/api/v1/admin/seed-initial-admin` endpoint is automatically disabled in Production
   - Never enable it in production deployments

4. **Rotate passwords regularly**
   - Implement password change policy
   - Force password reset on first login

5. **Monitor admin access**
   - Review AuditLogs table regularly
   - Track all admin actions via the audit trail

---

## Next Steps After Seeding

1. **Login with SuperAdmin credentials**
   ```bash
   POST /api/v1/admin/login
   {
     "username": "admin",
     "password": "Admin@123456"
   }
   ```

2. **Change default password** (future feature - password change endpoint)

3. **Create additional admin users:**
   ```bash
   POST /api/v1/admin/users
   Authorization: Bearer <JWT_TOKEN>
   {
     "username": "hr.manager",
     "email": "hr.manager@kenya-airways.com",
     "password": "SecurePassword123!",
     "fullName": "HR Manager",
     "role": "HRManager"
   }
   ```

4. **Start using the admin dashboard** at `/admin/login` (frontend - to be implemented)

---

## Admin User Roles

| Role | Permissions |
|------|-------------|
| **SuperAdmin** | Full access: Create/delete admin users, approve/reject registrations, view all data |
| **HRManager** | Approve/reject registrations, view dashboard, create HROfficer users |
| **HROfficer** | View registrations, view dashboard, read-only access |

---

## Support

For issues with admin user seeding:
1. Check database migrations are applied: `dotnet ef database update`
2. Verify database connectivity
3. Check application logs for detailed error messages
4. Review this guide's troubleshooting section

---

**Last Updated:** 2025-10-30
