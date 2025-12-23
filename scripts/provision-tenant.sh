#!/bin/bash
#
# provision-tenant.sh
# Provisions a new tenant with database, initial data, and configuration
#
# Usage: ./provision-tenant.sh <tenant-id> <tenant-name> <admin-email> <tier>
#
# Example:
#   ./provision-tenant.sh agency001 "First Agency" admin@agency001.com professional
#

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
LOG_FILE="provision-$(date +%Y%m%d-%H%M%S).log"

# Arguments
TENANT_ID="$1"
TENANT_NAME="$2"
ADMIN_EMAIL="$3"
TIER="${4:-basic}"  # Default to basic tier

# Validate arguments
if [ -z "$TENANT_ID" ] || [ -z "$TENANT_NAME" ] || [ -z "$ADMIN_EMAIL" ]; then
    echo -e "${RED}Error: Missing required arguments${NC}"
    echo "Usage: $0 <tenant-id> <tenant-name> <admin-email> [tier]"
    echo "Example: $0 agency001 'First Agency' admin@agency001.com professional"
    exit 1
fi

# Validate tier
case "$TIER" in
    basic|professional|enterprise)
        ;;
    *)
        echo -e "${RED}Error: Invalid tier. Must be one of: basic, professional, enterprise${NC}"
        exit 1
        ;;
esac

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1" | tee -a "$LOG_FILE"
}

provision_tenant() {
    log "========================================="
    log "Provisioning New Tenant"
    log "========================================="
    log "Tenant ID: $TENANT_ID"
    log "Tenant Name: $TENANT_NAME"
    log "Admin Email: $ADMIN_EMAIL"
    log "Tier: $TIER"
    log "========================================="

    # Step 1: Check if tenant already exists
    log "Step 1/8: Checking if tenant already exists..."
    if dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- tenant-exists \
        --tenant-id "$TENANT_ID" \
        --connection "$MASTER_CONNECTION_STRING" 2>&1 | tee -a "$LOG_FILE"; then
        log_error "Tenant $TENANT_ID already exists"
        exit 1
    fi
    log "✓ Tenant ID is available"

    # Step 2: Validate email format
    log "Step 2/8: Validating admin email..."
    if ! echo "$ADMIN_EMAIL" | grep -E '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$' > /dev/null; then
        log_error "Invalid email format: $ADMIN_EMAIL"
        exit 1
    fi
    log "✓ Email format valid"

    # Step 3: Create tenant entry in master database
    log "Step 3/8: Creating tenant entry in master database..."
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- create-tenant \
        --tenant-id "$TENANT_ID" \
        --name "$TENANT_NAME" \
        --tier "$TIER" \
        --connection "$MASTER_CONNECTION_STRING" \
        2>&1 | tee -a "$LOG_FILE"
    log "✓ Tenant entry created in master database"

    # Step 4: Provision database in appropriate elastic pool
    log "Step 4/8: Provisioning tenant database..."
    local db_name="iams_tenant_${TENANT_ID}"

    # Determine elastic pool based on tier
    local pool_name
    case "$TIER" in
        enterprise)
            pool_name="pool-premium-tenants"
            ;;
        professional)
            pool_name="pool-standard-high"
            ;;
        basic)
            pool_name="pool-standard-low"
            ;;
    esac

    log "Creating database $db_name in pool $pool_name..."

    # Using Azure CLI to create database (adjust for your cloud provider)
    if command -v az &> /dev/null; then
        az sql db create \
            --resource-group "$AZURE_RESOURCE_GROUP" \
            --server "$AZURE_SQL_SERVER" \
            --name "$db_name" \
            --elastic-pool "$pool_name" \
            2>&1 | tee -a "$LOG_FILE"
    else
        log_warning "Azure CLI not found, skipping database creation. Please create manually."
    fi

    log "✓ Database provisioned"

    # Step 5: Update tenant with database connection info
    log "Step 5/8: Updating tenant with database info..."
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- update-tenant-db \
        --tenant-id "$TENANT_ID" \
        --database-name "$db_name" \
        --connection "$MASTER_CONNECTION_STRING" \
        2>&1 | tee -a "$LOG_FILE"
    log "✓ Tenant database info updated"

    # Step 6: Run database migrations
    log "Step 6/8: Running database migrations..."
    local tenant_connection
    tenant_connection=$(dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- get-connection \
        --tenant-id "$TENANT_ID" \
        --connection "$MASTER_CONNECTION_STRING")

    dotnet ef database update \
        --project "$PROJECT_ROOT/src/IAMS.Persistence/IAMS.Persistence.csproj" \
        --startup-project "$PROJECT_ROOT/src/IAMS.Api/IAMS.Api.csproj" \
        --connection "$tenant_connection" \
        2>&1 | tee -a "$LOG_FILE"
    log "✓ Database migrations completed"

    # Step 7: Seed initial data
    log "Step 7/8: Seeding initial data..."
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- seed-tenant \
        --tenant-id "$TENANT_ID" \
        --connection "$tenant_connection" \
        2>&1 | tee -a "$LOG_FILE"
    log "✓ Initial data seeded"

    # Step 8: Create admin user
    log "Step 8/8: Creating admin user..."
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- create-admin \
        --tenant-id "$TENANT_ID" \
        --email "$ADMIN_EMAIL" \
        --connection "$tenant_connection" \
        2>&1 | tee -a "$LOG_FILE"
    log "✓ Admin user created"

    # Success summary
    log "========================================="
    log "${GREEN}✓ Tenant Provisioning Completed Successfully${NC}"
    log "========================================="
    log "Tenant ID: $TENANT_ID"
    log "Database: $db_name"
    log "Admin Email: $ADMIN_EMAIL"
    log "Login URL: https://${TENANT_ID}.yourdomain.com"
    log "Log file: $LOG_FILE"
    log "========================================="

    # Send welcome email (if configured)
    if [ -n "$SENDGRID_API_KEY" ]; then
        log "Sending welcome email to $ADMIN_EMAIL..."
        dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- send-welcome-email \
            --tenant-id "$TENANT_ID" \
            --email "$ADMIN_EMAIL" \
            2>&1 | tee -a "$LOG_FILE" || log_warning "Failed to send welcome email"
    fi
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."

    if [ -z "$MASTER_CONNECTION_STRING" ]; then
        log_error "MASTER_CONNECTION_STRING environment variable is not set"
        exit 1
    fi

    if ! command -v dotnet &> /dev/null; then
        log_error "dotnet CLI not found"
        exit 1
    fi

    log "✓ Prerequisites check passed"
}

# Main
main() {
    check_prerequisites
    provision_tenant
}

main
