#!/bin/bash
#
# migrate-all-tenants.sh
# Migrates all active tenant databases to the latest schema version
#
# Usage: ./migrate-all-tenants.sh
#
# Environment variables required:
#   MASTER_CONNECTION_STRING - Connection string to master database
#   SLACK_WEBHOOK_URL (optional) - Slack webhook for notifications
#

set -e  # Exit on error
set -o pipefail  # Catch errors in pipes

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
MIGRATION_LOG="migration-$(date +%Y%m%d-%H%M%S).log"
RETRY_COUNT=3
RETRY_DELAY=5

# Counters
TOTAL_TENANTS=0
SUCCESSFUL_MIGRATIONS=0
FAILED_MIGRATIONS=0

# Functions
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$MIGRATION_LOG"
}

log_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$MIGRATION_LOG"
}

log_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1" | tee -a "$MIGRATION_LOG"
}

send_slack_notification() {
    local message="$1"
    local color="$2"  # good, warning, danger

    if [ -n "$SLACK_WEBHOOK_URL" ]; then
        curl -X POST "$SLACK_WEBHOOK_URL" \
            -H 'Content-Type: application/json' \
            -d "{\"attachments\":[{\"color\":\"$color\",\"text\":\"$message\"}]}" \
            2>&1 | tee -a "$MIGRATION_LOG" || true
    fi
}

get_tenant_list() {
    log "Fetching list of active tenants from master database..."

    # Query master database for active tenants
    # This assumes you have a tool to query SQL Server, or use dotnet tool
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- list-tenants \
        --active-only \
        --output json \
        --connection "$MASTER_CONNECTION_STRING" 2>&1 | tee -a "$MIGRATION_LOG"
}

migrate_tenant() {
    local tenant_json="$1"
    local tenant_id=$(echo "$tenant_json" | jq -r '.tenantId')
    local tenant_db=$(echo "$tenant_json" | jq -r '.databaseName')
    local tenant_name=$(echo "$tenant_json" | jq -r '.name')

    log "========================================="
    log "Migrating tenant: $tenant_name ($tenant_id)"
    log "Database: $tenant_db"
    log "========================================="

    # Get tenant-specific connection string
    local tenant_connection
    tenant_connection=$(dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- get-connection \
        --tenant-id "$tenant_id" \
        --connection "$MASTER_CONNECTION_STRING" 2>&1 | tee -a "$MIGRATION_LOG")

    if [ -z "$tenant_connection" ]; then
        log_error "Failed to retrieve connection string for tenant $tenant_id"
        return 1
    fi

    # Retry logic
    local attempt=1
    local success=false

    while [ $attempt -le $RETRY_COUNT ]; do
        log "Migration attempt $attempt of $RETRY_COUNT for tenant $tenant_id"

        if migrate_database "$tenant_connection" "$tenant_id"; then
            log "${GREEN}✓${NC} Successfully migrated $tenant_name ($tenant_id)"
            success=true
            break
        else
            log_error "Migration attempt $attempt failed for $tenant_id"
            if [ $attempt -lt $RETRY_COUNT ]; then
                log "Waiting $RETRY_DELAY seconds before retry..."
                sleep $RETRY_DELAY
            fi
            attempt=$((attempt + 1))
        fi
    done

    if [ "$success" = true ]; then
        return 0
    else
        log_error "${RED}✗${NC} CRITICAL: Migration failed for $tenant_name ($tenant_id) after $RETRY_COUNT attempts"
        send_slack_notification "⚠️ Database migration failed for tenant: $tenant_name ($tenant_id)" "danger"
        return 1
    fi
}

migrate_database() {
    local connection_string="$1"
    local tenant_id="$2"

    # Run EF Core migration
    dotnet ef database update \
        --project "$PROJECT_ROOT/src/IAMS.Persistence/IAMS.Persistence.csproj" \
        --startup-project "$PROJECT_ROOT/src/IAMS.Api/IAMS.Api.csproj" \
        --connection "$connection_string" \
        --verbose \
        2>&1 | tee -a "$MIGRATION_LOG"

    local exit_code=${PIPESTATUS[0]}

    if [ $exit_code -eq 0 ]; then
        # Verify migration was successful
        verify_migration "$connection_string" "$tenant_id"
        return $?
    else
        return $exit_code
    fi
}

verify_migration() {
    local connection_string="$1"
    local tenant_id="$2"

    log "Verifying migration for tenant $tenant_id..."

    # Check if __EFMigrationsHistory table exists and has entries
    # This is a simplified check - adjust based on your verification needs
    dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- verify-migration \
        --tenant-id "$tenant_id" \
        --connection "$connection_string" \
        2>&1 | tee -a "$MIGRATION_LOG"

    return $?
}

main() {
    log "========================================="
    log "Starting Tenant Database Migration Process"
    log "========================================="
    log "Migration log: $MIGRATION_LOG"

    # Check prerequisites
    if [ -z "$MASTER_CONNECTION_STRING" ]; then
        log_error "MASTER_CONNECTION_STRING environment variable is not set"
        exit 1
    fi

    # Check if dotnet CLI is available
    if ! command -v dotnet &> /dev/null; then
        log_error "dotnet CLI not found. Please install .NET SDK"
        exit 1
    fi

    # Check if jq is available
    if ! command -v jq &> /dev/null; then
        log_error "jq not found. Please install jq for JSON processing"
        exit 1
    fi

    # Get list of tenants
    local tenants_json
    tenants_json=$(get_tenant_list)

    if [ -z "$tenants_json" ] || [ "$tenants_json" = "[]" ]; then
        log_warning "No active tenants found"
        exit 0
    fi

    # Count total tenants
    TOTAL_TENANTS=$(echo "$tenants_json" | jq '. | length')
    log "Found $TOTAL_TENANTS active tenant(s)"

    # Send start notification
    send_slack_notification "🔄 Starting migration for $TOTAL_TENANTS tenant database(s)" "warning"

    # Iterate through each tenant
    echo "$tenants_json" | jq -c '.[]' | while read -r tenant_json; do
        if migrate_tenant "$tenant_json"; then
            SUCCESSFUL_MIGRATIONS=$((SUCCESSFUL_MIGRATIONS + 1))
        else
            FAILED_MIGRATIONS=$((FAILED_MIGRATIONS + 1))
        fi
    done

    # Summary
    log "========================================="
    log "Migration Process Completed"
    log "========================================="
    log "Total tenants: $TOTAL_TENANTS"
    log "Successful migrations: $SUCCESSFUL_MIGRATIONS"
    log "Failed migrations: $FAILED_MIGRATIONS"
    log "Log file: $MIGRATION_LOG"

    # Send completion notification
    if [ $FAILED_MIGRATIONS -eq 0 ]; then
        send_slack_notification "✅ All $TOTAL_TENANTS tenant databases migrated successfully!" "good"
        exit 0
    else
        send_slack_notification "⚠️ Migration completed with $FAILED_MIGRATIONS failure(s) out of $TOTAL_TENANTS tenants. Check logs for details." "danger"
        exit 1
    fi
}

# Run main function
main
