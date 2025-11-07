@echo off
REM Clean and rebuild script for Windows Command Prompt
REM Run this to fix routing conflicts from deleted controllers

echo Cleaning KQ Alumni Backend...
echo.

REM Navigate to backend directory
cd /d "%~dp0"

REM Clean solution
echo Cleaning solution...
dotnet clean --verbosity minimal

REM Remove all bin and obj folders
echo.
echo Removing bin and obj folders...
for /d /r . %%d in (bin,obj) do @if exist "%%d" (
    echo   Removing: %%d
    rd /s /q "%%d" 2>nul
)

REM Rebuild solution
echo.
echo Rebuilding solution...
dotnet build --no-incremental --verbosity minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Clean rebuild completed successfully!
    echo.
    echo You can now run the application with:
    echo   cd src\KQAlumni.API
    echo   dotnet run
) else (
    echo.
    echo [ERROR] Build failed! Check the errors above.
    exit /b 1
)
