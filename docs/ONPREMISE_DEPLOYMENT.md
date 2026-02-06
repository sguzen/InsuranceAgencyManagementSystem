# On-Premise Deployment Guide
## Physical Server Setup for elzem.websuresoft.com

---

## Prerequisites Checklist

### Server Requirements (Minimum)
- [ ] Windows Server 2019 or 2022 (recommended) OR Ubuntu Server 22.04 LTS
- [ ] 4 CPU cores (8 cores recommended)
- [ ] 16GB RAM minimum (32GB recommended)
- [ ] 100GB SSD storage (500GB recommended)
- [ ] Static IP address
- [ ] Internet connectivity with open ports 80, 443

### Domain Setup
- [ ] Domain: elzem.websuresoft.com
- [ ] DNS A record pointing to server IP
- [ ] SSL certificate (Let's Encrypt or purchased)

---

## Deployment Options

### Option A: Windows Server (Recommended for SQL Server)
### Option B: Linux Server (Lower cost, Docker-based)

---

# OPTION A: Windows Server Deployment

## Phase 1: Server Setup (Day 1 - Morning)

### Step 1: Install Required Software

**1.1 Install .NET 8.0 Runtime**
```powershell
# Download and install .NET 8.0 Hosting Bundle
# URL: https://dotnet.microsoft.com/download/dotnet/8.0

# Verify installation
dotnet --list-runtimes
```

**1.2 Install IIS**
```powershell
# Open PowerShell as Administrator
Install-WindowsFeature -name Web-Server -IncludeManagementTools

# Install additional IIS features
Install-WindowsFeature Web-WebSockets
Install-WindowsFeature Web-Asp-Net45
```

**1.3 Install URL Rewrite Module** (for HTTPS redirect)
```powershell
# Download from: https://www.iis.net/downloads/microsoft/url-rewrite
# Install the MSI package
```

**1.4 Install SQL Server**

**Option 1: SQL Server Express (FREE, up to 10GB database)**
```powershell
# Download SQL Server 2022 Express
# URL: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# During installation:
# - Choose "Custom" installation
# - Select "Database Engine Services"
# - Enable "Mixed Mode" authentication
# - Set SA password (write it down!)
# - Add current user as administrator
```

**Option 2: SQL Server Developer Edition (FREE for dev/test)**
- Same as Express but no size limits
- Not licensed for production (but widely used for small deployments)

**1.5 Install SQL Server Management Studio (SSMS)**
```powershell
# Download from: https://aka.ms/ssmsfullsetup
# Install and launch to verify SQL Server connection
```

---

## Phase 2: Database Setup (Day 1 - Afternoon)

### Step 2: Create Databases

**2.1 Open SSMS and connect to localhost**

**2.2 Create Master Database**
```sql
-- Create master database
CREATE DATABASE IAMS_Master;
GO

-- Create login for application
CREATE LOGIN iams_app WITH PASSWORD = 'YourSecurePassword123!';
GO

-- Create user in master database
USE IAMS_Master;
CREATE USER iams_app FOR LOGIN iams_app;
ALTER ROLE db_owner ADD MEMBER iams_app;
GO
```

**2.3 Create Tenant Database**
```sql
-- Create tenant database
CREATE DATABASE IAMS_Tenant_elzem;
GO

-- Create user in tenant database
USE IAMS_Tenant_elzem;
CREATE USER iams_app FOR LOGIN iams_app;
ALTER ROLE db_owner ADD MEMBER iams_app;
GO
```

**2.4 Enable TCP/IP for SQL Server**
```powershell
# Open SQL Server Configuration Manager
# Navigate to: SQL Server Network Configuration > Protocols
# Enable TCP/IP
# Restart SQL Server service

# Restart SQL Server
Restart-Service -Name "MSSQLSERVER"
```

**2.5 Test Connection**
```powershell
# Test from command line
sqlcmd -S localhost -U iams_app -P "YourSecurePassword123!"
# Should connect successfully
```

---

## Phase 3: Application Deployment (Day 1 - Evening)

### Step 3: Build and Publish Application

**On your development machine:**

```bash
cd /path/to/InsuranceAgencyManagementSystem

# Publish API
dotnet publish src/IAMS.Api/IAMS.Api.csproj \
  -c Release \
  -o ./publish/api \
  /p:EnvironmentName=Production

# Publish Web
dotnet publish src/IAMS.Web/IAMS.Web.csproj \
  -c Release \
  -o ./publish/web \
  /p:EnvironmentName=Production
```

**Transfer to server:**
```powershell
# On server, create directories
New-Item -ItemType Directory -Path "C:\inetpub\iams-api"
New-Item -ItemType Directory -Path "C:\inetpub\iams-web"

# Copy published files to server
# Use Remote Desktop, WinSCP, or robocopy
```

---

### Step 4: Configure Application

**4.1 Update appsettings.Production.json**

Create `C:\inetpub\iams-api\appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "MasterConnection": "Server=localhost;Database=IAMS_Master;User Id=iams_app;Password=YourSecurePassword123!;TrustServerCertificate=True;",
    "DefaultConnection": "Server=localhost;Database=IAMS_Tenant_elzem;User Id=iams_app;Password=YourSecurePassword123!;TrustServerCertificate=True;"
  },
  "TenantConnections": {
    "elzem": "Server=localhost;Database=IAMS_Tenant_elzem;User Id=iams_app;Password=YourSecurePassword123!;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "YourJwtSecretKeyMustBeAtLeast32CharactersLongForSecurity!",
    "Issuer": "https://elzem.websuresoft.com",
    "Audience": "https://elzem.websuresoft.com",
    "ExpiryInMinutes": 60
  },
  "ApiSettings": {
    "BaseUrl": "https://elzem.websuresoft.com/api",
    "EnableSwagger": false
  },
  "MultiTenancy": {
    "ResolutionStrategy": "Header",
    "DefaultTenantId": "elzem"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\Logs\\IAMS\\log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

**4.2 Create Logs Directory**
```powershell
New-Item -ItemType Directory -Path "C:\Logs\IAMS"
# Grant IIS_IUSRS write permissions
icacls "C:\Logs\IAMS" /grant "IIS_IUSRS:(OI)(CI)M"
```

---

### Step 5: Configure IIS

**5.1 Create Application Pools**
```powershell
# Import IIS module
Import-Module WebAdministration

# Create API App Pool
New-WebAppPool -Name "IAMS_API_Pool"
Set-ItemProperty IIS:\AppPools\IAMS_API_Pool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\IAMS_API_Pool -Name enable32BitAppOnWin64 -Value $false

# Create Web App Pool
New-WebAppPool -Name "IAMS_Web_Pool"
Set-ItemProperty IIS:\AppPools\IAMS_Web_Pool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\IAMS_Web_Pool -Name enable32BitAppOnWin64 -Value $false
```

**5.2 Create IIS Sites**
```powershell
# Create API site
New-WebSite -Name "IAMS_API" `
  -PhysicalPath "C:\inetpub\iams-api" `
  -ApplicationPool "IAMS_API_Pool" `
  -Port 5000

# Create Web site
New-WebSite -Name "IAMS_Web" `
  -PhysicalPath "C:\inetpub\iams-web" `
  -ApplicationPool "IAMS_Web_Pool" `
  -Port 5001
```

**5.3 Test Local Access**
```powershell
# Test API
curl http://localhost:5000/health

# Test Web
curl http://localhost:5001/health
```

---

## Phase 4: SSL & Domain Setup (Day 2 - Morning)

### Step 6: Install SSL Certificate

**Option A: Let's Encrypt (FREE) - Recommended**

**6.1 Install win-acme**
```powershell
# Download win-acme
# URL: https://github.com/win-acme/win-acme/releases

# Extract to C:\win-acme

# Run win-acme
cd C:\win-acme
.\wacs.exe
```

**6.2 Configure SSL in win-acme**
```
1. Choose: N - Create certificate with advanced options
2. Choose: 2 - Manual input
3. Enter host: elzem.websuresoft.com
4. Choose validation: 1 - HTTP validation
5. Choose store: 1 - Default certificate store
6. Choose installation: 2 - IIS Web Site
7. Select sites: IAMS_API and IAMS_Web
8. Accept all defaults
```

**Option B: Purchased SSL Certificate**
```powershell
# Import certificate
Import-PfxCertificate -FilePath "C:\path\to\cert.pfx" `
  -CertStoreLocation Cert:\LocalMachine\My `
  -Password (ConvertTo-SecureString -String "certpassword" -AsPlainText -Force)

# Bind to IIS site
New-WebBinding -Name "IAMS_API" -Protocol https -Port 443
```

---

### Step 7: Configure Reverse Proxy (Single Domain)

Since you want everything on elzem.websuresoft.com, we'll use URL Rewrite.

**7.1 Stop default sites**
```powershell
Stop-WebSite -Name "Default Web Site"
Stop-WebSite -Name "IAMS_API"
Stop-WebSite -Name "IAMS_Web"
```

**7.2 Create main site on port 443**
```powershell
New-WebSite -Name "IAMS_Main" `
  -PhysicalPath "C:\inetpub\iams-web" `
  -ApplicationPool "IAMS_Web_Pool" `
  -Port 443 `
  -Protocol https `
  -HostHeader "elzem.websuresoft.com"

# Also bind port 80 for HTTP redirect
New-WebBinding -Name "IAMS_Main" -Protocol http -Port 80 -HostHeader "elzem.websuresoft.com"
```

**7.3 Create API as Virtual Directory**
```powershell
New-WebVirtualDirectory -Site "IAMS_Main" `
  -Name "api" `
  -PhysicalPath "C:\inetpub\iams-api"
```

**7.4 Configure URL Rewrite for API**

Create `C:\inetpub\iams-web\web.config`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Redirect HTTP to HTTPS -->
        <rule name="HTTP to HTTPS" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="off" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## Phase 5: Database Migration (Day 2 - Afternoon)

### Step 8: Run EF Core Migrations

**On the server:**

```powershell
cd C:\inetpub\iams-api

# Install EF Core tools if not present
dotnet tool install --global dotnet-ef

# Set connection string
$env:ConnectionStrings__MasterConnection = "Server=localhost;Database=IAMS_Master;User Id=iams_app;Password=YourSecurePassword123!;TrustServerCertificate=True;"

# Run migrations for master database
dotnet ef database update --project IAMS.Persistence.dll --startup-project IAMS.Api.dll

# Run migrations for tenant database
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=IAMS_Tenant_elzem;User Id=iams_app;Password=YourSecurePassword123!;TrustServerCertificate=True;"
dotnet ef database update --project IAMS.Persistence.dll --startup-project IAMS.Api.dll
```

---

### Step 9: Create Initial Tenant and User

**9.1 Insert Tenant Record**
```sql
USE IAMS_Master;

INSERT INTO Tenants (TenantId, Name, DatabaseName, IsActive, CreatedAt)
VALUES ('elzem', 'Elzem Insurance Agency', 'IAMS_Tenant_elzem', 1, GETUTCDATE());
GO
```

**9.2 Create Admin User** (using API or direct SQL)

Option: Direct SQL (temporary for setup):
```sql
USE IAMS_Tenant_elzem;

-- This is a placeholder - adjust based on your actual schema
-- You may need to use your application's user creation API instead
```

---

## Phase 6: Security Hardening (Day 2 - Evening)

### Step 10: Firewall Configuration

```powershell
# Allow HTTP
New-NetFirewallRule -DisplayName "IAMS HTTP" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 80 `
  -Action Allow

# Allow HTTPS
New-NetFirewallRule -DisplayName "IAMS HTTPS" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 443 `
  -Action Allow

# Block direct access to application ports
New-NetFirewallRule -DisplayName "Block IAMS Ports" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 5000,5001 `
  -Action Block
```

### Step 11: File Permissions

```powershell
# Application files - read-only for IIS
icacls "C:\inetpub\iams-api" /grant "IIS_IUSRS:(OI)(CI)RX"
icacls "C:\inetpub\iams-web" /grant "IIS_IUSRS:(OI)(CI)RX"

# Logs directory - write access
icacls "C:\Logs\IAMS" /grant "IIS_IUSRS:(OI)(CI)M"

# Remove inherited permissions from sensitive files
icacls "C:\inetpub\iams-api\appsettings.Production.json" /inheritance:r
icacls "C:\inetpub\iams-api\appsettings.Production.json" /grant "Administrators:F"
icacls "C:\inetpub\iams-api\appsettings.Production.json" /grant "IIS_IUSRS:R"
```

### Step 12: SQL Server Security

```sql
-- Disable SA account if not needed
ALTER LOGIN sa DISABLE;
GO

-- Restrict SQL Server to local connections only (if applicable)
-- In SQL Server Configuration Manager:
-- SQL Server Network Configuration > Protocols > TCP/IP Properties
-- IP Addresses tab > Set "Listen All" to No
-- Enable only 127.0.0.1
```

---

## Phase 7: Backup & Monitoring (Day 3)

### Step 13: Configure Automated Backups

**13.1 SQL Server Backup Script**

Create `C:\Scripts\BackupIAMS.ps1`:
```powershell
# SQL Server backup script
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "D:\Backups\IAMS"

# Create backup directory if not exists
if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath
}

