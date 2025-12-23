#!/bin/bash
#
# backup-tenant.sh
# Creates a backup of a tenant database
#
# Usage: ./backup-tenant.sh <tenant-id> [backup-type]
#
# backup-type: full (default), differential, log
#

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
BACKUP_DIR="${BACKUP_DIR:-/var/backups/iams}"
LOG_FILE="backup-$(date +%Y%m%d-%H%M%S).log"

# Arguments
TENANT_ID="$1"
BACKUP_TYPE="${2:-full}"

if [ -z "$TENANT_ID" ]; then
    echo -e "${RED}Error: Missing tenant ID${NC}"
    echo "Usage: $0 <tenant-id> [backup-type]"
    echo "backup-type: full (default), differential, log"
    exit 1
fi

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$LOG_FILE"
}

backup_tenant() {
    log "========================================="
    log "Starting Tenant Database Backup"
    log "========================================="
    log "Tenant ID: $TENANT_ID"
    log "Backup Type: $BACKUP_TYPE"

    # Get tenant database info
    local tenant_info
    tenant_info=$(dotnet run --project "$PROJECT_ROOT/tools/IAMS.Admin.CLI" -- get-tenant \
        --tenant-id "$TENANT_ID" \
        --connection "$MASTER_CONNECTION_STRING" \
        --output json 2>&1 | tee -a "$LOG_FILE")

    local db_name
    db_name=$(echo "$tenant_info" | jq -r '.databaseName')

    if [ -z "$db_name" ] || [ "$db_name" = "null" ]; then
        log_error "Failed to get database name for tenant $TENANT_ID"
        exit 1
    fi

    log "Database: $db_name"

    # Create backup directory if it doesn't exist
    mkdir -p "$BACKUP_DIR"

    # Backup filename
    local backup_file
    backup_file="$BACKUP_DIR/${db_name}_${BACKUP_TYPE}_$(date +%Y%m%d_%H%M%S).bak"

    log "Backup file: $backup_file"

    # Perform backup using Azure CLI (adjust for your cloud provider)
    if command -v az &> /dev/null; then
        case "$BACKUP_TYPE" in
            full)
                log "Creating full backup..."
                # Azure automatically creates backups, but you can trigger manual backup
                az sql db export \
                    --resource-group "$AZURE_RESOURCE_GROUP" \
                    --server "$AZURE_SQL_SERVER" \
                    --name "$db_name" \
                    --storage-key "$AZURE_STORAGE_KEY" \
                    --storage-key-type "StorageAccessKey" \
                    --storage-uri "${AZURE_STORAGE_URI}/${db_name}_$(date +%Y%m%d_%H%M%S).bacpac" \
                    --admin-user "$AZURE_SQL_ADMIN" \
                    --admin-password "$AZURE_SQL_PASSWORD" \
                    2>&1 | tee -a "$LOG_FILE"
                ;;
            differential|log)
                log_error "Differential and log backups are managed automatically by Azure SQL"
                log "Use point-in-time restore instead"
                exit 1
                ;;
            *)
                log_error "Invalid backup type: $BACKUP_TYPE"
                exit 1
                ;;
        esac
    else
        log_error "Azure CLI not found. Cannot perform backup."
        exit 1
    fi

    log "========================================="
    log "${GREEN}✓ Backup Completed Successfully${NC}"
    log "========================================="
    log "Tenant ID: $TENANT_ID"
    log "Database: $db_name"
    log "Backup Type: $BACKUP_TYPE"
    log "Log file: $LOG_FILE"
}

# Check prerequisites
if [ -z "$MASTER_CONNECTION_STRING" ]; then
    log_error "MASTER_CONNECTION_STRING environment variable is not set"
    exit 1
fi

# Run backup
backup_tenant
