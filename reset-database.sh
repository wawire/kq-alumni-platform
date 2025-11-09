#!/bin/bash

# =====================================================
# DATABASE RESET SCRIPT
# =====================================================
# This script completely resets the database with clean migrations
# Run from the project root directory
# =====================================================

set -e  # Exit on any error

echo "=========================================="
echo "KQ ALUMNI DATABASE RESET"
echo "=========================================="
echo ""

# Navigate to API project
cd KQAlumni.Backend/src/KQAlumni.API

echo "Step 1: Cleaning build artifacts..."
echo "-----------------------------------"
# Remove ALL bin and obj folders recursively
find ../.. -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find ../.. -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
echo "✓ Build artifacts cleaned"
echo ""

echo "Step 2: Rebuilding solution..."
echo "-----------------------------------"
dotnet build
echo "✓ Solution rebuilt with fresh migrations"
echo ""

echo "Step 3: Dropping existing database..."
echo "-----------------------------------"
dotnet ef database drop --project ../KQAlumni.Infrastructure --force
echo "✓ Database dropped"
echo ""

echo "Step 4: Applying all migrations..."
echo "-----------------------------------"
dotnet ef database update --project ../KQAlumni.Infrastructure
echo "✓ All migrations applied successfully"
echo ""

echo "Step 5: Listing applied migrations..."
echo "-----------------------------------"
dotnet ef migrations list --project ../KQAlumni.Infrastructure
echo ""

echo "=========================================="
echo "✓ DATABASE RESET COMPLETE!"
echo "=========================================="
echo ""
echo "You can now run the application:"
echo "  cd KQAlumni.Backend/src/KQAlumni.API"
echo "  dotnet run"
echo ""
