# SECURITY NOTICE - EXPOSED CREDENTIALS

## CRITICAL: Passwords Exposed in Git History

The following credentials were accidentally committed to the Git repository and **MUST BE ROTATED IMMEDIATELY**:

### 1. Office 365 Email Password
- **Email Account**: `KQ.Alumni@kenya-airways.com`
- **Exposed Password**: `Syntonin&carses@` (in appsettings.json and appsettings.Production.json)
- **Status**: EXPOSED - Must be changed
- **Action Required**:
  1. Change this password in Office 365 admin portal
  2. Update configuration using environment variables (see below)
  3. Never commit the new password to git

### 2. JWT Secret Keys
- **Development Key**: Placeholder text (acceptable for dev)
- **Production Key**: Placeholder text in appsettings.Production.json
- **Action Required**: Generate strong 64+ character secret key for production

## Immediate Actions Required

### Step 1: Rotate Office 365 Password
```bash
# Contact IT/Email administrator to change password for:
# KQ.Alumni@kenya-airways.com
```

### Step 2: Generate Strong JWT Secret
```bash
# Generate a secure random key (64+ characters):
openssl rand -base64 64 | tr -d '\n'
```

### Step 3: Configure Environment Variables

**For Windows (Production Server)**:
```powershell
# Set Email Password
[System.Environment]::SetEnvironmentVariable("EMAIL_PASSWORD", "YOUR_NEW_PASSWORD", "Machine")

# Set JWT Secret
[System.Environment]::SetEnvironmentVariable("JWT_SECRET_KEY", "YOUR_GENERATED_SECRET", "Machine")

# Set SQL Password
[System.Environment]::SetEnvironmentVariable("SQL_PASSWORD", "YOUR_SQL_PASSWORD", "Machine")

# Restart IIS to apply
iisreset
```

**For Linux/Docker**:
```bash
export EMAIL_PASSWORD="YOUR_NEW_PASSWORD"
export JWT_SECRET_KEY="YOUR_GENERATED_SECRET"
export SQL_PASSWORD="YOUR_SQL_PASSWORD"
```

### Step 4: Update Configuration Files
Replace sensitive values with environment variable references:
```json
{
  "Email": {
    "Password": "${EMAIL_PASSWORD}"
  },
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=10.2.150.23;Database=KQAlumniDB;User Id=kqalumni_user;Password=${SQL_PASSWORD};..."
  }
}
```

## Azure Key Vault (Recommended for Production)

For enhanced security, use Azure Key Vault:

```bash
# Install Azure CLI
az login

# Create Key Vault
az keyvault create --name kq-alumni-vault --resource-group kq-alumni-rg --location eastus

# Store secrets
az keyvault secret set --vault-name kq-alumni-vault --name EmailPassword --value "YOUR_NEW_PASSWORD"
az keyvault secret set --vault-name kq-alumni-vault --name JwtSecretKey --value "YOUR_GENERATED_SECRET"
az keyvault secret set --vault-name kq-alumni-vault --name SqlPassword --value "YOUR_SQL_PASSWORD"
```

Update `Program.cs` to read from Key Vault:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://kq-alumni-vault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Git History Cleanup (Optional but Recommended)

Since passwords are in git history, consider:

1. **BFG Repo-Cleaner** - Remove sensitive data from history
2. **Force push** - After cleanup (coordinate with team)
3. **Rotate ALL credentials** - Even after cleanup

```bash
# Install BFG Repo-Cleaner
# https://rtyley.github.io/bfg-repo-cleaner/

# Remove passwords from history
bfg --replace-text passwords.txt kq-alumni-platform.git

# Force push (DANGEROUS - coordinate with team)
git push --force
```

## Verification Checklist

- [ ] Office 365 password rotated
- [ ] New password stored in environment variables or Key Vault
- [ ] JWT secret key generated and configured
- [ ] SQL password configured via environment variables
- [ ] appsettings.json contains NO real passwords (only placeholders)
- [ ] appsettings.Production.json contains NO real passwords
- [ ] Application tested with new credentials
- [ ] All team members notified of changes
- [ ] Git history cleaned (optional)

## Contact

For questions or assistance with credential rotation:
- **IT Security**: [contact information]
- **DevOps Team**: [contact information]

---
**Created**: 2025-11-06
**Status**: URGENT - Action Required
**Priority**: CRITICAL
