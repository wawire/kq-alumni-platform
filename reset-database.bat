@echo off
REM =====================================================
REM DATABASE RESET SCRIPT (Windows)
REM =====================================================
REM This script completely resets the database with clean migrations
REM Run from the project root directory
REM =====================================================

echo ==========================================
echo KQ ALUMNI DATABASE RESET
echo ==========================================
echo.

echo Step 0: Verifying migration fix...
echo -----------------------------------
findstr /C:"IF EXISTS" KQAlumni.Backend\src\KQAlumni.Infrastructure\Data\Migrations\20251108000000_AddUniqueConstraintIdNumber.cs >nul 2>&1
if errorlevel 1 (
    echo ERROR: Migration file not updated!
    echo.
    echo You need to pull the latest changes first:
    echo   git pull origin claude/test-registration-workflow-011CUw4YBfpRQtkGTLg7Nh8s
    echo.
    echo The migration file should contain "IF EXISTS" checks.
    echo Current file does not have the fix.
    pause
    exit /b 1
)
echo - Migration fix verified
echo.

cd KQAlumni.Backend\src\KQAlumni.API

echo Step 1: Cleaning build artifacts...
echo -----------------------------------
for /d /r ..\.. %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
echo - Build artifacts cleaned
echo.

echo Step 2: Rebuilding solution...
echo -----------------------------------
dotnet build
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo - Solution rebuilt with fresh migrations
echo.

echo Step 3: Dropping existing database...
echo -----------------------------------
dotnet ef database drop --project ..\KQAlumni.Infrastructure --force
if errorlevel 1 (
    echo ERROR: Database drop failed!
    pause
    exit /b 1
)
echo - Database dropped
echo.

echo Step 4: Applying all migrations...
echo -----------------------------------
dotnet ef database update --project ..\KQAlumni.Infrastructure
if errorlevel 1 (
    echo ERROR: Migration failed!
    pause
    exit /b 1
)
echo - All migrations applied successfully
echo.

echo Step 5: Listing applied migrations...
echo -----------------------------------
dotnet ef migrations list --project ..\KQAlumni.Infrastructure
echo.

echo ==========================================
echo - DATABASE RESET COMPLETE!
echo ==========================================
echo.
echo You can now run the application:
echo   cd KQAlumni.Backend\src\KQAlumni.API
echo   dotnet run
echo.
pause
