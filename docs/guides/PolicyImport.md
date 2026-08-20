# Policy Import and Endorsements Feature

## Overview

This document describes the implementation of policy and endorsement import functionality, along with enhanced payment tracking capabilities for the Insurance Agency Management System.

## Key Features Implemented

### 1. Endorsement Support (Zeyilname)

Endorsements are modifications or amendments to existing insurance policies. The system now supports:

- **Endorsement Tracking**: Each policy can have multiple endorsements
- **Automatic Numbering**: Endorsements are numbered sequentially (000, 001, 002, etc.)
- **Original Policy Reference**: Each endorsement maintains a link to its original policy
- **Branch Code**: External insurance type code (e.g., Trafik, Kasko, Yangin)

#### Database Fields Added to Policy Entity:
- `IsEndorsement` (bool): Indicates if this is an endorsement
- `EndorsementNumber` (string): Sequential number (000, 001, 002...)
- `OriginalPolicyId` (int?): Reference to the original policy
- `BranchCode` (string): External insurance type code

### 2. Marketer Tracking (Pazarlamacı)

The system now tracks marketers who bring customers to the agency:

- **Marketer Entity**: New entity to manage marketer information
- **Marketer Code and Name**: Unique identifier and display name
- **Commission Tracking**: Optional commission rate per marketer
- **Active Status**: Track active vs. inactive marketers

#### Marketer Entity Fields:
- `Code` (string): Unique marketer code (paz.kod)
- `Name` (string): Marketer name (pazarlamaci adi)
- `ContactPerson` (string): Contact person
- `PhoneNumber` (string): Phone number
- `Email` (string): Email address
- `CommissionRate` (decimal?): Optional commission percentage
- `IsActive` (bool): Active status

### 3. Driver Information

Enhanced vehicle insurance policies with driver-specific information:

- **Driver Age**: Age of the primary driver
- **Driver Type**: Single driver or any driver allowed

#### Database Fields:
- `DriverAge` (int?): Age of driver (YAS column in Excel)
- `DriverType` (enum): Single or Any

```csharp
public enum DriverType
{
    Single = 0,  // Single driver only
    Any = 1      // Any driver allowed
}
```

### 4. Payment History Tracking

The system already had payment tracking via the `PolicyPayment` entity. The implementation leverages this existing functionality:

#### Existing Features:
- **Payment Records**: Each payment is recorded as a separate `PolicyPayment` entity
- **Payment Status**: Pending, Completed, Failed
- **Payment Method**: Cash, Credit Card, Bank Transfer, Check
- **Historical Tracking**: All payments are timestamped and audited

#### Key Methods in Policy Entity:
```csharp
decimal GetTotalPremiumPaid()        // Total amount paid
decimal GetOutstandingPremium()      // Amount still owed
bool HasOverduePayments()            // Check for overdue payments
```

#### Example Payment Tracking:
1. Policy created with premium of $300
2. Initial payment of $100 recorded → Outstanding: $200
3. Second payment of $50 recorded → Outstanding: $150
4. Payment history maintained with all transactions

### 5. Excel Import Service

A comprehensive import service for policies and endorsements:

#### Import Service Features:
- **Excel File Parsing**: Read policies from Excel files
- **Validation**: Comprehensive validation before import
- **Error Handling**: Detailed error reporting per row
- **Automatic Entity Creation**: Auto-creates marketers, vehicles if needed
- **Endorsement Numbering**: Automatic sequential numbering
- **Payment Recording**: Records initial payments during import

#### Import Flow:
1. Parse Excel file into `ImportPolicyDto` objects
2. Validate each row (customer exists, amounts valid, dates valid)
3. Lookup or create related entities (marketer, vehicle, etc.)
4. For endorsements: Find original policy and generate endorsement number
5. Create policy entity
6. Create initial payment record if provided
7. Return detailed import results

#### Excel Column Mapping:

