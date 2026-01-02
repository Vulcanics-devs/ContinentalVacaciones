# Continental Vacation App Deployment Script
# This script deploys the application to a new server

param(
    [string]$BackendPath = "C:\continental-app\backend",
    [string]$FrontendPath = "C:\continental-app\frontend",
    [string]$SqlServer = "localhost",
    [string]$SqlUser = "sa",
    [string]$SqlPassword = "YourStrong@Passw0rd"
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Continental Vacation App Deployment" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Create deployment directories
Write-Host "Creating deployment directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $BackendPath | Out-Null
New-Item -ItemType Directory -Force -Path $FrontendPath | Out-Null

# Get the script directory
$DeployDir = $PSScriptRoot

# Step 1: Restore Database
Write-Host ""
Write-Host "Step 1: Restoring database..." -ForegroundColor Green
$BackupFile = Join-Path $DeployDir "database\repaldirijillo.bak"

if (-not (Test-Path $BackupFile)) {
    Write-Host "ERROR: Database backup file not found at: $BackupFile" -ForegroundColor Red
    exit 1
}

# Copy backup to temp location
$TempBackup = "C:\SQLBackup\repaldirijillo.bak"
New-Item -ItemType Directory -Force -Path "C:\SQLBackup" | Out-Null
Copy-Item $BackupFile $TempBackup -Force

# Restore database
$SqlCmd = @"
USE master;
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Vacaciones')
BEGIN
    ALTER DATABASE Vacaciones SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Vacaciones;
END
RESTORE DATABASE Vacaciones FROM DISK = '$TempBackup' WITH REPLACE;
ALTER DATABASE Vacaciones SET MULTI_USER;
"@

sqlcmd -S $SqlServer -U $SqlUser -P $SqlPassword -C -Q $SqlCmd

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database restored successfully!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Database restoration failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Deploy Backend
Write-Host ""
Write-Host "Step 2: Deploying backend..." -ForegroundColor Green
$BackendSource = Join-Path $DeployDir "backend"
Copy-Item "$BackendSource\*" -Destination $BackendPath -Recurse -Force
Write-Host "Backend deployed to: $BackendPath" -ForegroundColor Green

# Step 3: Deploy Frontend
Write-Host ""
Write-Host "Step 3: Deploying frontend..." -ForegroundColor Green
$FrontendSource = Join-Path $DeployDir "frontend"
Copy-Item "$FrontendSource\*" -Destination $FrontendPath -Recurse -Force
Write-Host "Frontend deployed to: $FrontendPath" -ForegroundColor Green

# Step 4: Install Frontend Dependencies
Write-Host ""
Write-Host "Step 4: Installing frontend dependencies..." -ForegroundColor Green
Push-Location $FrontendPath
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host "Frontend dependencies installed successfully!" -ForegroundColor Green
} else {
    Write-Host "WARNING: npm install had issues. You may need to run it manually." -ForegroundColor Yellow
}
Pop-Location

# Step 5: Final Instructions
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To start the application:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Start Backend:" -ForegroundColor White
Write-Host "   cd $BackendPath" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Start Frontend (in a new terminal):" -ForegroundColor White
Write-Host "   cd $FrontendPath" -ForegroundColor Gray
Write-Host "   npm run dev" -ForegroundColor Gray
Write-Host ""
Write-Host "Default credentials for new users:" -ForegroundColor Yellow
Write-Host "   Password: Continental2024!" -ForegroundColor Gray
Write-Host ""
Write-Host "Backend will run on: http://localhost:5050" -ForegroundColor Cyan
Write-Host "Frontend will run on: http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
