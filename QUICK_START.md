# 🚀 KQ Alumni Platform - Quick Start Guide

Get your development environment up and running in 5 minutes!

---

## ✅ Prerequisites

- Node.js 18+ and npm
- .NET 8.0 SDK
- SQL Server (LocalDB for development)
- Git

---

## 📦 Step 1: Clone & Install (Already Done!)

Your repository is ready at:
```
/home/user/kq-alumni-platform
```

---

## 🎨 Step 2: Frontend Setup (2 minutes)

```bash
cd /home/user/kq-alumni-platform/kq-alumni-frontend

# .env.local already created for you! ✅
# Just verify the API URL matches your backend port

# Install dependencies
npm install

# Start frontend
npm run dev
```

**Frontend will run at:** http://localhost:3000

---

## 🔧 Step 3: Backend Setup (3 minutes)

### Set User Secrets (Required)

```bash
cd /home/user/kq-alumni-platform/KQAlumni.Backend/src/KQAlumni.API

# Email credentials (required for email features)
dotnet user-secrets set "Email:Username" "KQ.Alumni@kenya-airways.com"
dotnet user-secrets set "Email:Password" "YOUR_PASSWORD"

# JWT secret (required for admin login)
dotnet user-secrets set "JwtSettings:SecretKey" "ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456"
```

### Run Database Migration

```bash
cd /home/user/kq-alumni-platform/KQAlumni.Backend

dotnet ef database update \
  --project src/KQAlumni.Infrastructure \
  --startup-project src/KQAlumni.API
```

### Start Backend

```bash
cd src/KQAlumni.API
dotnet run
```

**Backend will run at:** http://localhost:5295 (or check console for actual port)

---

## 🧪 Step 4: Test with Mock Data

### Enable Mock ERP Mode (for testing without real ERP)

Edit `appsettings.json` in `KQAlumni.API`:

```json
"ErpApi": {
  "EnableMockMode": true
}
```

### Test Users

Use these mock ID numbers for testing:

| ID Number | Staff Number | Name | Result |
|-----------|--------------|------|--------|
| 12345678 | 0012345 | John Doe | ✅ Success |
| 87654321 | 0087654 | Jane Smith | ✅ Success |
| 99999999 | - | - | ❌ Failed (triggers manual review) |

---

## 🎯 Step 5: Test the Registration Flow

1. **Open Frontend:** http://localhost:3000
2. **Click "Register"**
3. **Enter Test ID:** `12345678`
4. **Watch it auto-populate:**
   - Staff Number: `0012345`
   - Full Name: `John Doe`
5. **Complete remaining steps**
6. **Submit registration** ✅

---

## 🔐 Step 6: Access Admin Dashboard

1. **Navigate to:** http://localhost:3000/admin/login
2. **Default credentials:**
   - Username: `admin`
   - Password: `Admin@123`
3. **Change password immediately** at `/admin/settings`

---

## 🧪 Step 7: Test Manual Review Mode

1. **Start new registration**
2. **Enter invalid ID:** `99999999`
3. **Click "Continue with Manual Review"** when it appears
4. **Manually fill in:**
   - Full Name
   - Staff Number (optional)
5. **Complete registration**
6. **Check admin dashboard** - should show "Requires Manual Review" ✅

---

## 📧 Step 8: Test Email Resend

### User Self-Service:
- Navigate to: http://localhost:3000/resend-verification
- Enter your test email
- Click "Resend Verification Email"

### Admin Resend:
- Login to admin dashboard
- Find approved registration
- Click "Resend Verification Email" action

---

## 🔍 Verify Everything Works

### ✅ Checklist

- [ ] Frontend loads at http://localhost:3000
- [ ] Backend responds at http://localhost:5295/health
- [ ] Can register with mock ID (12345678)
- [ ] ID verification auto-populates fields
- [ ] Manual review mode activates (ID: 99999999)
- [ ] Admin login works
- [ ] Admin dashboard shows registrations
- [ ] Password change works
- [ ] Resend verification page accessible

---

## 🎨 URLs Reference

| Feature | URL |
|---------|-----|
| **Frontend** | http://localhost:3000 |
| **Registration** | http://localhost:3000/register |
| **Resend Verification** | http://localhost:3000/resend-verification |
| **Admin Login** | http://localhost:3000/admin/login |
| **Admin Dashboard** | http://localhost:3000/admin/registrations |
| **Backend API** | http://localhost:5295 |
| **Swagger Docs** | http://localhost:5295/swagger |
| **Hangfire Dashboard** | http://localhost:5295/hangfire |

---

## 🐛 Troubleshooting

### "Cannot connect to backend"
```bash
# Check if backend is running
curl http://localhost:5295/health

# Verify NEXT_PUBLIC_API_URL in .env.local matches backend port
cat kq-alumni-frontend/.env.local
```

### "Database migration failed"
```bash
# Check SQL Server is running
# For LocalDB: sqllocaldb info mssqllocaldb

# Try creating database manually
dotnet ef database drop --force
dotnet ef database update
```

### "Email not sending"
```bash
# Check user secrets are set
cd KQAlumni.Backend/src/KQAlumni.API
dotnet user-secrets list

# For testing, email will fail gracefully - registration still works!
```

### "ERP verification always fails"
```bash
# Enable mock mode in appsettings.json
"ErpApi": {
  "EnableMockMode": true
}
```

---

## 🎉 Next Steps

1. **Test all features** with mock data
2. **Configure real ERP** credentials when ready
3. **Configure real email** SMTP settings
4. **Update admin password** from default
5. **Deploy to staging** environment
6. **Run full integration tests**
7. **Go live!** 🚀

---

## 📚 Additional Resources

- **Full Configuration:** See `CONFIGURATION_TEMPLATES.md`
- **Architecture Docs:** See `docs/` folder
- **API Documentation:** http://localhost:5295/swagger (when running)
- **Support:** KQ.Alumni@kenya-airways.com

---

## 🆘 Need Help?

### Common Issues:

**Q: What port is my backend running on?**
A: Check the console when you run `dotnet run`. It will show:
```
Now listening on: http://localhost:5295
```

**Q: Can I use a different database?**
A: Yes! Update `ConnectionStrings:DefaultConnection` in appsettings.json

**Q: How do I reset everything?**
A:
```bash
# Drop database
dotnet ef database drop --force

# Recreate from migrations
dotnet ef database update

# Clear frontend state
# In browser: Application → Local Storage → Clear
```

**Q: Mock emails going nowhere?**
A: That's expected in development. Check logs to see email content.

---

**You're all set!** 🎊

Your KQ Alumni Platform is now running with:
- ✅ ERP Fallback Mode (manual review)
- ✅ Email Verification Resend
- ✅ Password Change API
- ✅ Admin Dashboard
- ✅ Background Job Processing

**Happy coding!** 🚀