| Excel Column | Field | Description |
|--------------|-------|-------------|
| Column A (Kod) | BranchCode | External insurance type code |
| Policy Number | PolicyNumber | Unique policy identifier |
| Column 7 | EndorsementNumber | 000 for first, 001, 002... |
| YAS | DriverAge | Driver age |
| Sürücü | DriverType | Single or Any |
| paz.kod | MarketerCode | Marketer code |
| pazarlamaci adi | MarketerName | Marketer name |
| Premium Amount | PremiumAmount | Total premium |
| Paid Amount | PaidAmount | Initial payment amount |

## Database Schema Changes

### New Tables

#### Marketers Table
```sql
CREATE TABLE Marketers (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(200),
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    CommissionRate DECIMAL(5,2),
    IsActive BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(1000),
    -- Audit fields
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME2,
    UpdatedBy NVARCHAR(100),
    UpdatedDate DATETIME2,
    IsDeleted BIT DEFAULT 0
);
```

### Modified Tables

#### Policies Table - New Columns
```sql
ALTER TABLE Policies ADD IsEndorsement BIT NOT NULL DEFAULT 0;
ALTER TABLE Policies ADD EndorsementNumber NVARCHAR(10);
ALTER TABLE Policies ADD OriginalPolicyId INT;
ALTER TABLE Policies ADD BranchCode NVARCHAR(50);
ALTER TABLE Policies ADD DriverAge INT;
ALTER TABLE Policies ADD DriverType INT;
ALTER TABLE Policies ADD MarketerId INT;

ALTER TABLE Policies ADD CONSTRAINT FK_Policies_OriginalPolicy
    FOREIGN KEY (OriginalPolicyId) REFERENCES Policies(Id);

ALTER TABLE Policies ADD CONSTRAINT FK_Policies_Marketer
    FOREIGN KEY (MarketerId) REFERENCES Marketers(Id);
```

## API Endpoints (To Be Implemented)

### Policy Import
```
POST /api/policies/import
- Uploads Excel file and imports policies/endorsements
- Returns import results with success/failure counts
```

### Validation
```
POST /api/policies/validate-import
- Validates Excel file without importing
- Returns validation errors
```

### Preview
```
POST /api/policies/preview-import
- Parses Excel and returns preview of data
- No database changes
```

### Endorsements
```
GET /api/policies/{policyId}/endorsements
- Returns all endorsements for a policy

GET /api/policies/{policyId}/endorsements/count
- Returns endorsement count
```

### Marketers
```
GET /api/marketers
- List all marketers

POST /api/marketers
- Create new marketer

GET /api/marketers/{id}
- Get marketer details

PUT /api/marketers/{id}
- Update marketer

GET /api/marketers/{id}/policies
- Get all policies for a marketer
```

## Usage Examples

### Creating an Endorsement Manually
```csharp
// Find the original policy
var originalPolicy = await _policyRepository.GetByPolicyNumberAsync("POL-2024-001");

// Generate endorsement number
var endorsements = await _policyRepository.GetEndorsementsByOriginalPolicyIdAsync(originalPolicy.Id);
var nextNumber = endorsements.Count; // 0, 1, 2, etc.
var endorsementNumber = nextNumber.ToString("000"); // "000", "001", "002"

// Create endorsement
var endorsement = new Policy
{
    PolicyNumber = originalPolicy.PolicyNumber, // Same as original
    IsEndorsement = true,
    EndorsementNumber = endorsementNumber,
    OriginalPolicyId = originalPolicy.Id,
    // ... copy other fields from original policy
    // ... apply changes for the endorsement
};

await _policyRepository.AddAsync(endorsement);
```

### Tracking Payment History
```csharp
// Get policy
var policy = await _policyRepository.GetByIdAsync(policyId);

// Check payment status
var totalPaid = policy.GetTotalPremiumPaid();
var outstanding = policy.GetOutstandingPremium();
var hasOverdue = policy.HasOverduePayments();

// Get payment history
var payments = await _policyPaymentRepository.GetByPolicyIdAsync(policyId);
foreach (var payment in payments)
{
    Console.WriteLine($"{payment.PaymentDate}: {payment.Amount} - {payment.Status}");
}

// Add new payment
var payment = new PolicyPayment
{
    PolicyId = policy.Id,
    Amount = 50.00m,
    PaymentDate = DateTime.Today,
    PaymentMethod = PaymentMethod.Cash,
    Status = PaymentStatus.Completed,
    CurrencyId = policy.CurrencyId
};

await _policyPaymentRepository.AddAsync(payment);
```

