# Create Continental App Deployment Package
# This script creates a zip file with all necessary files for deployment

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Creating Deployment Package" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Set paths
$ProjectRoot = $PSScriptRoot
$DeployStaging = Join-Path $ProjectRoot "deployment-staging"
$ZipFile = Join-Path $ProjectRoot "continental-app-deployment.zip"

# Clean up existing staging directory
if (Test-Path $DeployStaging) {
    Write-Host "Cleaning up old staging directory..." -ForegroundColor Yellow
    Remove-Item $DeployStaging -Recurse -Force
}

# Create staging structure
Write-Host "Creating staging directory structure..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $DeployStaging | Out-Null
New-Item -ItemType Directory -Force -Path "$DeployStaging\backend" | Out-Null
New-Item -ItemType Directory -Force -Path "$DeployStaging\frontend" | Out-Null
New-Item -ItemType Directory -Force -Path "$DeployStaging\database" | Out-Null

# Copy Backend (excluding bin, obj, and other build artifacts)
Write-Host ""
Write-Host "Copying backend files..." -ForegroundColor Green
$BackendSource = Join-Path $ProjectRoot "FreeTimeApp\tiempo-libre.app"
Get-ChildItem -Path $BackendSource -Recurse | Where-Object {
    $_.FullName -notmatch '\\bin\\' -and
    $_.FullName -notmatch '\\obj\\' -and
    $_.Name -ne '.vs'
} | ForEach-Object {
    $TargetPath = $_.FullName.Replace($BackendSource, "$DeployStaging\backend")
    if ($_.PSIsContainer) {
        New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null
    } else {
        $TargetDir = Split-Path -Parent $TargetPath
        if (-not (Test-Path $TargetDir)) {
            New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null
        }
        Copy-Item $_.FullName -Destination $TargetPath -Force
    }
}
Write-Host "Backend files copied!" -ForegroundColor Green

# Copy Frontend (excluding node_modules and dist)
Write-Host ""
Write-Host "Copying frontend files..." -ForegroundColor Green
$FrontendSource = Join-Path $ProjectRoot "continental-frontend"
Get-ChildItem -Path $FrontendSource -Recurse | Where-Object {
    $_.FullName -notmatch '\\node_modules\\' -and
    $_.FullName -notmatch '\\dist\\' -and
    $_.Name -ne '.vite'
} | ForEach-Object {
    $TargetPath = $_.FullName.Replace($FrontendSource, "$DeployStaging\frontend")
    if ($_.PSIsContainer) {
        New-Item -ItemType Directory -Force -Path $TargetPath | Out-Null
    } else {
        $TargetDir = Split-Path -Parent $TargetPath
        if (-not (Test-Path $TargetDir)) {
            New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null
        }
        Copy-Item $_.FullName -Destination $TargetPath -Force
    }
}
Write-Host "Frontend files copied!" -ForegroundColor Green

# Copy Database Backup
Write-Host ""
Write-Host "Copying database backup..." -ForegroundColor Green
$DbBackup = "C:\SQLBackup\repaldirijillo.bak"
if (Test-Path $DbBackup) {
    Copy-Item $DbBackup -Destination "$DeployStaging\database\repaldirijillo.bak" -Force
    Write-Host "Database backup copied!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Database backup not found at $DbBackup" -ForegroundColor Red
}

# Copy Deployment Script
Write-Host ""
Write-Host "Copying deployment script..." -ForegroundColor Green
Copy-Item "$ProjectRoot\deploy.ps1" -Destination "$DeployStaging\deploy.ps1" -Force
Write-Host "Deployment script copied!" -ForegroundColor Green

# Create README
Write-Host ""
Write-Host "Creating README..." -ForegroundColor Green
$ReadmeContent = @"
# Continental Vacation App - Deployment Package

## Package Contents
- backend/: .NET 9.0 Backend API
- frontend/: React + Vite Frontend
- database/: SQL Server database backup
- deploy.ps1: Automated deployment script

## Prerequisites
- .NET 9.0 SDK
- Node.js 18+ and npm
- SQL Server (Express, Standard, or Enterprise)
- Windows Server or Windows 10/11

## Quick Deployment

1. Extract this zip file to your server
2. Open PowerShell as Administrator
3. Navigate to the extracted directory
4. Run the deployment script:

   .\deploy.ps1

   Optional parameters:
   .\deploy.ps1 -BackendPath "C:\your\path\backend" -FrontendPath "C:\your\path\frontend" -SqlServer "localhost" -SqlUser "sa" -SqlPassword "YourPassword"

## Manual Deployment

### 1. Restore Database
sqlcmd -S localhost -U sa -P "YourPassword" -C -Q "RESTORE DATABASE Vacaciones FROM DISK = 'database\repaldirijillo.bak' WITH REPLACE"

### 2. Deploy Backend
- Copy backend/ folder to your desired location
- Navigate to the backend folder
- Run: dotnet restore
- Run: dotnet run

### 3. Deploy Frontend
- Copy frontend/ folder to your desired location
- Navigate to the frontend folder
- Run: npm install
- Run: npm run dev (development) or npm run build (production)

## Default Credentials
New users imported with default password: Continental2024!

## Configuration
- Backend runs on: http://localhost:5050
- Frontend runs on: http://localhost:5173
- Database connection string is in backend/appsettings.json

## Support
For issues or questions, contact your system administrator.
"@

Set-Content -Path "$DeployStaging\README.txt" -Value $ReadmeContent
Write-Host "README created!" -ForegroundColor Green

# Create Zip File
Write-Host ""
Write-Host "Creating zip file..." -ForegroundColor Yellow

if (Test-Path $ZipFile) {
    Remove-Item $ZipFile -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($DeployStaging, $ZipFile)

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Package Created Successfully!" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Deployment package: $ZipFile" -ForegroundColor Green
Write-Host "Size: $([math]::Round((Get-Item $ZipFile).Length / 1MB, 2)) MB" -ForegroundColor Green
Write-Host ""
Write-Host "To deploy on new server:" -ForegroundColor Yellow
Write-Host "1. Copy continental-app-deployment.zip to the new server" -ForegroundColor White
Write-Host "2. Extract the zip file" -ForegroundColor White
Write-Host "3. Run deploy.ps1 as Administrator" -ForegroundColor White
Write-Host ""

# Clean up staging directory
Write-Host "Cleaning up staging directory..." -ForegroundColor Yellow
Remove-Item $DeployStaging -Recurse -Force

Write-Host "Done!" -ForegroundColor Green
