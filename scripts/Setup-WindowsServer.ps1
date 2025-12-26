# IAMS Windows Server Setup Script
# Run as Administrator
# For: elzem.websuresoft.com

#Requires -RunAsAdministrator

param(
    [string]$SqlPassword = "YourSecurePassword123!",
    [string]$Domain = "elzem.websuresoft.com",
    [string]$TenantId = "elzem"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "IAMS On-Premise Setup Script" -ForegroundColor Cyan
Write-Host "Domain: $Domain" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Function to check if running as admin
function Test-Administrator {
    $user = [Security.Principal.WindowsIdentity]::GetCurrent();
    (New-Object Security.Principal.WindowsPrincipal $user).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

if (-not (Test-Administrator)) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

# Step 1: Install IIS
Write-Host "`n[1/8] Installing IIS..." -ForegroundColor Yellow
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature Web-WebSockets
Install-WindowsFeature Web-Asp-Net45

# Step 2: Create directories
Write-Host "`n[2/8] Creating application directories..." -ForegroundColor Yellow
$directories = @(
    "C:\inetpub\iams-api",
    "C:\inetpub\iams-web",
    "C:\Logs\IAMS",
    "D:\Backups\IAMS",
    "C:\Scripts"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force
        Write-Host "Created: $dir" -ForegroundColor Green
    }
}

# Step 3: Configure IIS Application Pools
Write-Host "`n[3/8] Configuring IIS Application Pools..." -ForegroundColor Yellow
Import-Module WebAdministration

# Create API App Pool
if (-not (Test-Path "IIS:\AppPools\IAMS_API_Pool")) {
    New-WebAppPool -Name "IAMS_API_Pool"
    Set-ItemProperty "IIS:\AppPools\IAMS_API_Pool" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty "IIS:\AppPools\IAMS_API_Pool" -Name enable32BitAppOnWin64 -Value $false
    Write-Host "Created: IAMS_API_Pool" -ForegroundColor Green
}

# Create Web App Pool
if (-not (Test-Path "IIS:\AppPools\IAMS_Web_Pool")) {
    New-WebAppPool -Name "IAMS_Web_Pool"
    Set-ItemProperty "IIS:\AppPools\IAMS_Web_Pool" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty "IIS:\AppPools\IAMS_Web_Pool" -Name enable32BitAppOnWin64 -Value $false
    Write-Host "Created: IAMS_Web_Pool" -ForegroundColor Green
}

# Step 4: Create IIS Sites (temporary ports)
Write-Host "`n[4/8] Creating IIS Sites..." -ForegroundColor Yellow

# Stop default site
if (Get-WebSite -Name "Default Web Site") {
    Stop-WebSite -Name "Default Web Site"
}

# Create or update API site
if (Get-WebSite -Name "IAMS_API") {
    Remove-WebSite -Name "IAMS_API"
}
New-WebSite -Name "IAMS_API" `
    -PhysicalPath "C:\inetpub\iams-api" `
    -ApplicationPool "IAMS_API_Pool" `
    -Port 5000

# Create or update Web site
if (Get-WebSite -Name "IAMS_Web") {
    Remove-WebSite -Name "IAMS_Web"
}
New-WebSite -Name "IAMS_Web" `
    -PhysicalPath "C:\inetpub\iams-web" `
    -ApplicationPool "IAMS_Web_Pool" `
    -Port 5001

Write-Host "IIS Sites created" -ForegroundColor Green

# Step 5: Configure Firewall
Write-Host "`n[5/8] Configuring Windows Firewall..." -ForegroundColor Yellow

$firewallRules = @(
    @{Name="IAMS HTTP"; Port=80},
    @{Name="IAMS HTTPS"; Port=443}
)

foreach ($rule in $firewallRules) {
    if (-not (Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $rule.Name `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort $rule.Port `
            -Action Allow
        Write-Host "Firewall rule created: $($rule.Name)" -ForegroundColor Green
    }
}

# Step 6: Set file permissions
Write-Host "`n[6/8] Setting file permissions..." -ForegroundColor Yellow

icacls "C:\inetpub\iams-api" /grant "IIS_IUSRS:(OI)(CI)RX" /T | Out-Null
icacls "C:\inetpub\iams-web" /grant "IIS_IUSRS:(OI)(CI)RX" /T | Out-Null
icacls "C:\Logs\IAMS" /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null

Write-Host "File permissions set" -ForegroundColor Green

# Step 7: Create SQL Server databases (requires SQL Server to be installed)
Write-Host "`n[7/8] Checking SQL Server..." -ForegroundColor Yellow

if (Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue) {
    Write-Host "SQL Server detected" -ForegroundColor Green

    # Create SQL script file
    $sqlScript = @"
-- Create master database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IAMS_Master')
BEGIN
    CREATE DATABASE IAMS_Master;
    PRINT 'IAMS_Master database created';
END
GO

-- Create tenant database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IAMS_Tenant_$TenantId')
BEGIN
    CREATE DATABASE IAMS_Tenant_$TenantId;
    PRINT 'IAMS_Tenant_$TenantId database created';
END
GO

-- Create login
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'iams_app')
BEGIN
    CREATE LOGIN iams_app WITH PASSWORD = '$SqlPassword';
    PRINT 'iams_app login created';
END
GO

-- Grant permissions to master database
USE IAMS_Master;
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'iams_app')
BEGIN
    CREATE USER iams_app FOR LOGIN iams_app;
    ALTER ROLE db_owner ADD MEMBER iams_app;
    PRINT 'Permissions granted on IAMS_Master';
END
GO

-- Grant permissions to tenant database
USE IAMS_Tenant_$TenantId;
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'iams_app')
BEGIN
    CREATE USER iams_app FOR LOGIN iams_app;
    ALTER ROLE db_owner ADD MEMBER iams_app;
    PRINT 'Permissions granted on IAMS_Tenant_$TenantId';