# Backup Master Database
$backupFile = "$backupPath\IAMS_Master_$timestamp.bak"
Invoke-Sqlcmd -Query "BACKUP DATABASE [IAMS_Master] TO DISK = N'$backupFile' WITH COMPRESSION, STATS = 10"

# Backup Tenant Database
$backupFile = "$backupPath\IAMS_Tenant_elzem_$timestamp.bak"
Invoke-Sqlcmd -Query "BACKUP DATABASE [IAMS_Tenant_elzem] TO DISK = N'$backupFile' WITH COMPRESSION, STATS = 10"

# Delete backups older than 30 days
Get-ChildItem -Path $backupPath -Filter "*.bak" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force

Write-Host "Backup completed successfully"
```

**13.2 Schedule Backup Task**
```powershell
# Create scheduled task (daily at 2 AM)
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
  -Argument "-File C:\Scripts\BackupIAMS.ps1"

$trigger = New-ScheduledTaskTrigger -Daily -At 2am

$principal = New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" `
  -LogonType ServiceAccount -RunLevel Highest

Register-ScheduledTask -Action $action -Trigger $trigger `
  -Principal $principal -TaskName "IAMS Database Backup" `
  -Description "Daily backup of IAMS databases"
```

**13.3 Application Files Backup**
```powershell
# Create application backup script
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "D:\Backups\IAMS\AppFiles"

