#!/bin/bash

###############################################################################
# Admin User Seeding Script for KQ Alumni Platform
#
# This script provides multiple ways to create the initial SuperAdmin user:
# 1. Via API endpoint (Development only)
# 2. Via direct database command (Production)
#
# Usage:
#   ./seed-admin-user.sh [environment]
#
#   environment: dev (default) | prod
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
ENVIRONMENT="${1:-dev}"
API_URL="${API_URL:-http://localhost:5000}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}KQ Alumni Platform - Admin User Seeder${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

if [ "$ENVIRONMENT" = "dev" ]; then
    echo -e "${YELLOW}Environment: Development${NC}"
    echo -e "${YELLOW}API URL: $API_URL${NC}"
    echo ""
    echo "Creating initial SuperAdmin user via API endpoint..."
    echo ""

    # Call the seeding endpoint
    RESPONSE=$(curl -s -X POST "$API_URL/api/v1/admin/seed-initial-admin" \
        -H "Content-Type: application/json" \
        -w "\nHTTP_STATUS:%{http_code}")

    HTTP_STATUS=$(echo "$RESPONSE" | grep "HTTP_STATUS" | cut -d':' -f2)
    BODY=$(echo "$RESPONSE" | sed '/HTTP_STATUS/d')

    if [ "$HTTP_STATUS" = "201" ]; then
        echo -e "${GREEN}✅ SUCCESS: Initial SuperAdmin user created!${NC}"
        echo ""
        echo -e "${YELLOW}Credentials:${NC}"
        echo "  Username: admin"
        echo "  Password: Admin@123456"
        echo "  Email: admin@kenya-airways.com"
        echo ""
        echo -e "${RED}⚠️  IMPORTANT: Change this password immediately after first login!${NC}"
        echo ""
        echo "Next steps:"
        echo "  1. Login at: $API_URL/api/v1/admin/login"
        echo "  2. Create additional admin users"
        echo "  3. Change the default password"
        echo ""
    elif [ "$HTTP_STATUS" = "400" ]; then
        echo -e "${YELLOW}⚠️  Admin user already exists. No action needed.${NC}"
        echo ""
    else
        echo -e "${RED}❌ ERROR: Failed to create admin user (HTTP $HTTP_STATUS)${NC}"
        echo "$BODY"
        exit 1
    fi

elif [ "$ENVIRONMENT" = "prod" ]; then
    echo -e "${YELLOW}Environment: Production${NC}"
    echo ""
    echo -e "${RED}⚠️  WARNING: Production environment detected${NC}"
    echo ""
    echo "For production, you must manually create the admin user using:"
    echo ""
    echo "Option 1: Using .NET CLI tool (recommended)"
    echo "  cd KQAlumni.Backend/src/KQAlumni.API"
    echo "  dotnet run -- seed-admin --username=admin --email=admin@kenya-airways.com --password=YourSecurePassword123"
    echo ""
    echo "Option 2: Using SQL Server Management Studio"
    echo "  1. Connect to your SQL Server database"
    echo "  2. Run the following SQL script:"
    echo ""
    cat << 'SQL'
-- Generate password hash using BCrypt (work factor: 12)
-- You must use BCrypt library to hash the password first
-- Example in C#: BCrypt.Net.BCrypt.HashPassword("YourPassword", 12)

DECLARE @PasswordHash NVARCHAR(255) = 'YOUR_BCRYPT_HASHED_PASSWORD_HERE';

INSERT INTO AdminUsers (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
VALUES (
    'admin',                          -- Username
    'admin@kenya-airways.com',        -- Email
    @PasswordHash,                    -- Password hash from BCrypt
    'System Administrator',           -- Full name
    'SuperAdmin',                     -- Role
    1,                                -- IsActive
    GETUTCDATE()                      -- CreatedAt
);

SELECT * FROM AdminUsers WHERE Username = 'admin';
SQL
    echo ""
    echo -e "${YELLOW}Note: You cannot use the API seeding endpoint in Production.${NC}"
    echo ""

else
    echo -e "${RED}Invalid environment: $ENVIRONMENT${NC}"
    echo "Usage: $0 [dev|prod]"
    exit 1
fi

echo -e "${GREEN}========================================${NC}"
