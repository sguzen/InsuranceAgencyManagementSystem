@echo off
echo ===============================================
echo          IAMS Database Setup Script
echo ===============================================
echo.


echo ===============================================
echo Creating Master Database Migration...
echo ===============================================
dotnet ef migrations add InitialMasterDatabase --project src/IAMS.MultiTenancy --startup-project src/IAMS.Web --context TenantDbContext --output-dir Migrations
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to create Master Database migration
    pause
    exit /b 1
)
echo.

echo ===============================================
echo Creating Master Database...
echo ===============================================
dotnet ef database update --project src/IAMS.MultiTenancy --startup-project src/IAMS.Web --context TenantDbContext
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to create Master Database
    pause
    exit /b 1
)
echo.

echo ===============================================
echo Creating Application Database Migration...
echo ===============================================
dotnet ef migrations add InitialApplicationDatabase --project src/IAMS.Infrastructure --startup-project src/IAMS.Web --context IntegrationDbContext --output-dir Migrations
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to create Application Database migration
    pause
    exit /b 1
)
echo.

echo ===============================================
echo Creating Application Database...
echo ===============================================
dotnet ef database update --project src/IAMS.Infrastructure --startup-project src/IAMS.Web --context IntegrationDbContext
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to create Application Database
    pause
    exit /b 1
)
echo.

echo ===============================================
echo              Setup Complete!
echo ===============================================
echo.
echo The following databases have been created:
echo - IAMS_Master (tenant management)
echo - IAMS_Demo (demo agency data)
echo.
echo You can now run your IAMS application:
echo   dotnet run --project src/IAMS.Web
echo.
pause