# Backup application files
Compress-Archive -Path "C:\inetpub\iams-api" -DestinationPath "$backupPath\api_$timestamp.zip"
Compress-Archive -Path "C:\inetpub\iams-web" -DestinationPath "$backupPath\web_$timestamp.zip"
```

---

### Step 14: Monitoring Setup

**14.1 Windows Event Viewer**
```powershell
# Application logs will appear in:
# Event Viewer > Windows Logs > Application
# Filter by source: "ASP.NET Core"
```

**14.2 Install Application Insights (Optional)**
- Add Application Insights NuGet package
- Configure with your instrumentation key
- Free tier: 5GB/month

**14.3 Health Check Monitoring**

Create `C:\Scripts\HealthCheck.ps1`:
```powershell
# Health check script
$url = "https://elzem.websuresoft.com/api/health"

try {
    $response = Invoke-WebRequest -Uri $url -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "Health check passed"
        exit 0
    } else {
        Write-Host "Health check failed with status: $($response.StatusCode)"
        # Send email alert or log error
        exit 1
    }
} catch {
    Write-Host "Health check failed: $_"
    # Send email alert
    exit 1
}
```

Schedule this every 5 minutes.

---

## Phase 8: Final Testing & Launch

### Step 15: Testing Checklist

```powershell
# Test HTTPS redirect
curl http://elzem.websuresoft.com
# Should redirect to https://

