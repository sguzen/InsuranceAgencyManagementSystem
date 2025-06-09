#!/bin/bash
# IAMS Git Repository Cleanup Script
# This script removes already committed files that should be ignored

echo "🧹 Starting IAMS Repository Cleanup..."
echo "⚠️  WARNING: This will remove files from Git tracking. Make sure you have backups!"
echo ""

# Function to confirm before proceeding
confirm() {
    read -p "Do you want to continue? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Operation cancelled."
        exit 1
    fi
}

# Step 1: Show what files will be affected
echo "📋 Step 1: Checking what files are currently tracked that should be ignored..."
echo ""

# Check for common unwanted files
echo "🔍 Checking for build artifacts (bin/, obj/ folders):"
git ls-files | grep -E "(bin/|obj/)" | head -10
echo ""

echo "🔍 Checking for IDE files (.vs/, .idea/, *.user, *.suo):"
git ls-files | grep -E "(\\.vs/|\\.idea/|.*\\.user$|.*\\.suo$)" | head -10
echo ""

echo "🔍 Checking for log files (*.log):"
git ls-files | grep -E ".*\\.log$" | head -10
echo ""

echo "🔍 Checking for package files (packages/, node_modules/):"
git ls-files | grep -E "(packages/|node_modules/)" | head -5
echo ""

echo "🔍 Checking for configuration files that might contain secrets:"
git ls-files | grep -E "(appsettings\\.(Production|Staging|Local)\\.json|connectionstrings\\.json)" | head -10
echo ""

confirm

# Step 2: Remove files from Git tracking
echo "🗑️  Step 2: Removing unwanted files from Git tracking..."
echo ""

# Remove build artifacts
echo "Removing bin/ and obj/ folders..."
git rm -r --cached */bin/ 2>/dev/null || true
git rm -r --cached */obj/ 2>/dev/null || true
git rm -r --cached **/bin/ 2>/dev/null || true
git rm -r --cached **/obj/ 2>/dev/null || true

# Remove IDE files
echo "Removing IDE files..."
git rm -r --cached .vs/ 2>/dev/null || true
git rm -r --cached .idea/ 2>/dev/null || true
git rm --cached *.user 2>/dev/null || true
git rm --cached *.suo 2>/dev/null || true
git rm --cached **/*.user 2>/dev/null || true
git rm --cached **/*.suo 2>/dev/null || true

# Remove log files
echo "Removing log files..."
git rm --cached *.log 2>/dev/null || true
git rm --cached **/*.log 2>/dev/null || true
git rm -r --cached logs/ 2>/dev/null || true
git rm -r --cached **/logs/ 2>/dev/null || true

# Remove package folders
echo "Removing package directories..."
git rm -r --cached packages/ 2>/dev/null || true
git rm -r --cached node_modules/ 2>/dev/null || true
git rm -r --cached **/packages/ 2>/dev/null || true

# Remove potentially sensitive config files
echo "Removing sensitive configuration files..."
git rm --cached appsettings.Production.json 2>/dev/null || true
git rm --cached appsettings.Staging.json 2>/dev/null || true
git rm --cached appsettings.Local.json 2>/dev/null || true
git rm --cached **/appsettings.Production.json 2>/dev/null || true
git rm --cached **/appsettings.Staging.json 2>/dev/null || true
git rm --cached **/appsettings.Local.json 2>/dev/null || true
git rm --cached connectionstrings.json 2>/dev/null || true
git rm --cached **/connectionstrings.json 2>/dev/null || true

# Remove database files
echo "Removing database files..."
git rm --cached *.db 2>/dev/null || true
git rm --cached *.sqlite 2>/dev/null || true
git rm --cached **/*.db 2>/dev/null || true
git rm --cached **/*.sqlite 2>/dev/null || true

# Remove certificate files
echo "Removing certificate files..."
git rm --cached *.pfx 2>/dev/null || true
git rm --cached *.p12 2>/dev/null || true
git rm --cached *.crt 2>/dev/null || true
git rm --cached *.key 2>/dev/null || true

echo ""
echo "✅ Files removed from Git tracking."

# Step 3: Update the repository
echo "📝 Step 3: Adding .gitignore and committing changes..."

# Add the .gitignore file
git add .gitignore

# Commit the cleanup
git commit -m "🧹 Clean up repository: Remove build artifacts, IDE files, and sensitive configs

- Remove bin/ and obj/ directories
- Remove IDE configuration files (.vs/, .idea/, *.user, *.suo)
- Remove log files and package directories
- Remove sensitive configuration files
- Add comprehensive .gitignore for IAMS project
- Prepare repository for clean multi-tenant development"

echo ""
echo "🎉 Repository cleanup complete!"
echo ""
echo "📋 Next steps:"
echo "1. Review the changes with: git status"
echo "2. Push changes to remote: git push origin main"
echo "3. Let your team know about the cleanup"
echo "4. Create template config files for development"
echo ""
echo "💡 Consider creating example configuration files:"
echo "   - appsettings.Development.example.json"
echo "   - connectionstrings.example.json"
echo "   - docker-compose.override.example.yml"