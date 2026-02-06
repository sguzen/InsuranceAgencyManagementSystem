# Single Tenant Deployment Guide - Minimal Cost
## Azure Free/Low-Cost Tier Configuration

This guide shows how to deploy IAMS for a single tenant using minimal Azure resources.

---

## Overview

**Target**: Single tenant, testing/pilot phase
**Estimated Cost**: $20-60/month
**Suitable For**:
- First customer pilot
- Development/staging environment
- Small agency (1-10 users)

---

## Architecture

```
Internet → Azure App Service (Web + API) → Azure SQL Database (Single DB)
                                        → In-Memory Cache (No Redis)
```

**Simplified from production:**
- Single App Service for both Web and API
- Single SQL Database (no elastic pool needed)
- In-memory caching (no Redis)
- No CDN initially
- No auto-scaling

---

## Step-by-Step Setup

### Prerequisites

1. Azure account with active subscription
2. Azure CLI installed: `az login`
3. .NET 8.0 SDK installed

### Step 1: Create Resource Group

```bash
# Login to Azure
az login

# Set variables
RESOURCE_GROUP="rg-iams-single"
LOCATION="westeurope"  # or your preferred region
APP_NAME="iams-app-001"  # Must be globally unique
SQL_SERVER="iams-sql-001"  # Must be globally unique
SQL_ADMIN="iamsadmin"
SQL_PASSWORD="YourSecurePassword123!"  # Change this!

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

### Step 2: Create SQL Database

**Option A: SQL Database Basic (Recommended for 24/7 operation)**

```bash
# Create SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your current IP (for development)
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP

# Create database - Basic tier
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name iams-db \
  --service-objective Basic \
  --backup-storage-redundancy Local

# Cost: ~$5/month
```

**Option B: SQL Database Serverless (Recommended for testing/dev)**

```bash
# Create serverless database (auto-pauses when idle)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name iams-db \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --capacity 1 \
  --min-capacity 0.5 \
  --auto-pause-delay 60 \
  --backup-storage-redundancy Local

# Cost: ~$5-15/month (depends on usage, can pause automatically)
```

### Step 3: Create App Service

**Option A: Free Tier (F1) - Testing Only**

```bash
# Create App Service Plan - Free tier
az appservice plan create \
  --name plan-$APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku F1 \
  --is-linux

# Create Web App
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan plan-$APP_NAME \
  --runtime "DOTNETCORE:8.0"

# Cost: FREE (limitations: 60 CPU min/day, 1GB RAM)
```

**Option B: Basic Tier (B1) - Recommended**

```bash
# Create App Service Plan - Basic tier
az appservice plan create \
  --name plan-$APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan plan-$APP_NAME \
  --runtime "DOTNETCORE:8.0"

# Cost: ~$13/month
```

### Step 4: Configure Application Settings

```bash
# Get SQL connection string
SQL_CONN="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Database=iams-db;User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configure app settings
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    ConnectionStrings__MasterConnection="$SQL_CONN" \
    ConnectionStrings__DefaultConnection="$SQL_CONN" \
    MultiTenancy__ResolutionStrategy="Header" \
    JwtSettings__Secret="YourJwtSecretKeyMinimum32CharactersLong!" \
    JwtSettings__Issuer="https://${APP_NAME}.azurewebsites.net" \
    JwtSettings__Audience="https://${APP_NAME}.azurewebsites.net" \
    Cache__Provider="Memory"  \
    Features__EnableSwagger="false"

# Note: In production, use Key Vault for secrets!
```

### Step 5: Deploy Application

**Option A: Deploy from local build**

```bash
cd /path/to/InsuranceAgencyManagementSystem

# Publish the application
dotnet publish src/IAMS.Api/IAMS.Api.csproj \
  -c Release \
  -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --src deploy.zip

# Clean up
rm deploy.zip
rm -rf publish
```

**Option B: Deploy via Docker (if using containers)**

```bash
# Build and push to Azure Container Registry first
# Then deploy
az webapp config container set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --docker-custom-image-name your-registry/iams-api:latest
```

### Step 6: Run Database Migrations

```bash
# SSH into the web app
az webapp ssh --name $APP_NAME --resource-group $RESOURCE_GROUP

# Inside the SSH session, run:
cd /home/site/wwwroot
dotnet ef database update

# Exit SSH
exit
```

**Or run migrations locally:**

```bash
# Set connection string
export ConnectionStrings__MasterConnection="Server=tcp:${SQL_SERVER}.database.windows.net,1433;..."

# Run migrations
dotnet ef database update \
  --project src/IAMS.Persistence/IAMS.Persistence.csproj \
  --startup-project src/IAMS.Api/IAMS.Api.csproj
```

### Step 7: Verify Deployment

```bash
# Get the URL
APP_URL=$(az webapp show --name $APP_NAME --resource-group $RESOURCE_GROUP --query defaultHostName -o tsv)

echo "Application URL: https://$APP_URL"

# Test health endpoint
curl https://$APP_URL/health

