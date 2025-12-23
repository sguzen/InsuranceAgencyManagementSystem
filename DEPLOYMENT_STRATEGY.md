# IAMS Deployment Strategy
## Production Deployment & Scaling to 1000+ Tenants

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Deployment Architecture](#deployment-architecture)
3. [Phase 1: Initial Deployment (MVP)](#phase-1-initial-deployment-mvp)
4. [Phase 2: Scaling to 100 Tenants](#phase-2-scaling-to-100-tenants)
5. [Phase 3: Scaling to 1000+ Tenants](#phase-3-scaling-to-1000-tenants)
6. [Infrastructure as Code](#infrastructure-as-code)
7. [CI/CD Pipeline Strategy](#cicd-pipeline-strategy)
8. [Database Strategy](#database-strategy)
9. [Monitoring & Observability](#monitoring--observability)
10. [Security & Compliance](#security--compliance)
11. [Disaster Recovery & Backup](#disaster-recovery--backup)
12. [Cost Optimization](#cost-optimization)
13. [Migration Checklist](#migration-checklist)

---

## Executive Summary

This document outlines a phased deployment strategy for IAMS, designed to:
- **Start simple** with an MVP that can onboard the first tenants quickly
- **Scale incrementally** to support growing tenant base
- **Optimize costs** while maintaining performance and reliability
- **Ensure security** and data isolation for all tenants
- **Enable automation** for tenant provisioning and management

### Key Architectural Decisions

1. **Database-per-Tenant**: Maintain isolation but optimize infrastructure
2. **Containerized Deployment**: Docker + Kubernetes for scalability
3. **Multi-Region Support**: Prepare for geographic distribution
4. **Automated Provisioning**: Self-service tenant onboarding
5. **Observability-First**: Built-in monitoring from day one

---

## Deployment Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Load Balancer / CDN                      │
│                   (Azure Front Door / CloudFlare)            │
└──────────────────────────┬──────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
    ┌──────────┐    ┌──────────┐    ┌──────────┐
    │  Web App │    │  Web App │    │  Web App │
    │ Instance │    │ Instance │    │ Instance │
    └─────┬────┘    └─────┬────┘    └─────┬────┘
          │               │               │
          └───────────────┼───────────────┘
                          │
              ┌───────────┴───────────┐
              │                       │
              ▼                       ▼
       ┌─────────────┐         ┌──────────┐
       │   API App   │         │ Background│
       │  Instances  │         │  Workers  │
       └──────┬──────┘         └─────┬────┘
              │                      │
    ┌─────────┼──────────────────────┤
    │         │                      │
    ▼         ▼                      ▼
┌────────┐ ┌──────────────────────────┐
│ Redis  │ │    Database Cluster      │
│ Cache  │ │                          │
└────────┘ │ ┌──────────────────────┐ │
           │ │  Master Database     │ │
           │ │  (Tenant Metadata)   │ │
           │ └──────────────────────┘ │
           │                          │
           │ ┌──────────────────────┐ │
           │ │   Tenant Database    │ │
           │ │   Pool (Shared)      │ │
           │ │   - DB Server 1      │ │
           │ │   - DB Server 2      │ │
           │ │   - DB Server N      │ │
           │ └──────────────────────┘ │
           └──────────────────────────┘
```

### Component Breakdown

**Frontend Layer**
- **IAMS.Web**: Blazor/MVC web application
- Served via CDN for static assets
- Auto-scaling based on traffic

**API Layer**
- **IAMS.Api**: RESTful API endpoints
- Stateless design for horizontal scaling
- Rate limiting and throttling per tenant

**Data Layer**
- **Master Database**: Single database for tenant metadata, subscriptions, configurations
- **Tenant Databases**: Separate database per tenant (pooled on shared servers)
- **Redis Cache**: Tenant metadata, session state, application cache

**Background Services**
- Tenant provisioning automation
- Database migration jobs
- Data synchronization with insurance companies
- Report generation
- Scheduled maintenance tasks

---

## Phase 1: Initial Deployment (MVP)
**Timeline**: Weeks 1-2
**Target**: First 1-10 tenants

### Objectives
- Get production environment running quickly
- Validate multi-tenant architecture in production
- Establish baseline monitoring and operations
- Minimize initial infrastructure costs

### Infrastructure Components

#### Cloud Provider Recommendation: **Azure** (preferred) or AWS

**Why Azure**:
- Excellent .NET integration
- Azure SQL Database with elastic pools (perfect for database-per-tenant)
- Azure App Service supports auto-scaling
- Azure Front Door for global load balancing
- Strong European data center presence (compliance)

#### Minimal Infrastructure Setup

```yaml
Resources:
  - 1x Azure App Service Plan (P2V3 or P3V3)
    - IAMS.Web instance
    - IAMS.Api instance

  - 1x Azure SQL Database Server
    - Master Database (S2 tier)
    - Tenant Databases (Elastic Pool - Standard 100 eDTU)

  - 1x Azure Cache for Redis (Basic C1)

  - 1x Azure Storage Account
    - Blob storage for documents
    - File storage for logs

  - 1x Azure Key Vault
    - Connection strings
    - Secrets and API keys

  - 1x Application Insights
    - Performance monitoring
    - Error tracking
```

### Deployment Steps

#### Step 1: Prepare Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name rg-iams-prod \
  --location westeurope

# Create App Service Plan
az appservice plan create \
  --name plan-iams-prod \
  --resource-group rg-iams-prod \
  --sku P2V3 \
  --is-linux

# Create Web App for API
az webapp create \
  --name iams-api-prod \
  --resource-group rg-iams-prod \
  --plan plan-iams-prod \
  --runtime "DOTNETCORE:8.0"

# Create Web App for Frontend
az webapp create \
  --name iams-web-prod \
  --resource-group rg-iams-prod \
  --plan plan-iams-prod \
  --runtime "DOTNETCORE:8.0"

# Create SQL Server
az sql server create \
  --name sql-iams-prod \
  --resource-group rg-iams-prod \
  --location westeurope \
  --admin-user iamsadmin \
  --admin-password '<strong-password>'

# Create Master Database
az sql db create \
  --resource-group rg-iams-prod \
  --server sql-iams-prod \
  --name iams-master \
  --service-objective S2

# Create Elastic Pool for Tenant Databases
az sql elastic-pool create \
  --resource-group rg-iams-prod \
  --server sql-iams-prod \
  --name pool-tenants \
  --edition Standard \
  --dtu 100 \
  --db-dtu-max 50 \
  --db-dtu-min 10

# Create Redis Cache
az redis create \
  --name redis-iams-prod \
  --resource-group rg-iams-prod \
  --location westeurope \
  --sku Basic \
  --vm-size C1

# Create Key Vault
az keyvault create \
  --name kv-iams-prod \
  --resource-group rg-iams-prod \
  --location westeurope

# Create Storage Account
az storage account create \
  --name stiamsprod \
  --resource-group rg-iams-prod \
  --location westeurope \
  --sku Standard_LRS
```

#### Step 2: Configure Secrets in Key Vault

```bash
# Store connection strings
az keyvault secret set \
  --vault-name kv-iams-prod \
  --name MasterConnectionString \
  --value "Server=tcp:sql-iams-prod.database.windows.net,1433;Database=iams-master;..."

# Store Redis connection
az keyvault secret set \
  --vault-name kv-iams-prod \
  --name RedisConnectionString \
  --value "redis-iams-prod.redis.cache.windows.net:6380,password=..."

# Grant App Service access to Key Vault
az webapp identity assign \
  --name iams-api-prod \
  --resource-group rg-iams-prod

# Set access policy
az keyvault set-policy \
  --name kv-iams-prod \
  --object-id <app-service-identity> \
  --secret-permissions get list
```

#### Step 3: Build and Deploy Application

```bash
# Build Docker images
cd src/IAMS.Api
docker build -t iams-api:latest .

cd ../IAMS.Web
docker build -t iams-web:latest .

# Push to Azure Container Registry
az acr create \
  --resource-group rg-iams-prod \
  --name acriamsprod \
  --sku Standard

az acr login --name acriamsprod

docker tag iams-api:latest acriamsprod.azurecr.io/iams-api:latest
docker push acriamsprod.azurecr.io/iams-api:latest

docker tag iams-web:latest acriamsprod.azurecr.io/iams-web:latest
docker push acriamsprod.azurecr.io/iams-web:latest

# Configure Web Apps to use ACR
az webapp config container set \
  --name iams-api-prod \
  --resource-group rg-iams-prod \
  --docker-custom-image-name acriamsprod.azurecr.io/iams-api:latest \
  --docker-registry-server-url https://acriamsprod.azurecr.io
```

#### Step 4: Initialize Master Database

```bash
# Run EF Core migrations
dotnet ef database update --project src/IAMS.Persistence --startup-project src/IAMS.Api
```

#### Step 5: Configure First Tenant

Create a tenant provisioning API or admin tool:

```csharp
// POST /api/admin/tenants
{
  "tenantId": "agency001",
  "name": "First Insurance Agency",
  "subdomain": "agency001",
  "adminEmail": "admin@agency001.com",
  "modules": ["Core", "Reporting"]
}
```

This will:
1. Create entry in Master database
2. Provision new database in elastic pool
3. Run tenant-specific migrations
4. Create admin user
5. Send welcome email

### Cost Estimate (Phase 1)

| Component | SKU | Monthly Cost (USD) |
|-----------|-----|-------------------|
| App Service Plan P2V3 | 2 cores, 8GB RAM | $292 |
| Azure SQL (Master) | S2 | $150 |
| Elastic Pool (10 tenants) | Standard 100 eDTU | $148 |
| Redis Cache | Basic C1 | $16 |
| Storage Account | Standard LRS, 100GB | $5 |
| Application Insights | 5GB/month | $12 |
| **Total** | | **~$623/month** |

**Per-tenant cost**: ~$60/month for infrastructure (decreases with scale)

---

## Phase 2: Scaling to 100 Tenants
**Timeline**: Months 1-6
**Target**: 10-100 tenants

### Challenges
- Database server capacity limits
- Application instance performance
- Cost optimization needed
- Automated tenant provisioning required
- Monitoring complexity increases

### Infrastructure Evolution

#### Database Scaling Strategy

**Elastic Pool Optimization**:
```yaml
Current: 1 Elastic Pool (100 eDTU) → 10 databases
Scaling: 3 Elastic Pools (200 eDTU each) → 100 databases

Pool Distribution Strategy:
  - Pool 1 (Premium): High-tier tenants (20 databases)
    - 250 eDTU, auto-scaling enabled

  - Pool 2 (Standard): Medium-tier tenants (40 databases)
    - 200 eDTU

  - Pool 3 (Standard): Small tenants (40 databases)
    - 200 eDTU
```

**Pool Allocation Logic**:
```csharp
public class TenantDatabaseProvisioner
{
    public async Task<string> ProvisionTenantDatabase(TenantSubscription subscription)
    {
        var pool = subscription.Tier switch
        {
            SubscriptionTier.Enterprise => "pool-premium-tenants",
            SubscriptionTier.Professional => "pool-standard-high",
            SubscriptionTier.Basic => "pool-standard-low",
            _ => "pool-standard-low"
        };

        // Check pool capacity before provisioning
        var poolMetrics = await _azureClient.GetElasticPoolMetrics(pool);
        if (poolMetrics.DatabaseCount >= poolMetrics.MaxDatabases)
        {
            // Create new pool or upgrade existing
            pool = await CreateOrExpandPool(subscription.Tier);
        }

        return await CreateDatabaseInPool(subscription.TenantId, pool);
    }
}
```

#### Application Scaling

**Auto-scaling Configuration**:
```yaml
Web App Auto-scaling Rules:
  - Scale out when: CPU > 70% for 5 minutes
  - Scale in when: CPU < 30% for 10 minutes
  - Min instances: 2
  - Max instances: 10
  - Scale increment: 1 instance

API App Auto-scaling Rules:
  - Scale out when: Request count > 1000/min OR CPU > 75%
  - Scale in when: Request count < 300/min AND CPU < 25%
  - Min instances: 3
  - Max instances: 20
```

#### Caching Enhancement

```yaml
Redis Cache Upgrade:
  From: Basic C1 (250MB)
  To: Standard C3 (6GB)

  Features:
    - Redis clustering enabled
    - Replication for high availability
    - Persistence enabled (RDB snapshots)

Cache Strategy:
  - Tenant metadata: 24-hour TTL
  - User sessions: 60-minute sliding expiration
  - Application configuration: Cache until change event
  - Database query results: 5-minute TTL
```

#### Content Delivery Network

```yaml
Azure Front Door Configuration:
  - Global load balancing
  - SSL/TLS termination
  - WAF (Web Application Firewall)
  - Static asset caching
  - Custom domain support per tenant

  Rules:
    - Cache static assets: 7 days
    - Cache API responses: Tenant-specific (via headers)
    - Compress responses: Gzip/Brotli
```

### Tenant Provisioning Automation

#### Self-Service Onboarding Portal

```csharp
// Automated Tenant Provisioning Flow
public class TenantProvisioningService
{
    public async Task<TenantProvisioningResult> ProvisionNewTenant(
        TenantRegistrationRequest request)
    {
        // 1. Validate registration data
        await ValidateRegistration(request);

        // 2. Create tenant entry in master database
        var tenant = await CreateTenantMetadata(request);

        // 3. Provision database in appropriate pool
        var connectionString = await ProvisionDatabase(tenant);

        // 4. Run database migrations
        await RunTenantMigrations(connectionString);

        // 5. Seed initial data
        await SeedTenantData(tenant, request);

        // 6. Create admin user
        var adminUser = await CreateAdminUser(tenant, request.AdminEmail);

        // 7. Configure tenant-specific settings
        await ConfigureTenantSettings(tenant, request.Modules);

        // 8. Setup custom subdomain (agency001.yoursystem.com)
        await ConfigureCustomDomain(tenant.Subdomain);

        // 9. Send welcome email with credentials
        await SendWelcomeEmail(adminUser);

        // 10. Notify operations team
        await NotifyOpsTeam(tenant);

        return new TenantProvisioningResult
        {
            TenantId = tenant.Id,
            DatabaseName = tenant.DatabaseName,
            AdminUserId = adminUser.Id,
            LoginUrl = $"https://{tenant.Subdomain}.yoursystem.com"
        };
    }
}
```

### Cost Estimate (Phase 2)

| Component | SKU | Monthly Cost (USD) |
|-----------|-----|-------------------|
| App Service Plan (2x P3V3) | Auto-scale 2-10 instances | $1,168 |
| Azure SQL (Master) | S3 | $300 |
| Elastic Pools (3x) | 1x Premium, 2x Standard | $1,850 |
| Redis Cache | Standard C3 | $250 |
| Azure Front Door | Standard tier | $90 |
| Storage Account | 500GB | $25 |
| Application Insights | 25GB/month | $60 |
| Bandwidth | ~2TB | $165 |
| **Total** | | **~$3,908/month** |

**Per-tenant cost**: ~$39/month (decreasing with scale)

---

## Phase 3: Scaling to 1000+ Tenants
**Timeline**: Months 6-24
**Target**: 100-1000+ tenants

### Architecture Transformation

At this scale, the architecture must evolve significantly:

#### Database Architecture for Hyperscale

**Problem**: Managing 1000+ separate databases becomes operationally complex and expensive.

**Solution: Hybrid Approach**

```yaml
Database Strategy:
  Tier 1 (Enterprise Tenants): Dedicated Database Servers
    - 50-100 large tenants
    - Dedicated SQL instances
    - Premium performance tier
    - Isolated backup/restore
    - Custom SLAs

  Tier 2 (Professional Tenants): Consolidated Elastic Pools
    - 200-400 medium tenants
    - 10-15 elastic pools (50 databases each)
    - Standard performance tier
    - Automated management

  Tier 3 (Basic Tenants): High-Density Pooling
    - 600-900 small tenants
    - Shared schema with tenant_id column (optional migration)
    - Or ultra-high-density pools (100+ databases per pool)
    - Basic performance tier
    - Aggressive resource sharing
```

**Tenant Segmentation Logic**:
```csharp
public class TenantSegmentationService
{
    public TenantSegment DetermineTenantSegment(TenantMetrics metrics)
    {
        // Segment based on multiple factors
        var score = CalculateTenantScore(metrics);

        if (metrics.MonthlyRevenue > 500 ||
            metrics.ActiveUsers > 50 ||
            metrics.DatabaseSize > 10_000_000) // 10GB
        {
            return TenantSegment.Enterprise; // Dedicated infrastructure
        }
        else if (metrics.MonthlyRevenue > 100 ||
                 metrics.ActiveUsers > 10)
        {
            return TenantSegment.Professional; // Elastic pools
        }
        else
        {
            return TenantSegment.Basic; // High-density pools
        }
    }

    // Automated tenant migration between segments
    public async Task MigrateTenantSegment(Tenant tenant, TenantSegment newSegment)
    {
        // 1. Provision new database in target segment
        // 2. Copy data (online migration)
        // 3. Switch connection string
        // 4. Verify and cleanup old database
    }
}
```

#### Kubernetes Migration

**Why Kubernetes at this scale**:
- Better resource utilization
- Easier horizontal scaling
- Multi-region deployment
- Blue-green deployments
- Advanced traffic routing

**Architecture**:
```yaml
Kubernetes Cluster Configuration:

Node Pools:
  - Web App Pool:
      Node count: 3-15 (auto-scale)
      VM size: Standard_D4s_v3 (4 cores, 16GB)
      Purpose: IAMS.Web instances

  - API Pool:
      Node count: 5-30 (auto-scale)
      VM size: Standard_D4s_v3
      Purpose: IAMS.Api instances

  - Background Jobs Pool:
      Node count: 2-10 (auto-scale)
      VM size: Standard_D2s_v3
      Purpose: Hangfire workers, batch jobs

Deployments:
  - iams-web:
      Replicas: 3-20 (HPA based on CPU/Memory)
      Resources:
        Requests: 500m CPU, 1Gi Memory
        Limits: 2000m CPU, 4Gi Memory

  - iams-api:
      Replicas: 5-40 (HPA based on requests/sec)
      Resources:
        Requests: 1000m CPU, 2Gi Memory
        Limits: 3000m CPU, 6Gi Memory

  - iams-background:
      Replicas: 2-10
      Resources:
        Requests: 500m CPU, 1Gi Memory
        Limits: 2000m CPU, 4Gi Memory

Ingress:
  - NGINX Ingress Controller
  - Automatic SSL via cert-manager
  - Rate limiting per tenant
  - WAF integration
```

#### Database Connection Management

**Challenge**: 1000 databases = connection pool nightmares

**Solution**: Intelligent Connection Pooling

```csharp
public class TenantConnectionManager
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _connectionLocks;
    private readonly ILogger<TenantConnectionManager> _logger;

    // Limit concurrent connections per tenant
    private const int MaxConcurrentConnectionsPerTenant = 10;

    public async Task<DbConnection> GetConnectionForTenant(string tenantId)
    {
        // 1. Check if tenant connection info is cached
        if (!_cache.TryGetValue($"tenant:{tenantId}:connection",
            out TenantConnectionInfo connectionInfo))
        {
            connectionInfo = await LoadConnectionInfo(tenantId);
            _cache.Set($"tenant:{tenantId}:connection", connectionInfo,
                TimeSpan.FromHours(24));
        }

        // 2. Apply connection throttling
        var semaphore = _connectionLocks.GetOrAdd(tenantId,
            _ => new SemaphoreSlim(MaxConcurrentConnectionsPerTenant));

        await semaphore.WaitAsync();

        try
        {
            // 3. Get connection from pool
            var connection = new SqlConnection(connectionInfo.ConnectionString);
            await connection.OpenAsync();

            // 4. Set context for row-level security (if using shared schema)
            await SetTenantContext(connection, tenantId);

            return connection;
        }
        catch
        {
            semaphore.Release();
            throw;
        }
    }

    // Lazy loading of inactive tenant connections
    private async Task<TenantConnectionInfo> LoadConnectionInfo(string tenantId)
    {
        using var masterConnection = await GetMasterConnection();
        var tenant = await masterConnection.QuerySingleAsync<Tenant>(
            "SELECT * FROM Tenants WHERE TenantId = @TenantId",
            new { TenantId = tenantId });

        return new TenantConnectionInfo
        {
            ConnectionString = BuildConnectionString(tenant),
            DatabaseName = tenant.DatabaseName,
            Segment = tenant.Segment
        };
    }
}
```

#### Multi-Region Deployment

**Geographic Distribution**:
```yaml
Regions:
  Primary: West Europe (Amsterdam)
    - Full stack deployment
    - Master database (primary)
    - 70% of tenants

  Secondary: North Europe (Dublin)
    - Full stack deployment
    - Master database (read replica)
    - 20% of tenants
    - Failover capability

  Tertiary: UK South (London)
    - API instances only
    - Database read replicas
    - 10% of tenants
    - Compliance (UK data residency)

Traffic Routing:
  - Azure Front Door with geo-routing
  - Latency-based routing for optimal performance
  - Automatic failover on health check failures
```

#### Advanced Caching Strategy

```yaml
Caching Layers:

L1 - In-Memory Cache (per instance):
  - Application configuration
  - User permissions (current request)
  - Lookup data (enums, constants)
  - TTL: 5-15 minutes

L2 - Distributed Cache (Redis):
  - Tenant metadata
  - User sessions
  - API responses (selective)
  - Database query results
  - TTL: 1-24 hours

L3 - CDN Cache (Azure Front Door):
  - Static assets
  - Public content
  - Compressed responses
  - TTL: 7 days

Cache Invalidation:
  - Event-driven invalidation via Azure Service Bus
  - Tenant-specific cache keys
  - Cascade invalidation for related data
```

### Monitoring & Observability at Scale

```yaml
Monitoring Stack:

Application Performance Monitoring:
  - Azure Application Insights
  - Distributed tracing across services
  - Custom metrics per tenant
  - Performance baselines and anomaly detection

Infrastructure Monitoring:
  - Azure Monitor for infrastructure metrics
  - Kubernetes metrics (Prometheus + Grafana)
  - Database performance metrics
  - Network performance monitoring

Logging:
  - Centralized logging (Azure Log Analytics)
  - Structured logging (Serilog with JSON)
  - Log aggregation per tenant
  - 90-day retention with archival

Alerting:
  - Tenant-specific SLA monitoring
  - Automatic incident creation
  - PagerDuty/OpsGenie integration
  - Escalation policies

Dashboards:
  - Executive dashboard (system health)
  - Operations dashboard (real-time metrics)
  - Per-tenant dashboards
  - Cost analysis dashboard
```

### Cost Estimate (Phase 3)

| Component | SKU | Monthly Cost (USD) |
|-----------|-----|-------------------|
| AKS Cluster (3 node pools) | Auto-scale 10-55 nodes | $8,500 |
| Azure SQL (Master + Replicas) | Business Critical | $2,400 |
| Elastic Pools (15x) | Mixed tiers | $12,000 |
| Dedicated SQL (50x) | Various tiers | $18,000 |
| Redis Cache (Premium P3) | Clustered, 26GB | $1,200 |
| Azure Front Door | Premium tier | $350 |
| Storage Account | 5TB | $120 |
| Application Insights | 200GB/month | $480 |
| Bandwidth | 15TB | $1,237 |
| **Total** | | **~$44,287/month** |

**Per-tenant cost**: ~$44/month (economies of scale achieved)

---

## Infrastructure as Code

### Terraform Configuration

Create modular Terraform configurations for reproducible infrastructure:

**Directory Structure**:
```
infrastructure/
├── terraform/
│   ├── modules/
│   │   ├── app-service/
│   │   ├── sql-database/
│   │   ├── elastic-pool/
│   │   ├── redis-cache/
│   │   ├── kubernetes/
│   │   └── monitoring/
│   ├── environments/
│   │   ├── dev/
│   │   ├── staging/
│   │   └── production/
│   └── global/
│       └── shared-resources/
└── scripts/
    ├── provision-tenant.sh
    ├── migrate-tenant.sh
    └── backup-tenant.sh
```

**Example: Elastic Pool Module**:
```hcl
# modules/elastic-pool/main.tf
resource "azurerm_mssql_elasticpool" "tenant_pool" {
  name                = var.pool_name
  resource_group_name = var.resource_group_name
  location            = var.location
  server_name         = var.sql_server_name

  sku {
    name     = var.sku_name
    tier     = var.sku_tier
    capacity = var.capacity
  }

  per_database_settings {
    min_capacity = var.min_capacity_per_db
    max_capacity = var.max_capacity_per_db
  }

  max_size_gb = var.max_size_gb

  tags = merge(var.tags, {
    Environment = var.environment
    ManagedBy   = "Terraform"
  })
}

# modules/elastic-pool/variables.tf
variable "pool_name" {
  description = "Name of the elastic pool"
  type        = string
}

variable "sku_tier" {
  description = "SKU tier (Standard, Premium)"
  type        = string
  default     = "Standard"
}

variable "capacity" {
  description = "Capacity in eDTUs or vCores"
  type        = number
}

variable "min_capacity_per_db" {
  description = "Minimum capacity per database"
  type        = number
  default     = 10
}

variable "max_capacity_per_db" {
  description = "Maximum capacity per database"
  type        = number
  default     = 50
}
```

**Example: Production Environment**:
```hcl
# environments/production/main.tf
terraform {
  required_version = ">= 1.5"

  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "sttfstateprod"
    container_name       = "tfstate"
    key                  = "production.terraform.tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.75"
    }
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "iams_prod" {
  name     = "rg-iams-prod"
  location = "westeurope"

  tags = {
    Environment = "Production"
    Application = "IAMS"
    CostCenter  = "Engineering"
  }
}

# SQL Server
resource "azurerm_mssql_server" "iams_prod" {
  name                         = "sql-iams-prod"
  resource_group_name          = azurerm_resource_group.iams_prod.name
  location                     = azurerm_resource_group.iams_prod.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  azuread_administrator {
    login_username = var.azuread_admin_username
    object_id      = var.azuread_admin_object_id
  }

  public_network_access_enabled = false

  tags = azurerm_resource_group.iams_prod.tags
}

# Master Database
resource "azurerm_mssql_database" "master" {
  name      = "iams-master"
  server_id = azurerm_mssql_server.iams_prod.id

  sku_name                    = "S3"
  max_size_gb                 = 250
  zone_redundant              = true
  geo_backup_enabled          = true
  auto_pause_delay_in_minutes = -1

  tags = azurerm_resource_group.iams_prod.tags
}

# Elastic Pools for Different Tiers
module "elastic_pool_premium" {
  source = "../../modules/elastic-pool"

  pool_name           = "pool-premium-tenants"
  resource_group_name = azurerm_resource_group.iams_prod.name
  location            = azurerm_resource_group.iams_prod.location
  sql_server_name     = azurerm_mssql_server.iams_prod.name

  sku_tier            = "Premium"
  sku_name            = "PremiumPool"
  capacity            = 250
  min_capacity_per_db = 25
  max_capacity_per_db = 125
  max_size_gb         = 750

  environment = "Production"
  tags        = azurerm_resource_group.iams_prod.tags
}

module "elastic_pool_standard_high" {
  source = "../../modules/elastic-pool"

  pool_name           = "pool-standard-high"
  resource_group_name = azurerm_resource_group.iams_prod.name
  location            = azurerm_resource_group.iams_prod.location
  sql_server_name     = azurerm_mssql_server.iams_prod.name

  sku_tier            = "Standard"
  sku_name            = "StandardPool"
  capacity            = 200
  min_capacity_per_db = 10
  max_capacity_per_db = 50
  max_size_gb         = 500

  environment = "Production"
  tags        = azurerm_resource_group.iams_prod.tags
}

# AKS Cluster (Phase 3)
module "kubernetes_cluster" {
  source = "../../modules/kubernetes"
  count  = var.use_kubernetes ? 1 : 0

  cluster_name        = "aks-iams-prod"
  resource_group_name = azurerm_resource_group.iams_prod.name
  location            = azurerm_resource_group.iams_prod.location

  default_node_pool = {
    name       = "system"
    node_count = 3
    vm_size    = "Standard_D4s_v3"
  }

  additional_node_pools = {
    web = {
      name       = "web"
      min_count  = 3
      max_count  = 15
      vm_size    = "Standard_D4s_v3"
    }
    api = {
      name       = "api"
      min_count  = 5
      max_count  = 30
      vm_size    = "Standard_D4s_v3"
    }
  }

  tags = azurerm_resource_group.iams_prod.tags
}
```

---

## CI/CD Pipeline Strategy

### GitHub Actions Workflow

**Multi-Environment Pipeline**:

```yaml
# .github/workflows/deploy.yml
name: Deploy IAMS

on:
  push:
    branches: [main, develop, 'release/**']
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '8.0.x'
  REGISTRY: acriamsprod.azurecr.io

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run unit tests
        run: dotnet test tests/IAMS.UnitTests --no-build --verbosity normal

      - name: Run integration tests
        run: dotnet test tests/IAMS.IntegrationTests --no-build --verbosity normal

      - name: Code coverage
        run: |
          dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
          dotnet tool install --global dotnet-reportgenerator-globaltool
          reportgenerator -reports:./coverage/**/coverage.cobertura.xml -targetdir:./coverage/report -reporttypes:Html

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          directory: ./coverage

  build-docker-images:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' || startsWith(github.ref, 'refs/heads/release/')

    strategy:
      matrix:
        project: [IAMS.Api, IAMS.Web]

    steps:
      - uses: actions/checkout@v3

      - name: Login to Azure Container Registry
        uses: azure/docker-login@v1
        with:
          login-server: ${{ env.REGISTRY }}
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}

      - name: Build and push Docker image
        run: |
          IMAGE_TAG=${{ github.sha }}
          docker build -t ${{ env.REGISTRY }}/${{ matrix.project }}:${IMAGE_TAG} \
            -t ${{ env.REGISTRY }}/${{ matrix.project }}:latest \
            -f src/${{ matrix.project }}/Dockerfile .
          docker push ${{ env.REGISTRY }}/${{ matrix.project }}:${IMAGE_TAG}
          docker push ${{ env.REGISTRY }}/${{ matrix.project }}:latest

  deploy-staging:
    needs: build-docker-images
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging

    steps:
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS_STAGING }}

      - name: Deploy to Azure Web App (API)
        uses: azure/webapps-deploy@v2
        with:
          app-name: iams-api-staging
          images: ${{ env.REGISTRY }}/IAMS.Api:${{ github.sha }}

      - name: Deploy to Azure Web App (Web)
        uses: azure/webapps-deploy@v2
        with:
          app-name: iams-web-staging
          images: ${{ env.REGISTRY }}/IAMS.Web:${{ github.sha }}

      - name: Run smoke tests
        run: |
          curl -f https://iams-api-staging.azurewebsites.net/health || exit 1
          curl -f https://iams-web-staging.azurewebsites.net/health || exit 1

  deploy-production:
    needs: build-docker-images
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production

    steps:
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS_PRODUCTION }}

      - name: Deploy to Azure Web App (API) - Slot
        uses: azure/webapps-deploy@v2
        with:
          app-name: iams-api-prod
          slot-name: staging
          images: ${{ env.REGISTRY }}/IAMS.Api:${{ github.sha }}

      - name: Run smoke tests on staging slot
        run: |
          curl -f https://iams-api-prod-staging.azurewebsites.net/health || exit 1

      - name: Swap slots (Blue-Green Deployment)
        run: |
          az webapp deployment slot swap \
            --resource-group rg-iams-prod \
            --name iams-api-prod \
            --slot staging \
            --target-slot production

      - name: Deploy to Azure Web App (Web)
        uses: azure/webapps-deploy@v2
        with:
          app-name: iams-web-prod
          images: ${{ env.REGISTRY }}/IAMS.Web:${{ github.sha }}

      - name: Create deployment tag
        run: |
          git tag -a "deploy-$(date +%Y%m%d-%H%M%S)" -m "Production deployment"
          git push origin --tags

      - name: Notify team
        uses: 8398a7/action-slack@v3
        with:
          status: ${{ job.status }}
          text: 'Production deployment completed'
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}

  database-migrations:
    needs: deploy-production
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Install EF Core tools
        run: dotnet tool install --global dotnet-ef

      - name: Run master database migrations
        run: |
          dotnet ef database update \
            --project src/IAMS.Persistence \
            --startup-project src/IAMS.Api \
            --connection "${{ secrets.MASTER_CONNECTION_STRING }}"

      - name: Run tenant database migrations
        run: |
          # Script to migrate all tenant databases
          # This should be idempotent and handle errors gracefully
          ./scripts/migrate-all-tenants.sh
```

### Database Migration Strategy

**Tenant Migration Script**:
```bash
#!/bin/bash
# scripts/migrate-all-tenants.sh

set -e

MASTER_CONNECTION="${MASTER_CONNECTION_STRING}"
MIGRATION_LOG="migration-$(date +%Y%m%d-%H%M%S).log"

echo "Starting tenant database migrations..." | tee -a $MIGRATION_LOG

# Get list of all active tenants
TENANTS=$(dotnet run --project tools/IAMS.Admin.CLI -- list-tenants --active-only --output json)

# Iterate through each tenant
echo "$TENANTS" | jq -r '.[] | @base64' | while read -r TENANT_BASE64; do
    TENANT=$(echo "$TENANT_BASE64" | base64 --decode)
    TENANT_ID=$(echo "$TENANT" | jq -r '.tenantId')
    TENANT_DB=$(echo "$TENANT" | jq -r '.databaseName')

    echo "Migrating tenant: $TENANT_ID (Database: $TENANT_DB)" | tee -a $MIGRATION_LOG

    # Get tenant-specific connection string
    TENANT_CONNECTION=$(dotnet run --project tools/IAMS.Admin.CLI -- get-connection --tenant-id "$TENANT_ID")

    # Run migration with retry logic
    RETRY_COUNT=0
    MAX_RETRIES=3

    while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
        if dotnet ef database update \
            --project src/IAMS.Persistence \
            --startup-project src/IAMS.Api \
            --connection "$TENANT_CONNECTION" 2>&1 | tee -a $MIGRATION_LOG; then

            echo "✓ Successfully migrated $TENANT_ID" | tee -a $MIGRATION_LOG
            break
        else
            RETRY_COUNT=$((RETRY_COUNT + 1))
            echo "✗ Migration failed for $TENANT_ID (Attempt $RETRY_COUNT/$MAX_RETRIES)" | tee -a $MIGRATION_LOG

            if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
                echo "✗ CRITICAL: Migration failed for $TENANT_ID after $MAX_RETRIES attempts" | tee -a $MIGRATION_LOG
                # Send alert to operations team
                curl -X POST "$SLACK_WEBHOOK_URL" \
                    -H 'Content-Type: application/json' \
                    -d "{\"text\":\"⚠️ Database migration failed for tenant: $TENANT_ID\"}"
            else
                sleep 5
            fi
        fi
    done
done

echo "Migration process completed. See $MIGRATION_LOG for details."
```

---

## Database Strategy

### Tenant Database Lifecycle Management

#### Provisioning
```csharp
public class TenantDatabaseLifecycleService
{
    public async Task<DatabaseProvisioningResult> ProvisionTenantDatabase(
        string tenantId,
        SubscriptionTier tier)
    {
        // 1. Determine target pool/server based on tier
        var targetPool = await DetermineTargetPool(tier);

        // 2. Check pool capacity
        if (!await HasCapacity(targetPool))
        {
            // Auto-scale: create new pool or upgrade existing
            targetPool = await ExpandCapacity(tier);
        }

        // 3. Create database
        var databaseName = $"iams_tenant_{tenantId}";
        await CreateDatabase(databaseName, targetPool);

        // 4. Run initial schema migration
        var connectionString = BuildConnectionString(databaseName);
        await RunMigrations(connectionString);

        // 5. Seed initial data
        await SeedTenantData(connectionString, tenantId);

        // 6. Register in master database
        await RegisterTenantDatabase(tenantId, databaseName, targetPool);

        // 7. Configure backups
        await ConfigureBackupPolicy(databaseName, tier);

        return new DatabaseProvisioningResult
        {
            DatabaseName = databaseName,
            ConnectionString = connectionString,
            PoolName = targetPool.Name,
            Status = ProvisioningStatus.Completed
        };
    }
}
```

#### Migration Between Pools
```csharp
public class TenantDatabaseMigrationService
{
    public async Task MigrateTenantToNewPool(
        string tenantId,
        string targetPool,
        bool onlineMigration = true)
    {
        var tenant = await GetTenant(tenantId);
        var sourceDatabaseName = tenant.DatabaseName;
        var targetDatabaseName = $"{sourceDatabaseName}_new";

        if (onlineMigration)
        {
            // 1. Create database in target pool
            await CreateDatabase(targetDatabaseName, targetPool);

            // 2. Setup replication from source to target
            await SetupReplication(sourceDatabaseName, targetDatabaseName);

            // 3. Monitor replication lag
            await WaitForReplicationSync();

            // 4. Put tenant in maintenance mode (brief)
            await SetMaintenanceMode(tenantId, true);

            // 5. Final sync
            await FinalReplicationSync();

            // 6. Switch connection strings
            await UpdateTenantConnectionString(tenantId, targetDatabaseName);

            // 7. Remove maintenance mode
            await SetMaintenanceMode(tenantId, false);

            // 8. Cleanup: remove old database after verification period
            await ScheduleOldDatabaseCleanup(sourceDatabaseName, days: 7);
        }
        else
        {
            // Offline migration: simpler but requires downtime
            await SetMaintenanceMode(tenantId, true);
            await CopyDatabase(sourceDatabaseName, targetDatabaseName, targetPool);
            await UpdateTenantConnectionString(tenantId, targetDatabaseName);
            await SetMaintenanceMode(tenantId, false);
        }
    }
}
```

#### Backup and Restore
```csharp
public class TenantBackupService
{
    public async Task ConfigureBackupPolicy(string databaseName, SubscriptionTier tier)
    {
        var backupPolicy = tier switch
        {
            SubscriptionTier.Enterprise => new BackupPolicy
            {
                FullBackupFrequency = BackupFrequency.Daily,
                TransactionLogBackup = true,
                PointInTimeRetention = TimeSpan.FromDays(35),
                LongTermRetention = new LongTermRetentionPolicy
                {
                    WeeklyRetention = 12, // 12 weeks
                    MonthlyRetention = 12, // 12 months
                    YearlyRetention = 7 // 7 years
                },
                GeoRedundancy = true
            },
            SubscriptionTier.Professional => new BackupPolicy
            {
                FullBackupFrequency = BackupFrequency.Daily,
                TransactionLogBackup = true,
                PointInTimeRetention = TimeSpan.FromDays(14),
                GeoRedundancy = true
            },
            _ => new BackupPolicy
            {
                FullBackupFrequency = BackupFrequency.Weekly,
                TransactionLogBackup = false,
                PointInTimeRetention = TimeSpan.FromDays(7),
                GeoRedundancy = false
            }
        };

        await ApplyBackupPolicy(databaseName, backupPolicy);
    }

    public async Task<RestoreResult> RestoreTenantDatabase(
        string tenantId,
        DateTime pointInTime,
        RestoreStrategy strategy = RestoreStrategy.NewDatabase)
    {
        var tenant = await GetTenant(tenantId);

        switch (strategy)
        {
            case RestoreStrategy.NewDatabase:
                // Restore to new database for investigation
                var newDbName = $"{tenant.DatabaseName}_restore_{DateTime.UtcNow:yyyyMMddHHmmss}";
                await RestoreToPointInTime(tenant.DatabaseName, newDbName, pointInTime);
                return new RestoreResult { NewDatabaseName = newDbName };

            case RestoreStrategy.Replace:
                // Replace current database (requires maintenance mode)
                await SetMaintenanceMode(tenantId, true);
                var backupDbName = $"{tenant.DatabaseName}_backup_{DateTime.UtcNow:yyyyMMddHHmmss}";
                await RenameDatabase(tenant.DatabaseName, backupDbName);
                await RestoreToPointInTime(backupDbName, tenant.DatabaseName, pointInTime);
                await SetMaintenanceMode(tenantId, false);
                return new RestoreResult { Success = true };

            default:
                throw new ArgumentException($"Unknown restore strategy: {strategy}");
        }
    }
}
```

### Schema Migration Strategy

**Zero-Downtime Migrations**:
```csharp
// Example: Adding a new column with zero downtime

// Migration 1: Add column as nullable
public class AddCustomerPreferencesColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Preferences",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: true); // Important: nullable first
    }
}

// Migration 2: Backfill data (background job)
public class BackfillCustomerPreferences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Use background job to populate defaults
        migrationBuilder.Sql(@"
            UPDATE Customers
            SET Preferences = '{}'
            WHERE Preferences IS NULL
        ");
    }
}

// Migration 3: Make column non-nullable
public class MakeCustomerPreferencesRequired : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Preferences",
            table: "Customers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);
    }
}
```

---

## Monitoring & Observability

### Key Metrics to Track

#### System-Level Metrics
```yaml
Infrastructure:
  - CPU utilization per instance
  - Memory usage and GC metrics
  - Network throughput and latency
  - Disk I/O and storage usage
  - Container/Pod health and restarts

Application:
  - Request rate (requests/second)
  - Response time (p50, p95, p99)
  - Error rate and types
  - Active users/sessions
  - API endpoint performance

Database:
  - Connection pool utilization
  - Query execution time
  - DTU/CPU usage per pool
  - Storage usage per database
  - Deadlocks and blocking queries
```

#### Tenant-Specific Metrics
```csharp
public class TenantMetricsCollector
{
    private readonly IMetricsClient _metrics;

    public void RecordTenantOperation(string tenantId, string operation, TimeSpan duration)
    {
        _metrics.RecordValue(
            "tenant.operation.duration",
            duration.TotalMilliseconds,
            new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId,
                ["operation"] = operation,
                ["tier"] = GetTenantTier(tenantId)
            });
    }

    public void RecordDatabaseQuery(string tenantId, string queryType, TimeSpan duration)
    {
        _metrics.RecordValue(
            "tenant.database.query",
            duration.TotalMilliseconds,
            new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId,
                ["query_type"] = queryType
            });

        // Alert if query is slow
        if (duration.TotalMilliseconds > 5000)
        {
            _metrics.RecordEvent("slow_query_detected", new
            {
                tenant_id = tenantId,
                query_type = queryType,
                duration_ms = duration.TotalMilliseconds
            });
        }
    }
}
```

### Alerting Rules

```yaml
Critical Alerts (Page immediately):
  - API error rate > 5% for 5 minutes
  - Database connection failures
  - Any tenant completely down
  - Master database unavailable
  - Disk space > 90% on any server

Warning Alerts (Notify during business hours):
  - API p95 latency > 2 seconds for 15 minutes
  - Database DTU usage > 80% for 30 minutes
  - Memory usage > 85% for 15 minutes
  - Failed background jobs
  - Tenant migration failures

Informational:
  - New tenant provisioned
  - Deployment completed
  - Auto-scaling triggered
  - Scheduled maintenance started/completed
```

### Custom Dashboards

**Executive Dashboard**:
- Total active tenants
- Revenue metrics by tier
- System uptime (99.9% SLA)
- Active users across all tenants
- Storage consumption trends

**Operations Dashboard**:
- Real-time request rates
- Error rates and types
- Database performance metrics
- Infrastructure costs
- Alert status

**Per-Tenant Dashboard**:
- Tenant-specific performance metrics
- User activity
- Storage usage
- Feature usage statistics
- SLA compliance

---

## Security & Compliance

### Security Checklist

#### Infrastructure Security
- [ ] Network isolation (VNet/subnet segmentation)
- [ ] Private endpoints for databases
- [ ] Web Application Firewall (WAF) enabled
- [ ] DDoS protection enabled
- [ ] TLS 1.2+ enforced for all connections
- [ ] Managed identities for Azure services
- [ ] Key Vault for secrets management
- [ ] Regular security patching

#### Application Security
- [ ] JWT token authentication
- [ ] Role-based access control (RBAC)
- [ ] Input validation and sanitization
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS protection
- [ ] CSRF protection
- [ ] Rate limiting per tenant
- [ ] API versioning and deprecation policy

#### Data Security
- [ ] Encryption at rest (database TDE)
- [ ] Encryption in transit (TLS)
- [ ] Tenant data isolation verified
- [ ] Audit logging for all data access
- [ ] PII data encryption/masking
- [ ] Data retention policies
- [ ] GDPR compliance (right to delete)
- [ ] Regular security audits

#### Compliance
- [ ] SOC 2 Type II certification
- [ ] GDPR compliance
- [ ] Data residency requirements
- [ ] Audit trail for all changes
- [ ] Regular penetration testing
- [ ] Incident response plan
- [ ] Business continuity plan

### Security Scanning

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  schedule:
    - cron: '0 0 * * 0' # Weekly
  push:
    branches: [main]

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run dependency scan
        run: dotnet list package --vulnerable --include-transitive

      - name: OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'IAMS'
          path: '.'
          format: 'HTML'

  code-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run CodeQL Analysis
        uses: github/codeql-action/init@v2
        with:
          languages: csharp

      - name: Build
        run: dotnet build

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v2

  container-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker image
        run: docker build -t iams-api:scan -f src/IAMS.Api/Dockerfile .

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'iams-api:scan'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'
```

---

## Disaster Recovery & Backup

### RTO and RPO Targets

| Tier | RTO (Recovery Time Objective) | RPO (Recovery Point Objective) |
|------|------------------------------|--------------------------------|
| Enterprise | 1 hour | 5 minutes |
| Professional | 4 hours | 1 hour |
| Basic | 24 hours | 24 hours |

### Disaster Recovery Plan

#### Scenario 1: Regional Outage

**Detection**:
- Azure Front Door health probes fail
- Automatic failover to secondary region

**Response**:
1. Verify secondary region is healthy
2. Promote secondary SQL replicas to primary
3. Update DNS/routing to secondary region
4. Notify tenants of region switch
5. Monitor performance in secondary region

**Recovery**:
1. Restore primary region
2. Setup replication from secondary to primary
3. Wait for full synchronization
4. Failback during maintenance window
5. Post-mortem and documentation

#### Scenario 2: Data Corruption

**Detection**:
- Application errors or tenant reports
- Data validation checks fail

**Response**:
1. Identify affected tenant(s)
2. Put tenant in maintenance mode
3. Investigate scope of corruption
4. Restore from point-in-time backup
5. Validate restored data
6. Resume tenant operations

#### Scenario 3: Complete Database Loss

**Detection**:
- Database server unreachable
- All connections failing

**Response**:
1. Activate disaster recovery plan
2. Restore from geo-redundant backup
3. Verify data integrity
4. Redirect traffic to restored database
5. Communicate with affected tenants

### Backup Strategy Summary

```yaml
Master Database:
  - Automated daily backups
  - Point-in-time restore (35 days)
  - Geo-redundant backups
  - Weekly manual verification

Tenant Databases (Enterprise):
  - Automated daily backups
  - Transaction log backups every 5 minutes
  - Point-in-time restore (35 days)
  - Geo-redundant backups
  - Monthly restore testing

Tenant Databases (Professional):
  - Automated daily backups
  - Transaction log backups hourly
  - Point-in-time restore (14 days)
  - Geo-redundant backups

Tenant Databases (Basic):
  - Automated weekly backups
  - Point-in-time restore (7 days)
  - Local-redundant backups

Application Configuration:
  - Stored in Git (version controlled)
  - Secrets in Key Vault (with backup)
  - Infrastructure as Code (Terraform state backed up)
```

---

## Cost Optimization

### Cost Optimization Strategies

#### 1. Right-Sizing Resources

**Database Pools**:
- Monitor DTU/vCore usage
- Consolidate under-utilized pools
- Use auto-scaling for variable workloads
- Reserved capacity for predictable workloads (save up to 38%)

**Compute**:
- Use Azure Reservations (1-3 year commitment) for base capacity
- Auto-scale for peak traffic
- Shut down non-production environments outside business hours
- Use spot instances for background jobs (save up to 90%)

#### 2. Storage Optimization

```csharp
public class StorageOptimizationService
{
    // Archive old documents to cheaper storage tiers
    public async Task ArchiveOldDocuments()
    {
        var cutoffDate = DateTime.UtcNow.AddYears(-1);

        var oldDocuments = await _repository.GetDocumentsOlderThan(cutoffDate);

        foreach (var doc in oldDocuments)
        {
            // Move from Hot to Cool storage (50% cost reduction)
            await _blobClient.SetAccessTier(doc.BlobName, AccessTier.Cool);

            // After 3 years, move to Archive tier (90% cost reduction)
            if (doc.CreatedDate < DateTime.UtcNow.AddYears(-3))
            {
                await _blobClient.SetAccessTier(doc.BlobName, AccessTier.Archive);
            }
        }
    }

    // Compress large data
    public async Task CompressHistoricalData()
    {
        // Compress old audit logs, reports, etc.
        // Can reduce storage by 70-90%
    }
}
```

#### 3. Tenant-Based Cost Allocation

```csharp
public class TenantCostTrackingService
{
    public async Task<TenantCostReport> CalculateTenantCosts(string tenantId, DateTime month)
    {
        var tenant = await GetTenant(tenantId);

        // Database costs
        var databaseCost = await CalculateDatabaseCost(tenant.DatabaseName, month);

        // Compute costs (proportional to usage)
        var computeCost = await CalculateComputeCost(tenantId, month);

        // Storage costs
        var storageCost = await CalculateStorageCost(tenantId, month);

        // Bandwidth costs
        var bandwidthCost = await CalculateBandwidthCost(tenantId, month);

        var totalCost = databaseCost + computeCost + storageCost + bandwidthCost;

        // Cost allocation for pricing decisions
        return new TenantCostReport
        {
            TenantId = tenantId,
            Month = month,
            DatabaseCost = databaseCost,
            ComputeCost = computeCost,
            StorageCost = storageCost,
            BandwidthCost = bandwidthCost,
            TotalCost = totalCost,
            CostPerUser = totalCost / tenant.ActiveUserCount
        };
    }
}
```

#### 4. Cost Monitoring Dashboard

```csharp
// Real-time cost tracking
public class CostMonitoringService
{
    public async Task TrackDailyCosts()
    {
        var costs = await _azureCostManagement.GetDailyCosts();

        // Alert if costs exceed budget
        if (costs.TotalCost > _settings.DailyBudget)
        {
            await SendCostAlert(costs);
        }

        // Track cost per tenant
        foreach (var tenant in await GetActiveTenants())
        {
            var tenantCost = await CalculateTenantCosts(tenant.Id, DateTime.UtcNow.Date);

            // Flag unprofitable tenants
            if (tenantCost.TotalCost > tenant.MonthlyRevenue)
            {
                await FlagUnprofitableTenant(tenant, tenantCost);
            }
        }
    }
}
```

### Estimated Cost Progression

| Phase | Tenants | Monthly Cost | Cost/Tenant | Notes |
|-------|---------|-------------|-------------|-------|
| Phase 1 (MVP) | 1-10 | $623 | $62 | Initial setup, not profitable yet |
| Phase 2 (Growth) | 10-100 | $3,908 | $39 | Economies of scale kicking in |
| Phase 3 (Scale) | 100-1000 | $44,287 | $44 | Stable costs, optimized infrastructure |

**Revenue Required for Profitability**:
- Assuming 40% gross margin target
- Phase 1: $104/tenant/month minimum
- Phase 2: $65/tenant/month minimum
- Phase 3: $73/tenant/month minimum

---

## Migration Checklist

### Pre-Production Checklist

**Infrastructure**:
- [ ] All Azure resources provisioned via Terraform
- [ ] Environments configured (Dev, Staging, Production)
- [ ] Network security groups and firewall rules configured
- [ ] Private endpoints configured for databases
- [ ] Key Vault setup with all secrets
- [ ] Managed identities configured
- [ ] Azure Front Door configured with WAF
- [ ] CDN configured for static assets

**Application**:
- [ ] CI/CD pipeline tested and working
- [ ] Docker images building successfully
- [ ] Health check endpoints implemented
- [ ] Logging configured (Application Insights)
- [ ] Error tracking configured
- [ ] Performance monitoring configured
- [ ] Feature flags system implemented
- [ ] API documentation (Swagger) published

**Database**:
- [ ] Master database created and migrated
- [ ] Elastic pools configured for each tier
- [ ] Backup policies configured
- [ ] Point-in-time restore tested
- [ ] Geo-replication configured (if applicable)
- [ ] Connection pooling optimized
- [ ] Indexes reviewed and optimized

**Security**:
- [ ] SSL/TLS certificates configured
- [ ] JWT authentication tested
- [ ] Authorization policies tested
- [ ] Input validation implemented
- [ ] SQL injection prevention verified
- [ ] XSS protection verified
- [ ] CORS configured correctly
- [ ] Rate limiting implemented
- [ ] Security headers configured
- [ ] Penetration testing completed

**Operations**:
- [ ] Monitoring dashboards created
- [ ] Alert rules configured
- [ ] On-call rotation established
- [ ] Runbooks documented
- [ ] Incident response plan created
- [ ] Disaster recovery plan created
- [ ] Backup and restore procedures tested
- [ ] Tenant provisioning process automated

**Compliance**:
- [ ] Privacy policy published
- [ ] Terms of service published
- [ ] GDPR compliance verified
- [ ] Data retention policies implemented
- [ ] Right to delete implemented
- [ ] Audit logging implemented
- [ ] Compliance documentation completed

### First Week in Production

**Day 1**:
- [ ] Deploy to production
- [ ] Verify all services healthy
- [ ] Create first production tenant
- [ ] Test tenant provisioning end-to-end
- [ ] Monitor for errors

**Day 2-3**:
- [ ] Onboard 2-3 pilot customers
- [ ] Monitor performance metrics
- [ ] Collect user feedback
- [ ] Fix any critical issues

**Day 4-5**:
- [ ] Review logs and metrics
- [ ] Optimize based on production data
- [ ] Document any issues encountered
- [ ] Update runbooks

**Day 6-7**:
- [ ] Team retrospective
- [ ] Plan next sprint
- [ ] Begin gradual customer onboarding

### Scaling Milestones

**10 Tenants**:
- [ ] Review infrastructure costs
- [ ] Validate monitoring and alerting
- [ ] Customer feedback survey
- [ ] Performance baseline established

**50 Tenants**:
- [ ] Evaluate database pool utilization
- [ ] Review and optimize queries
- [ ] Cost optimization review
- [ ] Consider Phase 2 infrastructure upgrades

**100 Tenants**:
- [ ] Implement Phase 2 architecture
- [ ] Auto-scaling verification
- [ ] Multi-region planning
- [ ] Advanced monitoring implementation

**500 Tenants**:
- [ ] Kubernetes migration planning
- [ ] Database segmentation review
- [ ] Cost per tenant analysis
- [ ] SLA review and optimization

**1000+ Tenants**:
- [ ] Phase 3 architecture fully implemented
- [ ] Multi-region deployment
- [ ] Advanced automation for all operations
- [ ] Consider dedicated ops team

---

## Conclusion

This deployment strategy provides a clear path from initial MVP to a system capable of supporting 1000+ tenants. Key principles:

1. **Start Simple**: Get to production quickly with minimal infrastructure
2. **Scale Incrementally**: Evolve architecture based on actual needs
3. **Automate Everything**: Tenant provisioning, deployments, scaling, monitoring
4. **Monitor Proactively**: Know about issues before customers do
5. **Optimize Costs**: Track and optimize costs as you scale
6. **Prioritize Security**: Build security in from day one
7. **Plan for Failure**: Disaster recovery and backup strategies are critical

The phased approach allows you to:
- Validate the business model early
- Learn from real production usage
- Avoid over-engineering
- Scale infrastructure and costs in line with revenue
- Maintain high quality and reliability throughout growth

**Next Steps**:
1. Review and approve this strategy
2. Set up development and staging environments
3. Implement CI/CD pipeline
4. Create Terraform modules
5. Deploy Phase 1 infrastructure
6. Onboard first pilot customers
7. Iterate based on feedback
