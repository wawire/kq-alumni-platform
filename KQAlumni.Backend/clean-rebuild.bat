@echo off
REM ============================================================
REM COMPREHENSIVE BUILD CACHE CLEANUP SCRIPT (Windows)
REM ============================================================
REM This script completely cleans all build artifacts and
REM rebuilds the project from scratch to fix persistent
REM Swagger/OpenAPI conflicts caused by cached assemblies.
REM ============================================================

echo ============================================================
echo 🧹 COMPREHENSIVE BUILD CACHE CLEANUP
echo ============================================================
echo.

REM Step 1: Stop any running instances
echo 📍 Step 1: Stopping any running .NET processes...
taskkill /F /IM dotnet.exe 2>NUL || echo No dotnet processes to kill
timeout /t 2 /nobreak >NUL
echo ✅ Processes stopped
echo.

REM Step 2: Navigate to backend directory
cd /d "%~dp0"
echo 📂 Working directory: %CD%
echo.

REM Step 3: Delete all bin and obj directories
echo 📍 Step 2: Removing all bin\ and obj\ directories...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
echo ✅ Build directories removed
echo.

REM Step 4: Clean NuGet local cache
echo 📍 Step 3: Cleaning NuGet local packages cache...
dotnet nuget locals all --clear
echo.

REM Step 5: Run dotnet clean
echo 📍 Step 4: Running dotnet clean...
dotnet clean --configuration Debug
dotnet clean --configuration Release
echo ✅ dotnet clean completed
echo.

REM Step 6: Restore packages
echo 📍 Step 5: Restoring NuGet packages...
dotnet restore --force --no-cache
echo ✅ Packages restored
echo.

REM Step 7: Rebuild solution
echo 📍 Step 6: Rebuilding solution...
dotnet build --no-restore --configuration Debug
echo ✅ Solution rebuilt
echo.

echo ============================================================
echo ✅ CLEANUP AND REBUILD COMPLETE
echo ============================================================
echo.
echo 🎯 NEXT STEPS:
echo    1. Run the application:
echo       cd src\KQAlumni.API
echo       dotnet run
echo.
echo    2. Test Swagger endpoint:
echo       http://localhost:5000/swagger
echo.
echo    3. Verify no conflicts appear in logs
echo ============================================================
pause