# Test API endpoints
curl https://elzem.websuresoft.com/api/health
# Should return: { "status": "Healthy" }

# Test web application
# Open browser: https://elzem.websuresoft.com
# Should load the application

# Test database connection
# Login to application and perform basic operations

# Check logs
Get-Content C:\Logs\IAMS\log-*.txt -Tail 50
```

---

## Maintenance Procedures

### Update Deployment

```powershell
# 1. Stop IIS site
Stop-WebSite -Name "IAMS_Main"

# 2. Backup current version
Compress-Archive -Path "C:\inetpub\iams-api" -DestinationPath "D:\Backups\IAMS\api_pre_update.zip"
Compress-Archive -Path "C:\inetpub\iams-web" -DestinationPath "D:\Backups\IAMS\web_pre_update.zip"

# 3. Copy new files
Copy-Item -Path "\\dev-machine\publish\api\*" -Destination "C:\inetpub\iams-api" -Recurse -Force
Copy-Item -Path "\\dev-machine\publish\web\*" -Destination "C:\inetpub\iams-web" -Recurse -Force

# 4. Run migrations if needed
cd C:\inetpub\iams-api
dotnet ef database update

# 5. Start IIS site
Start-WebSite -Name "IAMS_Main"

# 6. Verify
curl https://elzem.websuresoft.com/api/health
```

### Troubleshooting

**Application won't start:**
```powershell
# Check IIS logs
Get-Content C:\inetpub\iams-api\logs\stdout*.log -Tail 100

