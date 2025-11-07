# Clean and rebuild script to remove cached assemblies
# Run this script from PowerShell to fix routing conflicts from deleted controllers

Write-Host "Cleaning KQ Alumni Backend..." -ForegroundColor Cyan

# Stop any running processes that might lock files
Write-Host "`nStopping any running dotnet processes..."
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Navigate to backend directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Clean solution
Write-Host "`nCleaning solution..." -ForegroundColor Yellow
dotnet clean --verbosity minimal

# Remove all bin and obj folders
Write-Host "`nRemoving bin and obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | ForEach-Object {
    Write-Host "  Removing: $($_.FullName)" -ForegroundColor Gray
    Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

# Clear NuGet cache for this solution only (optional but thorough)
Write-Host "`nClearing local NuGet packages cache..."
if (Test-Path ".nuget") {
    Remove-Item ".nuget" -Recurse -Force -ErrorAction SilentlyContinue
}

# Rebuild solution
Write-Host "`nRebuilding solution..." -ForegroundColor Yellow
dotnet build --no-incremental --verbosity minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✓ Clean rebuild completed successfully!" -ForegroundColor Green
    Write-Host "`nYou can now run the application with:" -ForegroundColor Cyan
    Write-Host "  cd src\KQAlumni.API" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
} else {
    Write-Host "`n✗ Build failed! Check the errors above." -ForegroundColor Red
    exit 1
}
