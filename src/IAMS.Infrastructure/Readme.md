# IAMS Infrastructure Project

This project contains all infrastructure-related services and implementations for the Insurance Agency Management System (IAMS).

## Project Structure

```
IAMS.Infrastructure/
├── BackgroundServices/
│   ├── TenantAwareBackgroundService.cs      # Base class for tenant-aware background services
│   ├── PolicyReminderService.cs             # Sends policy renewal reminders
│   ├── IntegrationSyncService.cs            # Syncs data with insurance companies
│   ├── ReportSchedulerService.cs            # Processes scheduled reports
│   ├── ClaimProcessingService.cs            # Processes insurance claims
│   ├── DataCleanupService.cs                # Cleans up old data and logs
│   └── BackupService.cs                     # Creates system backups
├── Data/
│   └── IntegrationDbContext.cs              # Database context for infrastructure data
├── Extensions/
│   └── ServiceCollectionExtensions.cs      # DI container registration
├── Interfaces/
│   ├── IPolicyService.cs                   # Policy management operations
│   ├── IEmailService.cs                    # Email sending operations
│   ├── IFileStorageService.cs              # File storage operations
│   ├── IIntegrationService.cs              # Integration with insurance companies
│   └── IReportingService.cs                # Report generation and scheduling
└── Services/
    ├── EmailService.cs                     # SMTP email implementation
    ├── LocalFileStorageService.cs          # Local file storage implementation
    ├── AzureBlobStorageService.cs          # Azure blob storage implementation
    ├── IntegrationService.cs               # Integration service implementation
    ├── PolicyService.cs                    # Policy operations implementation
    ├── ModuleService.cs                    # Module enable/disable functionality
    └── ReportingService.cs                 # Report generation implementation
```

## Features

### Email Services
- **SMTP Email Support**: Send emails via SMTP providers (Gmail, Outlook, etc.)
- **Template Support**: HTML email templates for policy reminders and claim notifications
- **Bulk Email**: Send multiple emails efficiently
- **Turkish Language Support**: Email templates in Turkish for Turkish Cyprus market

### File Storage
- **Local Storage**: Store files on local file system
- **Azure Blob Storage**: Store files in Azure cloud (optional)
- **File Management**: Upload, download, delete, and list files
- **Metadata Tracking**: Track file information in database

### Integration Services
- **Insurance Company APIs**: Integrate with major Turkish insurance companies
- **Customer Data Sync**: Synchronize customer information
- **Policy Data Sync**: Synchronize policy information
- **Claim Submission**: Submit claims to insurance companies
- **Connection Testing**: Test API connections
- **Logging**: Track all integration activities

### Reporting Services
- **Report Generation**: Generate various insurance reports
- **Multiple Formats**: Export to PDF, Excel, CSV
- **Scheduled Reports**: Automatically generate and email reports
- **Turkish Reports**: Reports localized for Turkish Cyprus market

### Background Services
- **Policy Reminders**: Automatically send policy renewal reminders
- **Integration Sync**: Periodically sync data with insurance companies
- **Report Scheduling**: Process scheduled reports
- **Claim Processing**: Automatically process and submit claims
- **Data Cleanup**: Clean up old logs and temporary files
- **System Backup**: Create database and file backups

### Module Management
- **Feature Toggles**: Enable/disable modules per tenant
- **Premium Modules**: Accounting and Integration modules as paid add-ons
- **Core Modules**: Basic functionality always available

## Configuration