END
GO
"@

    $sqlScript | Out-File -FilePath "C:\Scripts\CreateDatabases.sql" -Encoding UTF8

    Write-Host "Running SQL setup script..." -ForegroundColor Yellow
    try {
        sqlcmd -S localhost -E -i "C:\Scripts\CreateDatabases.sql"
        Write-Host "SQL databases created successfully" -ForegroundColor Green
    } catch {
        Write-Warning "Could not create databases automatically. Please run C:\Scripts\CreateDatabases.sql manually"
    }

} else {
    Write-Warning "SQL Server not detected. Please install SQL Server Express first."
    Write-Host "Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Cyan
}

# Step 8: Create backup script
Write-Host "`n[8/8] Creating backup script..." -ForegroundColor Yellow

$backupScript = @"
# IAMS Automated Backup Script
`$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
`$backupPath = "D:\Backups\IAMS"

# Backup Master Database
`$backupFile = "`$backupPath\IAMS_Master_`$timestamp.bak"
Invoke-Sqlcmd -Query "BACKUP DATABASE [IAMS_Master] TO DISK = N'`$backupFile' WITH COMPRESSION, STATS = 10"

# Backup Tenant Database
`$backupFile = "`$backupPath\IAMS_Tenant_$TenantId_`$timestamp.bak"
Invoke-Sqlcmd -Query "BACKUP DATABASE [IAMS_Tenant_$TenantId] TO DISK = N'`$backupFile' WITH COMPRESSION, STATS = 10"

# Delete backups older than 30 days
Get-ChildItem -Path `$backupPath -Filter "*.bak" |
    Where-Object { `$_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force

Write-Host "Backup completed successfully"
"@

$backupScript | Out-File -FilePath "C:\Scripts\BackupIAMS.ps1" -Encoding UTF8
Write-Host "Backup script created: C:\Scripts\BackupIAMS.ps1" -ForegroundColor Green

# Summary
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Install .NET 8.0 Hosting Bundle from:" -ForegroundColor White
Write-Host "   https://dotnet.microsoft.com/download/dotnet/8.0`n" -ForegroundColor Cyan

Write-Host "2. Copy your published application files to:" -ForegroundColor White
Write-Host "   - C:\inetpub\iams-api" -ForegroundColor Cyan
Write-Host "   - C:\inetpub\iams-web`n" -ForegroundColor Cyan

Write-Host "3. Update configuration in:" -ForegroundColor White
Write-Host "   - C:\inetpub\iams-api\appsettings.Production.json`n" -ForegroundColor Cyan

Write-Host "4. Run database migrations:" -ForegroundColor White
Write-Host "   cd C:\inetpub\iams-api" -ForegroundColor Cyan
Write-Host "   dotnet ef database update`n" -ForegroundColor Cyan

Write-Host "5. Install SSL certificate using win-acme:" -ForegroundColor White
Write-Host "   Download: https://github.com/win-acme/win-acme/releases`n" -ForegroundColor Cyan

Write-Host "6. Test your application:" -ForegroundColor White
Write-Host "   http://localhost:5000/health (API)" -ForegroundColor Cyan
Write-Host "   http://localhost:5001/health (Web)" -ForegroundColor Cyan
Write-Host "   https://$Domain (Production)`n" -ForegroundColor Cyan

Write-Host "SQL Connection String:" -ForegroundColor Yellow
Write-Host "Server=localhost;Database=IAMS_Master;User Id=iams_app;Password=$SqlPassword;TrustServerCertificate=True;" -ForegroundColor Cyan

Write-Host "`nFor full instructions, see: ONPREMISE_DEPLOYMENT.md" -ForegroundColor White