# Check application logs
Get-Content C:\Logs\IAMS\log-*.txt -Tail 100

# Check Windows Event Viewer
Get-EventLog -LogName Application -Source "ASP.NET Core*" -Newest 50
```

**Database connection issues:**
```powershell
# Test SQL connection
sqlcmd -S localhost -U iams_app -P "YourPassword" -Q "SELECT @@VERSION"

# Check SQL Server is running
Get-Service -Name "MSSQLSERVER"
```

**SSL certificate renewal:**
```powershell
# win-acme auto-renews, but to manually renew:
cd C:\win-acme
.\wacs.exe --renew
```

---

## Cost Summary

| Component | Cost |
|-----------|------|
| Windows Server license | $0-500 (if you have license) |
| SQL Server Express | FREE |
| SSL Certificate (Let's Encrypt) | FREE |
| Hardware (if purchasing) | $500-2000 one-time |
| **Monthly operational cost** | **~$0** (electricity only) |

---

## Timeline

- **Day 1 Morning**: Server software installation (2-3 hours)
- **Day 1 Afternoon**: Database setup (1-2 hours)
- **Day 1 Evening**: Application deployment (2-3 hours)
- **Day 2 Morning**: SSL & domain setup (1-2 hours)
- **Day 2 Afternoon**: Database migration (1 hour)
- **Day 2 Evening**: Security hardening (1-2 hours)
- **Day 3**: Backup, monitoring, testing (2-4 hours)

**Total: 2-3 days** for complete setup

---

## Next Steps

1. Ensure server meets requirements
2. Configure DNS: Point elzem.websuresoft.com to server IP
3. Follow Phase 1-8 in order
4. Test thoroughly before production use
5. Document admin passwords securely

---

## Support Contacts

If you need help during setup:
- SQL Server issues: Check SQL Server error logs at `C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Log\ERRORLOG`
- IIS issues: Check IIS logs at `C:\inetpub\logs\LogFiles`
- Application issues: Check application logs at `C:\Logs\IAMS`