# Test in browser
# Navigate to: https://$APP_URL/swagger (if enabled for testing)
```

---

## Configuration for Single Tenant

### Simplified appsettings.json

Update your `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "MasterConnection": "",  // Set via environment variable
    "DefaultConnection": ""   // Set via environment variable
  },
  "MultiTenancy": {
    "ResolutionStrategy": "Header",
    "DefaultTenantId": "tenant001",
    "CacheDuration": 3600
  },
  "Cache": {
    "Provider": "Memory",  // No Redis needed
    "DefaultExpirationMinutes": 60
  },
  "JwtSettings": {
    "Secret": "",  // Set via environment variable
    "Issuer": "",
    "Audience": "",
    "ExpiryInMinutes": 60
  },
  "ApiSettings": {
    "BaseUrl": "https://your-app.azurewebsites.net",
    "EnableSwagger": false
  }
}
```

### Create Initial Tenant

**Option 1: Using SQL Script**

```sql
-- Connect to your database using Azure Data Studio or SSMS
-- Insert initial tenant
INSERT INTO Tenants (TenantId, Name, DatabaseName, IsActive, CreatedAt)
VALUES ('tenant001', 'My First Agency', 'iams-db', 1, GETUTCDATE());
```

**Option 2: Using Admin CLI Tool** (if you create one)

```bash
dotnet run --project tools/IAMS.Admin.CLI -- create-tenant \
  --tenant-id tenant001 \
  --name "My First Agency" \
  --database-name iams-db
```

---

## Cost Breakdown

### Minimum Configuration (Testing)

| Resource | SKU | Monthly Cost |
|----------|-----|--------------|
| App Service | F1 Free | $0 |
| SQL Database | Serverless (auto-pause) | $5-10 |
| Storage | Minimal usage | <$1 |
| **Total** | | **~$5-11/month** |

**Limitations:**
- F1 App Service: 60 CPU minutes/day limit
- Not suitable for production workload
- Good for testing/development

### Recommended Configuration (Production-Ready)

| Resource | SKU | Monthly Cost |
|----------|-----|--------------|
| App Service | B1 Basic | $13 |
| SQL Database | Basic (2GB) | $5 |
| Storage | Standard LRS | $2 |
| Bandwidth | ~10GB/month | $1 |
| **Total** | | **~$21/month** |

### With Redis (Better Performance)

| Resource | SKU | Monthly Cost |
|----------|-----|--------------|
| App Service | B1 Basic | $13 |
| SQL Database | Basic | $5 |
| Redis Cache | C0 Basic (250MB) | $16 |
| Storage | Standard LRS | $2 |
| **Total** | | **~$36/month** |

---

## Scaling Up Later

When you need more performance:

```bash
# Scale up App Service
az appservice plan update \
  --name plan-$APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku B2  # $26/month - 2 cores, 3.5GB RAM

# Scale up SQL Database
az sql db update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name iams-db \
  --service-objective S0  # $15/month - 10 DTUs

# Add Redis Cache
az redis create \
  --name redis-iams \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Basic \
  --vm-size C0  # $16/month
```

---

## Monitoring (Free)

```bash
# Enable Application Insights (Free tier: 5GB/month)
az monitor app-insights component create \
  --app iams-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Get instrumentation key
INSIGHTS_KEY=$(az monitor app-insights component show \
  --app iams-insights \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

# Add to app settings
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings ApplicationInsights__InstrumentationKey="$INSIGHTS_KEY"
```

---

## Important Notes

### Security Best Practices

Even for single tenant:

1. **Never hardcode secrets** - use environment variables
2. **Enable HTTPS only** - Azure does this by default
3. **Use Azure Key Vault** for production secrets
4. **Enable backup** for SQL Database
5. **Set up alerts** in Azure Monitor

### Backup Configuration

```bash
# SQL Database automatic backups are enabled by default
# Retention: 7 days for Basic tier

# To extend retention (costs extra):
az sql db ltr-policy set \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --database iams-db \
  --weekly-retention P4W \
  --monthly-retention P12M
```

---

## Troubleshooting

### Common Issues

**1. Cannot connect to database from local machine**
```bash
# Add your IP to firewall
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name MyCurrentIP \
  --start-ip-address $(curl -s https://api.ipify.org) \
  --end-ip-address $(curl -s https://api.ipify.org)
```

**2. App Service shows "Service Unavailable"**
```bash
# Check logs
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP

# Or download logs
az webapp log download --name $APP_NAME --resource-group $RESOURCE_GROUP
```

**3. High costs on serverless database**
```bash
# Reduce min capacity
az sql db update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name iams-db \
  --min-capacity 0.5 \
  --auto-pause-delay 60
```

---

## Migration Path

When you're ready to scale:

1. **Add second tenant** → Keep same infrastructure, just add tenant entry
2. **5-10 tenants** → Upgrade to S1 App Service (~$75/month)
3. **10+ tenants** → Move to elastic pool for databases
4. **50+ tenants** → Implement full Phase 2 architecture from DEPLOYMENT_STRATEGY.md

---

## Cleanup (When Done Testing)

```bash
# Delete everything
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

This removes all resources and stops billing.
