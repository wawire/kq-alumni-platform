# Configuration Files Guide

## Overview

This project uses ASP.NET Core's hierarchical configuration system. Configuration files are loaded in this order:

```
appsettings.json                    (Base - Always loaded)
    ↓
appsettings.{Environment}.json      (Environment-specific overrides)
    ↓
Environment Variables               (Highest priority)
```

## File Structure

### ✅ appsettings.json
**Purpose:** Base configuration shared across all environments
**Status:** ✅ Committed to git
**Contains:**
- Default/safe values
- Structure and schema
- Non-sensitive settings
- Development-friendly defaults (EnableMockMode: false, UseMockEmailService: true)

### ✅ appsettings.Development.json
**Purpose:** Development environment overrides
**Status:** ✅ Committed to git (safe for team sharing)
**Contains:**
- EnableMockMode: true
- Local database connection
- Mock email service
- Test staff numbers
- Relaxed rate limiting

**This file overrides specific settings from appsettings.json for local development.**

### ❌ appsettings.Production.json
**Purpose:** Production environment configuration
**Status:** ❌ NOT committed to git (.gitignored)
**How to create:**
```bash
# On production server, copy the template
cp appsettings.Production.template.json appsettings.Production.json

# Edit with production values
nano appsettings.Production.json
```

**Contains:**
- Real ERP connection (EnableMockMode: false)
- Production database credentials
- Real SMTP credentials
- Production secrets and API keys

### ✅ appsettings.Production.template.json
**Purpose:** Template/example for production configuration
**Status:** ✅ Committed to git
**Contains:** Placeholder values showing what needs to be configured

---

## Best Practices

### ✅ DO:
- Commit `appsettings.json` (base config)
- Commit `appsettings.Development.json` (safe dev settings)
- Commit `appsettings.Production.template.json` (template only)
- Use environment variables for secrets in production
- Only override settings that differ from base

### ❌ DON'T:
- Commit `appsettings.Production.json` (has production secrets)
- Put secrets in `appsettings.json`
- Duplicate entire config in environment files (only override what's different)
- Hardcode production credentials

---

## How It Works

### Development (Default)
When you run `dotnet run`, ASP.NET Core loads:
1. `appsettings.json` (base)
2. `appsettings.Development.json` (overrides from Development file)

**Result:** Mock mode enabled, local database, test data

### Production
Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`

ASP.NET Core loads:
1. `appsettings.json` (base)
2. `appsettings.Production.json` (overrides from Production file)

**Result:** Real ERP, production database, real email

---

## Quick Reference

| Setting | Base (appsettings.json) | Development Override | Production Override |
|---------|------------------------|---------------------|-------------------|
| ErpApi.EnableMockMode | `false` | `true` ✅ | `false` |
| Email.UseMockEmailService | `true` | `true` | `false` ✅ |
| ConnectionStrings | LocalDB | Your local DB ✅ | Production DB ✅ |
| RateLimiting.RequestsPerHour | `100` | `1000` ✅ | `50` ✅ |

---

## Example: Adding a New Setting

### 1. Add to base config (appsettings.json)
```json
{
  "MyNewFeature": {
    "Enabled": false,
    "Timeout": 30
  }
}
```

### 2. Override in Development (appsettings.Development.json)
```json
{
  "MyNewFeature": {
    "Enabled": true
  }
}
```

### 3. Override in Production template (appsettings.Production.template.json)
```json
{
  "MyNewFeature": {
    "Enabled": true,
    "Timeout": 60
  }
}
```

**Note:** Only include the settings that differ from the base!

---

## Troubleshooting

### "Mock mode not working"
✅ Check: `appsettings.Development.json` has `"EnableMockMode": true`
✅ Verify: `ASPNETCORE_ENVIRONMENT` is set to `Development` (or not set at all)

### "Can't connect to database"
✅ Check: Connection string in `appsettings.Development.json`
✅ Verify: SQL Server is running and accessible

### "Production secrets exposed"
❌ Make sure `appsettings.Production.json` is in `.gitignore`
❌ Never commit files with real passwords/API keys

---

## Environment Variables (Advanced)

You can override ANY setting using environment variables:

```bash
# Override database connection
export ConnectionStrings__DefaultConnection="Server=prod;Database=KQAlumniDB;..."

# Override ERP mock mode
export ErpApi__EnableMockMode="false"

# Override email password
export Email__Password="secure-password-here"
```

**Format:** `Section__Setting__NestedSetting`
**Priority:** Environment variables > appsettings.{Environment}.json > appsettings.json