### Importing Policies from Excel
```csharp
var importService = serviceProvider.GetRequiredService<IPolicyImportService>();

// Import from file
var result = await importService.ImportFromExcelAsync(
    filePath: "policies.xlsx",
    userId: "admin"
);

Console.WriteLine($"Total Rows: {result.TotalRows}");
Console.WriteLine($"Success: {result.SuccessCount}");
Console.WriteLine($"Failed: {result.FailureCount}");

foreach (var error in result.Errors)
{
    Console.WriteLine($"Row {error.RowNumber}: {error.ErrorMessage}");
}
```

## Important Notes

1. **Endorsements Are Import-Only**: Endorsements should not be created from the UI, only imported from Excel files.

2. **Payment Tracking**: The existing `PolicyPayment` system tracks all payments historically. Each payment is a separate record with timestamp, method, and status.

3. **Marketer Auto-Creation**: During import, if a marketer code doesn't exist, it will be automatically created.

4. **Customer Pre-Requisite**: Customers must exist in the system before importing policies. The import will fail if a customer cannot be found.

5. **Excel Library**: The implementation requires the ClosedXML NuGet package for Excel file processing:
   ```xml
   <PackageReference Include="ClosedXML" Version="0.102.1" />
   ```

6. **Endorsement Numbering**: Endorsements start at 000 (not 001). This follows Turkish insurance industry standards.

## Migration Steps

To apply these changes to your database:

```bash
# Create migration
dotnet ef migrations add AddEndorsementsAndMarketers -p src/IAMS.Persistence -s src/IAMS.Web

# Apply migration
dotnet ef database update -p src/IAMS.Persistence -s src/IAMS.Web
```

## Next Steps

1. **Add ClosedXML Package**: Add the NuGet package reference to IAMS.Application.csproj
2. **Implement Excel Parsing**: Complete the `ParseExcelAsync` method in `PolicyImportService`
3. **Create Import UI**: Build Blazor components for file upload and import
4. **Add API Controllers**: Create endpoints for import functionality
5. **Create Marketer Management UI**: Build CRUD pages for marketers
6. **Add Endorsement Views**: Display endorsements on policy detail pages
7. **Enhance Payment Tracking UI**: Show payment history and outstanding amounts

## Security Considerations

1. **File Upload**: Validate Excel file size and format
2. **Import Permissions**: Restrict import functionality to authorized users only
3. **Data Validation**: Always validate imported data before database insertion
4. **Audit Logging**: Log all import operations with user information
5. **Transaction Handling**: Use database transactions for import operations

## Testing Recommendations

1. **Unit Tests**: Test endorsement numbering logic
2. **Integration Tests**: Test import with sample Excel files
3. **Payment Tests**: Verify payment calculation methods
4. **Validation Tests**: Test import validation rules
5. **Performance Tests**: Test import with large Excel files

## Support

For questions or issues, please refer to the main project documentation or contact the development team.

## Agency code per insurance company (MySQL import)

Every insurance company assigns the agency its **own** agency code — the `ackod` value in the
insurer's policy database. The MySQL policy import filters on that code
(`@agencyCodeStart/@agencyCodeEnd` in `MySqlPolicyImportService.BuildPolicyQuery`), so the value
must be correct *per insurer*.

- Set it in the admin panel: **Acentalar → Sigorta Şirketleri → Düzenle → "Acenta Kodu"**
  (stored in `AgencyInsuranceCompanies.AgencyCode`, master DB; migration `sql/005_AddAgencyCodeToAgencyInsuranceCompanies.sql`).
- Format: 1–10 letters/digits (`IAMS.Shared.Validation.AgencyCodeRules`). Anything else is rejected by the API.
- Links with no code fall back to the agency-level `Tenants.ExternalId` (the old single-code behaviour)
  and log a warning. If neither is set, preview/import fails fast with a clear message instead of querying.
- `DbServer` must include the MySQL port (e.g. `host:3306` or `host:23306`); without a MySQL port the
  server is treated as SQL Server.
