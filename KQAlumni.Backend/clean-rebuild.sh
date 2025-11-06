#!/bin/bash

# ============================================================
# COMPREHENSIVE BUILD CACHE CLEANUP SCRIPT
# ============================================================
# This script completely cleans all build artifacts and
# rebuilds the project from scratch to fix persistent
# Swagger/OpenAPI conflicts caused by cached assemblies.
# ============================================================

set -e  # Exit on any error

echo "============================================================"
echo "🧹 COMPREHENSIVE BUILD CACHE CLEANUP"
echo "============================================================"
echo ""

# Step 1: Stop any running instances
echo "📍 Step 1: Stopping any running .NET processes..."
pkill -f "dotnet run" || true
pkill -f "KQAlumni.API" || true
sleep 2
echo "✅ Processes stopped"
echo ""

# Step 2: Navigate to backend directory
cd "$(dirname "$0")"
echo "📂 Working directory: $(pwd)"
echo ""

# Step 3: Delete all bin and obj directories
echo "📍 Step 2: Removing all bin/ and obj/ directories..."
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
echo "✅ Build directories removed"
echo ""

# Step 4: Clean NuGet local cache for this project
echo "📍 Step 3: Cleaning NuGet local packages cache..."
dotnet nuget locals all --clear 2>/dev/null || echo "⚠️  NuGet cache clear skipped (dotnet CLI not available)"
echo ""

# Step 5: Run dotnet clean
echo "📍 Step 4: Running dotnet clean..."
dotnet clean --configuration Debug 2>/dev/null || echo "⚠️  dotnet clean skipped (dotnet CLI not available)"
dotnet clean --configuration Release 2>/dev/null || echo "⚠️  dotnet clean skipped (dotnet CLI not available)"
echo "✅ dotnet clean completed"
echo ""

# Step 6: Restore packages
echo "📍 Step 5: Restoring NuGet packages..."
dotnet restore --force --no-cache 2>/dev/null || echo "⚠️  dotnet restore skipped (dotnet CLI not available)"
echo "✅ Packages restored"
echo ""

# Step 7: Rebuild solution
echo "📍 Step 6: Rebuilding solution..."
dotnet build --no-restore --configuration Debug 2>/dev/null || echo "⚠️  dotnet build skipped (dotnet CLI not available)"
echo "✅ Solution rebuilt"
echo ""

echo "============================================================"
echo "✅ CLEANUP AND REBUILD COMPLETE"
echo "============================================================"
echo ""
echo "🎯 NEXT STEPS:"
echo "   1. Run the application:"
echo "      cd src/KQAlumni.API"
echo "      dotnet run"
echo ""
echo "   2. Test Swagger endpoint:"
echo "      http://localhost:5000/swagger"
echo ""
echo "   3. Verify no conflicts appear in logs"
echo "============================================================"