### Email Settings
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "IAMS Insurance Agency",
    "AgencyName": "IAMS Sigorta Acentesi",
    "ContactPhone": "+90 392 XXX XXXX",
    "ContactEmail": "info@iams-agency.com"
  }
}
```

### File Storage Settings
```json
{
  "FileStorage": {
    "Type": "local", // or "azure"
    "LocalPath": "C:\\IAMS\\Files",
    "BaseUrl": "/files",
    "AzureStorage": {
      "ConnectionString": "your-azure-storage-connection-string",
      "ContainerName": "iams-files"
    }
  }
}
```

### Module Configuration
```json
{
  "Modules": {
    "Reporting": true,      // Basic reporting (free)
    "Accounting": false,    // Premium module
    "Integration": false,   // Premium module
    "Claims": true,         // Core functionality
    "Policies": true,       // Core functionality
    "Customers": true       // Core functionality
  }
}
```

### Integration Providers
```json
{
  "IntegrationProviders": [
    {
      "Name": "axa-turkey",
      "DisplayName": "AXA Türkiye",
      "IsEnabled": false,
      "Settings": {
        "BaseUrl": "https://api.axa.com.tr",
        "AuthType": "bearer",
        "Token": "your-api-token"
      }
    }
  ]
}
```

## Multi-Tenancy Support

All services are tenant-aware and support multi-tenancy:

- **Tenant Isolation**: Each tenant has isolated data and configuration
- **Tenant-Specific Settings**: Email settings, file storage, and integrations per tenant
- **Background Services**: All background services work across all tenants
- **Module Toggles**: Each tenant can have different modules enabled

## Usage

### Register Infrastructure Services
```csharp
// In Program.cs or Startup.cs
services.AddInfrastructure(configuration);
services.AddInfrastructureBackgroundServices(); // Optional background services
```

### Using Services
```csharp
// Email Service
public class PolicyController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public PolicyController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send-reminder")]
    public async Task<IActionResult> SendReminder(int policyId)
    {
        await _emailService.SendPolicyReminderAsync(
            "customer@email.com",
            "Customer Name",
            "POL-2024-001",
            DateTime.Now.AddDays(30)
        );
        
        return Ok();
    }
}
```

### File Storage
```csharp
public class DocumentController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var filePath = await _fileStorage.UploadAsync(
            file.OpenReadStream(),
            file.FileName,
            "documents"
        );
        
        return Ok(new { path = filePath });
    }
}
```

### Integration Service
```csharp
public class CustomerController : ControllerBase
{
    private readonly IIntegrationService _integrationService;
    
    [HttpPost("{id}/sync")]
    public async Task<IActionResult> SyncCustomer(int id)
    {
        var result = await _integrationService.SyncCustomerDataAsync(id);
        return Ok(result);
    }
}
```

## Turkish Cyprus Specific Features

### Insurance Companies
- Pre-configured integration settings for major Turkish insurance companies
- AXA Türkiye, Allianz Türkiye, MAPFRE Türkiye support

### Localization
- Email templates in Turkish
- Turkish date formats (dd/MM/yyyy)
- Turkish currency support (TL)
- Turkish business terminology

### Legal Compliance
- Data retention policies compliant with Turkish regulations
- Audit logging for regulatory requirements
- Backup and disaster recovery procedures

## Dependencies

- **Microsoft.EntityFrameworkCore.SqlServer**: Database operations
- **Microsoft.Extensions.Hosting**: Background services
- **Microsoft.Extensions.Http**: HTTP client factory
- **System.Text.Json**: JSON serialization
- **IAMS.MultiTenancy**: Multi-tenancy support
- **IAMS.Application**: Application layer DTOs

## Future Enhancements

1. **Additional Storage Providers**: AWS S3, Google Cloud Storage
2. **More Integration Providers**: Additional Turkish insurance companies
3. **Advanced Reporting**: More sophisticated report templates
4. **SMS Integration**: Send SMS notifications
5. **Webhook Support**: Real-time notifications from insurance companies
6. **Document OCR**: Automatic document processing
7. **AI-Powered Claims**: Automated claim processing with AI

## Security Considerations

- All external API calls use secure authentication
- File uploads are validated and scanned
- Email templates are sanitized to prevent XSS
- Integration logs exclude sensitive data
- All database connections use encrypted connections