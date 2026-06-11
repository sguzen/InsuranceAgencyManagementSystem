# Calculation Architecture Documentation

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Policy Premium Calculations](#policy-premium-calculations)
- [Commission Calculations](#commission-calculations)
- [Claim Calculations](#claim-calculations)
- [Usage Examples](#usage-examples)
- [Adding New Policy Types](#adding-new-policy-types)
- [Configuration](#configuration)
  - [Commission Rate Configuration](#commission-rate-configuration)
  - [External Premium Calculation Service](#external-premium-calculation-service)
- [API Reference](#api-reference)

## Overview

The Insurance Agency Management System uses a **Strategy Pattern** based calculation architecture to handle different types of insurance policies, commissions, and claims. This architecture provides:

- **Type-specific calculations**: Each policy type (Traffic, Kasko, Life, Property, etc.) has its own calculator
- **Database-driven commission rates**: Commission rates are looked up from the database based on policy type and insurance company
- **Automatic discount application**: No-claim and fleet discounts are applied automatically
- **Claim validation and deductibles**: Claims are validated against policy coverage and deductibles are applied

## Architecture

### Component Overview

```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚                   Application Layer                          â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚                                                               â”‚
â”‚  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”   â”‚
â”‚  â”‚     PolicyCalculatorFactory                          â”‚   â”‚
â”‚  â”‚     (Resolves appropriate calculator)                â”‚   â”‚
â”‚  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜   â”‚
â”‚                   â”‚                                           â”‚
â”‚         â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”    â”‚
â”‚         â–¼                    â–¼              â–¼         â–¼     â”‚
â”‚  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”    â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”€â”â”‚
â”‚  â”‚ Traffic  â”‚    â”‚    Kasko      â”‚  â”‚   Life   â”‚  â”‚ Prop â”‚â”‚â”‚
â”‚  â”‚Calculatorâ”‚    â”‚  Calculator   â”‚  â”‚Calculatorâ”‚  â”‚ ...  â”‚â”‚â”‚
â”‚  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜    â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”˜â”‚â”‚
â”‚                                                               â”‚
â”‚  â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”      â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”  â”‚
â”‚  â”‚ CommissionCalculator â”‚      â”‚   ClaimCalculator      â”‚  â”‚
â”‚  â”‚ (Database lookup)    â”‚      â”‚   (Deductibles, etc)   â”‚  â”‚
â”‚  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜      â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Key Components

| Component | Location | Purpose |
|-----------|----------|---------|
| **IPolicyPremiumCalculator** | `Services/Calculations/IPolicyPremiumCalculator.cs` | Interface for policy-specific premium calculators |
| **ICommissionCalculator** | `Services/Calculations/ICommissionCalculator.cs` | Interface for commission calculations |
| **IClaimCalculator** | `Services/Calculations/IClaimCalculator.cs` | Interface for claim validations and calculations |
| **IPolicyCalculatorFactory** | `Services/Calculations/IPolicyCalculatorFactory.cs` | Factory to resolve appropriate calculator |

## Policy Premium Calculations

### Supported Policy Types

The system supports 13 policy types across 5 categories:

#### 1. Vehicle Insurance
- **TRF** (Trafik SigortasÄ± - Traffic Insurance)
  - Calculator: `TrafficInsurancePremiumCalculator`
  - Factors: Vehicle age, engine power, vehicle type
  - Location: `Services/Calculations/TrafficInsurancePremiumCalculator.cs`

- **KAS, MINKAS** (Kasko - Comprehensive Insurance)
  - Calculator: `KaskoInsurancePremiumCalculator`
  - Factors: Vehicle market value, vehicle age, optional coverages
  - Optional coverages: Glass, Theft, Natural Disaster, Driver Accident
  - Location: `Services/Calculations/KaskoInsurancePremiumCalculator.cs`

#### 2. Property Insurance
- **KNT** (Konut - Home), **DASK** (Earthquake), **ISY** (Ä°ÅŸyeri - Business)
  - Calculator: `PropertyInsurancePremiumCalculator`
  - Factors: Coverage period, property value (when available)
  - Location: `Services/Calculations/PropertyInsurancePremiumCalculator.cs`

#### 3. Health Insurance
- **TSS** (TamamlayÄ±cÄ± SaÄŸlÄ±k), **OSS** (Ã–zel SaÄŸlÄ±k), **SEY** (Seyahat - Travel)
  - Calculator: `HealthInsurancePremiumCalculator`
  - Factors: Coverage period, coverage type
  - Location: `Services/Calculations/HealthInsurancePremiumCalculator.cs`

#### 4. Life Insurance
- **HAY** (Hayat - Life), **FK** (Ferdi Kaza - Personal Accident)
  - Calculator: `LifeInsurancePremiumCalculator`
  - Factors: Term length (supports up to 20 years)
  - Location: `Services/Calculations/LifeInsurancePremiumCalculator.cs`

#### 5. Liability Insurance
- **MMS** (Mesleki Mali Sorumluluk), **USS** (ÃœÃ§Ã¼ncÃ¼ ÅžahÄ±s)
  - Calculator: `LiabilityInsurancePremiumCalculator`
  - Factors: Coverage period, business type
  - Location: `Services/Calculations/LiabilityInsurancePremiumCalculator.cs`

### Discount Application

All calculators inherit from `BasePolicyPremiumCalculator` which provides common discount logic:

#### No-Claim Discount
```csharp
// Applied when policy has no-claim history
if (policy.NoClaimDiscountRate.HasValue && policy.NoClaimDiscountRate.Value > 0)
{
    var discount = basePremium * (policy.NoClaimDiscountRate.Value / 100);
    discountedPremium -= discount;
}
```

**Fields:**
- `NoClaimDiscountRate`: Percentage discount (e.g., 10 for 10%)
- `NoClaimYears`: Number of claim-free years

#### Fleet Discount
```csharp
// Applied for multiple vehicles under same policy holder
if (policy.FleetDiscountRate.HasValue && policy.FleetDiscountRate.Value > 0)
{
    var discount = basePremium * (policy.FleetDiscountRate.Value / 100);
    discountedPremium -= discount;
}
```

**Field:**
- `FleetDiscountRate`: Percentage discount for fleet policies

### Calculation Flow

```
1. Check if premium already set â†’ Use as base
2. Calculate base premium (type-specific logic)
3. Adjust for payment frequency/term
4. Apply no-claim discount
5. Apply fleet discount
6. Add optional coverages (for Kasko)
7. Return final premium
```

## Commission Calculations

### Database-Driven Lookup

The `CommissionCalculator` looks up commission rates from the `CommissionRate` table:

**Location:** `Services/Calculations/CommissionCalculator.cs`

```csharp
var commissionRate = await _commissionRateRepository
    .GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId);
```

### Fallback Mechanism

If no rate is configured in the database:
- **Default Rate:** 10%
- **Logging:** Warning logged with policy type and company IDs

### Commission Formula

```
Commission Amount = Premium Amount Ã— (Commission Rate / 100)
```

### Example

```csharp
// Policy: TRF (Traffic)
// Company: ABC Insurance
// Premium: 1,500 TL

// Database lookup: TRF + ABC Insurance = 12% commission rate
// Commission Amount = 1,500 Ã— (12 / 100) = 180 TL
```

## Claim Calculations

### Validation Steps

**Location:** `Services/Calculations/ClaimCalculator.cs`

#### 1. Policy Eligibility Check
```csharp
bool IsPolicyEligibleForClaim(Policy policy, DateTime claimDate)
{
    // Must be Active status
    if (policy.Status != PolicyStatus.Active) return false;

    // Claim date must be within coverage period
    if (claimDate < policy.StartDate || claimDate > policy.EndDate)
        return false;

    return true;
}
```

#### 2. Coverage Limit Validation
```csharp
bool ValidateClaimAmount(Policy policy, decimal claimedAmount)
{
    var maxClaimable = GetMaximumClaimableAmount(policy);
    var deductible = policy.DeductibleAmount ?? 0;
    var potentialPayable = claimedAmount - deductible;

    return potentialPayable <= maxClaimable;
}
```

#### 3. Payable Amount Calculation
```csharp
decimal CalculatePayableAmount(Policy policy, decimal claimedAmount)
{
    // Apply deductible
    var deductible = policy.DeductibleAmount ?? 0;
    var payableAmount = Math.Max(0, claimedAmount - deductible);

    // Ensure within max coverage
    var maxClaimable = GetMaximumClaimableAmount(policy);
    return Math.Min(payableAmount, maxClaimable);
}
```

### Maximum Claimable Amount

Default logic:
```
Max Claimable = Premium Amount Ã— 1.5

For policies with Driver Accident Coverage:
Max Claimable = (Premium Amount Ã— 1.5) + Driver Accident Coverage Amount
```

### Deductible Application

**Field:** `Policy.DeductibleAmount` (typically used for Kasko policies)

**Example:**
```
Claimed Amount:  5,000 TL
Deductible:      500 TL
Payable Amount:  4,500 TL
```

## Usage Examples

### Example 1: Creating a Policy with Automatic Calculations

```csharp
// In CreatePolicyCommandHandler
public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, ...)
{
    var policy = _mapper.Map<Policy>(request.PolicyDto);

    // 1. Generate policy number
    policy.PolicyNumber = await _policyNumberGenerator.GenerateAsync(
        policy.InsuranceCompanyId,
        policy.PolicyTypeId);

    // 2. Calculate premium (if not already set)
    if (policy.PremiumAmount <= 0)
    {
        var premiumCalculator = await _calculatorFactory
            .GetCalculatorForPolicyAsync(policy);
        policy.PremiumAmount = await premiumCalculator
            .CalculatePremiumAsync(policy);
    }

    // 3. Calculate commission from database
    var (commissionAmount, commissionRate) = await _commissionCalculator
        .CalculateCommissionAsync(
            policy.PolicyTypeId,
            policy.InsuranceCompanyId,
            policy.PremiumAmount);

    policy.CommissionAmount = commissionAmount;
    policy.CommissionRate = commissionRate;

    await _unitOfWork.Policies.AddAsync(policy);
    await _unitOfWork.SaveChangesAsync();
}
```

### Example 2: Creating a Claim with Validation

```csharp
// In CreateClaimCommandHandler
public async Task<Result<PolicyClaimDto>> Handle(CreateClaimCommand request, ...)
{
    var policy = await _unitOfWork.Policies.GetByIdAsync(request.ClaimDto.PolicyId);
    var claim = _mapper.Map<PolicyClaim>(request.ClaimDto);

    // 1. Validate policy eligibility
    if (!_claimCalculator.IsPolicyEligibleForClaim(policy, claim.ClaimDate))
    {
        return Result.ValidationFailure("Policy is not eligible for claims");
    }

    // 2. Validate claim amount
    if (!_claimCalculator.ValidateClaimAmount(policy, claim.ClaimAmount))
    {
        var maxClaimable = _claimCalculator.GetMaximumClaimableAmount(policy);
        return Result.ValidationFailure(
            $"Claim exceeds maximum coverage of {maxClaimable:C}");
    }

    // 3. Calculate payable amount (after deductible)
    claim.ApprovedAmount = _claimCalculator.CalculatePayableAmount(
        policy,
        claim.ClaimAmount);

    await _unitOfWork.PolicyClaims.AddAsync(claim);
    await _unitOfWork.SaveChangesAsync();
}
```

### Example 3: Updating a Policy with Recalculation

```csharp
// In UpdatePolicyCommandHandler
public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, ...)
{
    var existingPolicy = await _unitOfWork.Policies.GetByIdAsync(request.Id);

    // Store original values
    var originalPolicyTypeId = existingPolicy.PolicyTypeId;
    var originalCompanyId = existingPolicy.InsuranceCompanyId;

    // Map updates
    _mapper.Map(request.PolicyDto, existingPolicy);

    // Recalculate if type or company changed
    if (originalPolicyTypeId != existingPolicy.PolicyTypeId ||
        originalCompanyId != existingPolicy.InsuranceCompanyId)
    {
        // Recalculate premium
        var calculator = await _calculatorFactory
            .GetCalculatorForPolicyAsync(existingPolicy);
        existingPolicy.PremiumAmount = await calculator
            .CalculatePremiumAsync(existingPolicy);

        // Recalculate commission
        var (amount, rate) = await _commissionCalculator
            .CalculateCommissionAsync(
                existingPolicy.PolicyTypeId,
                existingPolicy.InsuranceCompanyId,
                existingPolicy.PremiumAmount);

        existingPolicy.CommissionAmount = amount;
        existingPolicy.CommissionRate = rate;
    }

    await _unitOfWork.SaveChangesAsync();
}
```

## Adding New Policy Types

To add a new policy type calculator:

### Step 1: Create Calculator Class

```csharp
// File: Services/Calculations/NewPolicyTypeCalculator.cs
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    public class NewPolicyTypeCalculator : BasePolicyPremiumCalculator
    {
        public NewPolicyTypeCalculator(
            ILogger<NewPolicyTypeCalculator> logger) : base(logger)
        {
        }

        public override string PolicyTypeCode => "NEW"; // Policy type code

        public override async Task<decimal> CalculatePremiumAsync(Policy policy)
        {
            if (!ValidateForCalculation(policy))
            {
                throw new InvalidOperationException("Invalid policy data");
            }

            // Use existing premium if set
            if (policy.PremiumAmount > 0)
            {
                return ApplyDiscounts(policy.PremiumAmount, policy);
            }

            // Your custom calculation logic here
            decimal basePremium = CalculateBasePremium(policy);

            // Apply discounts (inherited from base)
            var finalPremium = ApplyDiscounts(basePremium, policy);

            return await Task.FromResult(finalPremium);
        }

        private decimal CalculateBasePremium(Policy policy)
        {
            // Your business logic here
            decimal premium = 1000m;

            // Adjust for term length
            var termMonths = GetTermLengthInMonths(policy);
            premium = premium * (termMonths / 12m);

            return premium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            // Use base validation
            if (!ValidateBasicPolicyData(policy))
                return false;

            // Add your custom validations
            return true;
        }
    }
}
```

### Step 2: Register in DI Container

```csharp
// File: Extensions/ServiceCollectionExtensions.cs

// Add to the calculator registrations
services.AddScoped<IPolicyPremiumCalculator, NewPolicyTypeCalculator>();
```

### Step 3: Map in Factory

```csharp
// File: Services/Calculations/PolicyCalculatorFactory.cs

private readonly Dictionary<string, Type> _calculatorMapping = new()
{
    // ... existing mappings ...
    { "NEW", typeof(NewPolicyTypeCalculator) }, // Add your mapping
};
```

### Step 4: Add Policy Type to Database

```sql
INSERT INTO PolicyTypes (Code, Name, Description, Category, IsActive)
VALUES ('NEW', 'New Policy Type Name', 'Description', 'Category', 1);
```

## Configuration

### Commission Rate Configuration

Commission rates are stored in the `CommissionRates` table:

```sql
-- Example: Set 15% commission for Traffic Insurance with ABC Company
INSERT INTO CommissionRates (PolicyTypeId, InsuranceCompanyId, Rate, IsActive)
VALUES (
    (SELECT Id FROM PolicyTypes WHERE Code = 'TRF'),
    (SELECT Id FROM InsuranceCompanies WHERE Code = 'ABC'),
    15.00,  -- 15% commission
    1       -- Active
);
```

### Viewing Current Configuration

```sql
-- View all commission rates
SELECT
    pt.Code AS PolicyType,
    pt.Name AS PolicyTypeName,
    ic.Code AS CompanyCode,
    ic.Name AS CompanyName,
    cr.Rate AS CommissionRate,
    cr.IsActive
FROM CommissionRates cr
JOIN PolicyTypes pt ON cr.PolicyTypeId = pt.Id
JOIN InsuranceCompanies ic ON cr.InsuranceCompanyId = ic.Id
WHERE cr.IsActive = 1
ORDER BY pt.Code, ic.Code;
```

### External Premium Calculation Service

The system supports routing premium calculations to an external service for specific policy types. This is useful when certain calculations need to be handled by external systems or third-party services.

#### Configuration

Configure external calculation in `appsettings.json`:

```json
{
  "PremiumCalculation": {
    "ExternalCalculationPolicyTypes": [ "TRF" ],
    "FallbackToInternalOnFailure": true,
    "ExternalService": {
      "Enabled": false,
      "BaseUrl": "https://external-calculator-service.example.com",
      "CalculationEndpoint": "/api/premium/calculate",
      "HealthCheckEndpoint": "/api/health",
      "TimeoutSeconds": 30,
      "ApiKey": "",
      "MaxRetryAttempts": 3
    }
  }
}
```

#### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `ExternalCalculationPolicyTypes` | Array of policy type codes that should use external calculation (e.g., ["TRF", "KAS"]) | `[]` |
| `FallbackToInternalOnFailure` | If true, uses internal calculator when external service fails | `true` |
| `ExternalService.Enabled` | Master switch to enable/disable external service | `false` |
| `ExternalService.BaseUrl` | Base URL of the external calculation service | - |
| `ExternalService.CalculationEndpoint` | API endpoint for premium calculation | `/api/premium/calculate` |
| `ExternalService.HealthCheckEndpoint` | Health check endpoint | `/api/health` |
| `ExternalService.TimeoutSeconds` | Request timeout in seconds | `30` |
| `ExternalService.ApiKey` | API key for authentication (if required) | - |
| `ExternalService.MaxRetryAttempts` | Maximum retry attempts for failed requests | `3` |

#### How It Works

1. **PolicyCalculatorFactory** resolves the appropriate internal calculator based on policy type
2. If external calculation is configured for that policy type, the calculator is wrapped with **RoutingPremiumCalculator**
3. **RoutingPremiumCalculator** checks configuration and routes the calculation:
   - If `ExternalService.Enabled` is `true` AND policy type is in `ExternalCalculationPolicyTypes`, calls external service
   - If external service fails AND `FallbackToInternalOnFailure` is `true`, uses internal calculator
   - Otherwise, uses internal calculator directly

#### Example: Enable External Calculation for Traffic Insurance

```json
{
  "PremiumCalculation": {
    "ExternalCalculationPolicyTypes": [ "TRF" ],
    "FallbackToInternalOnFailure": true,
    "ExternalService": {
      "Enabled": true,
      "BaseUrl": "https://traffic-calculator.example.com",
      "ApiKey": "your-api-key-here",
      "TimeoutSeconds": 15
    }
  }
}
```

#### External Service Request Format

The external service should accept POST requests with this JSON structure:

```json
{
  "policyTypeCode": "TRF",
  "policyId": 123,
  "policyNumber": "POL-2024-001",
  "premiumAmount": 0.00,
  "deductibleAmount": 500.00,
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-12-31T23:59:59",
  "noClaimDiscountRate": 10.0,
  "fleetDiscountRate": 5.0,
  "driverAccidentCoverageAmount": 10000,
  "vehicleData": {
    "vehicleId": 456,
    "plateNumber": "ABC123",
    "modelYear": 2020,
    "currentValue": 25000.00,
    "enginePower": 150
  },
  "hasGlassCoverage": true,
  "hasTheftCoverage": false,
  "hasNaturalDisasterCoverage": false,
  "hasDriverAccidentCoverage": true
}
```

#### External Service Response Format

The external service should return this JSON structure:

```json
{
  "success": true,
  "premiumAmount": 1250.50,
  "errorMessage": null,
  "breakDown": {
    "basePremium": 1000.00,
    "ageSurcharge": 100.00,
    "powerSurcharge": 75.00,
    "discounts": -150.00,
    "glassCoverage": 225.50
  }
}
```

#### Monitoring and Logging

The routing calculator logs the following events:
- When external calculation is attempted
- When external calculation succeeds/fails
- When fallback to internal calculator occurs

Example log output:
```
[INFO] Wrapping calculator TrafficInsurancePremiumCalculator with routing for policy type 'TRF'
[INFO] Attempting external calculation for policy type 'TRF'
[INFO] Successfully calculated premium using external service: 1250.50
```

Or in case of fallback:
```
[ERROR] External calculation failed for policy type 'TRF'
[WARN] Falling back to internal calculator for policy type 'TRF'
```

#### Security Considerations

- Store API keys in **User Secrets** (development) or **Azure Key Vault** (production), not in appsettings.json
- Use HTTPS for external service communication
- Implement request/response validation
- Set appropriate timeout values to prevent long-running requests
- Monitor external service availability and fallback usage

Example using User Secrets:
```bash
dotnet user-secrets set "PremiumCalculation:ExternalService:ApiKey" "your-secret-api-key"
```

## API Reference

### IPolicyPremiumCalculator

**Location:** `Services/Calculations/IPolicyPremiumCalculator.cs`

#### Properties
```csharp
string PolicyTypeCode { get; }  // e.g., "TRF", "KAS", "HAY"
```

#### Methods
```csharp
// Calculate premium for a policy
Task<decimal> CalculatePremiumAsync(Policy policy);

// Apply discounts to base premium
decimal ApplyDiscounts(decimal basePremium, Policy policy);

// Validate policy data for calculation
bool ValidateForCalculation(Policy policy);
```

### ICommissionCalculator

**Location:** `Services/Calculations/ICommissionCalculator.cs`

#### Methods
```csharp
// Calculate commission amount and rate
Task<(decimal CommissionAmount, decimal CommissionRate)> CalculateCommissionAsync(
    int policyTypeId,
    int insuranceCompanyId,
    decimal premiumAmount);

// Get commission rate for policy type and company
Task<decimal?> GetCommissionRateAsync(
    int policyTypeId,
    int insuranceCompanyId);
```

### IClaimCalculator

**Location:** `Services/Calculations/IClaimCalculator.cs`

#### Methods
```csharp
// Calculate payable amount after deductibles
decimal CalculatePayableAmount(Policy policy, decimal claimedAmount);

// Validate claim amount against coverage limits
bool ValidateClaimAmount(Policy policy, decimal claimedAmount);

// Get maximum claimable amount for policy
decimal GetMaximumClaimableAmount(Policy policy);

// Check if policy is eligible for claims
bool IsPolicyEligibleForClaim(Policy policy, DateTime claimDate);
```

### IPolicyCalculatorFactory

**Location:** `Services/Calculations/IPolicyCalculatorFactory.cs`

#### Methods
```csharp
// Get calculator by policy type code
IPolicyPremiumCalculator GetCalculator(string policyTypeCode);

// Get calculator for a policy entity
Task<IPolicyPremiumCalculator> GetCalculatorForPolicyAsync(Policy policy);
```

### BasePolicyPremiumCalculator

**Location:** `Services/Calculations/BasePolicyPremiumCalculator.cs`

Base class for all premium calculators providing common functionality.

#### Protected Helper Methods
```csharp
// Calculate term length in months
protected int GetTermLengthInMonths(Policy policy);

// Validate basic policy data
protected bool ValidateBasicPolicyData(Policy policy);

// Apply discounts (virtual, can be overridden)
public virtual decimal ApplyDiscounts(decimal basePremium, Policy policy);
```

## Best Practices

### 1. Always Use Calculators for New Code

âŒ **Don't:**
```csharp
// Old way - manual calculation
policy.CommissionAmount = policy.PremiumAmount * (policy.CommissionRate / 100);
```

âœ… **Do:**
```csharp
// New way - use calculator service
var (commissionAmount, commissionRate) = await _commissionCalculator
    .CalculateCommissionAsync(
        policy.PolicyTypeId,
        policy.InsuranceCompanyId,
        policy.PremiumAmount);

policy.CommissionAmount = commissionAmount;
policy.CommissionRate = commissionRate;
```

### 2. Validate Before Calculating

```csharp
var calculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);

if (!calculator.ValidateForCalculation(policy))
{
    throw new ValidationException("Policy data is incomplete");
}

var premium = await calculator.CalculatePremiumAsync(policy);
```

### 3. Log Calculations for Audit Trail

```csharp
_logger.LogInformation(
    "Premium calculated for policy {PolicyNumber}: Type={Type}, Premium={Premium}",
    policy.PolicyNumber, policyType.Code, premium);
```

### 4. Handle Missing Commission Rates Gracefully

The `CommissionCalculator` automatically falls back to 10% default rate and logs a warning. Monitor these warnings to identify missing configurations.

### 5. Test with Real Data

Before deploying, ensure commission rates are configured for all:
- Active policy types
- Active insurance companies
- Common policy type + company combinations

## Migration Notes

### Existing Policies

For policies created before this architecture:
- They retain their original commission rates
- On update, they will use the new calculation system
- No automatic recalculation is performed on existing policies

### Gradual Migration

To migrate existing policies to use database commission rates:

```sql
-- Update policies to use current database commission rates
UPDATE p
SET
    p.CommissionRate = cr.Rate,
    p.CommissionAmount = p.PremiumAmount * (cr.Rate / 100)
FROM Policies p
JOIN CommissionRates cr ON
    cr.PolicyTypeId = p.PolicyTypeId AND
    cr.InsuranceCompanyId = p.InsuranceCompanyId
WHERE cr.IsActive = 1;
```

## Troubleshooting

### Issue: Commission rate is 10% instead of expected rate

**Cause:** No commission rate configured in database for policy type + company combination

**Solution:**
1. Check `CommissionRates` table for missing entries
2. Add missing commission rate configuration
3. Update the policy to trigger recalculation

### Issue: Premium calculation returns 0 or very low amount

**Cause:** Missing required data (e.g., vehicle information for car insurance)

**Solution:**
1. Check calculator validation logs
2. Ensure all required fields are populated
3. Provide a manual premium if automatic calculation isn't possible

### Issue: Claim validation fails unexpectedly

**Cause:** Policy status not Active or claim date outside coverage period

**Solution:**
1. Verify `Policy.Status = Active`
2. Verify `claim.ClaimDate` is between `Policy.StartDate` and `Policy.EndDate`
3. Check policy eligibility using `IClaimCalculator.IsPolicyEligibleForClaim()`

## Performance Considerations

### Database Queries
- Commission rate lookup uses indexed query on `PolicyTypeId` and `InsuranceCompanyId`
- Consider caching frequently used commission rates

### Async Operations
- All calculators use async/await for database operations
- Premium calculations are CPU-bound but quick (<10ms typically)

### Logging
- Logging is set to Information level for calculations
- Production environments may want to reduce to Warning level for performance

---

## Summary

The calculation architecture provides:
âœ… Type-specific premium calculations
âœ… Database-driven commission rates
âœ… Automatic discount application
âœ… Claim validation with deductibles
âœ… Easy extensibility for new policy types
âœ… Comprehensive logging and audit trail

For questions or issues, please refer to the inline code documentation or contact the development team.



---


# Calculation Services - Quick Reference

## Quick Start

### Inject Services in Your Handler

```csharp
public class YourCommandHandler
{
    private readonly IPolicyCalculatorFactory _calculatorFactory;
    private readonly ICommissionCalculator _commissionCalculator;
    private readonly IClaimCalculator _claimCalculator;

    public YourCommandHandler(
        IPolicyCalculatorFactory calculatorFactory,
        ICommissionCalculator commissionCalculator,
        IClaimCalculator claimCalculator)
    {
        _calculatorFactory = calculatorFactory;
        _commissionCalculator = commissionCalculator;
        _claimCalculator = claimCalculator;
    }
}
```

## Common Tasks

### 1. Calculate Policy Premium

```csharp
// Get the right calculator for this policy type
var calculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);

// Calculate premium
var premium = await calculator.CalculatePremiumAsync(policy);

policy.PremiumAmount = premium;
```

### 2. Calculate Commission

```csharp
// Automatically looks up rate from database
var (commissionAmount, commissionRate) = await _commissionCalculator.CalculateCommissionAsync(
    policy.PolicyTypeId,
    policy.InsuranceCompanyId,
    policy.PremiumAmount);

policy.CommissionAmount = commissionAmount;
policy.CommissionRate = commissionRate;
```

### 3. Validate and Calculate Claim

```csharp
// Check if policy is eligible
if (!_claimCalculator.IsPolicyEligibleForClaim(policy, claim.ClaimDate))
{
    return Error("Policy is not eligible for claims");
}

// Validate claim amount
if (!_claimCalculator.ValidateClaimAmount(policy, claim.ClaimAmount))
{
    var maxClaimable = _claimCalculator.GetMaximumClaimableAmount(policy);
    return Error($"Claim exceeds maximum coverage of {maxClaimable:C}");
}

// Calculate payable amount (after deductibles)
claim.ApprovedAmount = _claimCalculator.CalculatePayableAmount(policy, claim.ClaimAmount);
```

## Policy Type Codes

| Code | Policy Type | Calculator |
|------|-------------|------------|
| **TRF** | Traffic Insurance | `TrafficInsurancePremiumCalculator` |
| **KAS** | Kasko (Comprehensive) | `KaskoInsurancePremiumCalculator` |
| **MINKAS** | Mini Kasko | `KaskoInsurancePremiumCalculator` |
| **KNT** | Home Insurance | `PropertyInsurancePremiumCalculator` |
| **DASK** | Earthquake Insurance | `PropertyInsurancePremiumCalculator` |
| **ISY** | Business Insurance | `PropertyInsurancePremiumCalculator` |
| **TSS** | Supplementary Health | `HealthInsurancePremiumCalculator` |
| **OSS** | Private Health | `HealthInsurancePremiumCalculator` |
| **SEY** | Travel Insurance | `HealthInsurancePremiumCalculator` |
| **HAY** | Life Insurance | `LifeInsurancePremiumCalculator` |
| **FK** | Personal Accident | `LifeInsurancePremiumCalculator` |
| **MMS** | Professional Liability | `LiabilityInsurancePremiumCalculator` |
| **USS** | Third Party Liability | `LiabilityInsurancePremiumCalculator` |

## Important Fields

### For Premium Calculation

**Vehicle Insurance:**
- `VehicleId` - Required for vehicle-based calculations
- `DeductibleAmount` - For Kasko policies
- `HasGlassCoverage`, `HasTheftCoverage`, etc. - Optional coverages

**Discounts:**
- `NoClaimDiscountRate` - Percentage (e.g., 10 for 10%)
- `NoClaimYears` - Years without claims
- `FleetDiscountRate` - Percentage for fleet policies

**All Policies:**
- `StartDate`, `EndDate` - Coverage period
- `PolicyTypeId`, `InsuranceCompanyId` - For lookups

### For Commission Calculation

- `PolicyTypeId` - Required
- `InsuranceCompanyId` - Required
- `PremiumAmount` - Required

### For Claim Validation

- `DeductibleAmount` - Amount subtracted from claim
- `Status` - Must be `Active`
- `StartDate`, `EndDate` - Must cover claim date

## Formulas

### Premium with Discounts
```
Base Premium
- No-Claim Discount (Base Ã— NoClaimDiscountRate / 100)
- Fleet Discount (Base Ã— FleetDiscountRate / 100)
= Final Premium
```

### Commission
```
Commission Amount = Premium Amount Ã— (Commission Rate / 100)
```

### Claim Payable Amount
```
Payable = MIN(
    MAX(0, Claimed Amount - Deductible),
    Maximum Claimable Amount
)
```

## Configuration

### Set Commission Rate in Database

```sql
INSERT INTO CommissionRates (PolicyTypeId, InsuranceCompanyId, Rate, IsActive, EffectiveDate)
VALUES (
    (SELECT Id FROM PolicyTypes WHERE Code = 'TRF'),  -- Policy Type
    (SELECT Id FROM InsuranceCompanies WHERE Code = 'ABC'),  -- Company
    15.00,  -- 15% commission
    1,      -- Active
    GETDATE()
);
```

### View Current Rates

```sql
SELECT
    pt.Code AS PolicyType,
    ic.Code AS Company,
    cr.Rate AS Commission
FROM CommissionRates cr
JOIN PolicyTypes pt ON cr.PolicyTypeId = pt.Id
JOIN InsuranceCompanies ic ON cr.InsuranceCompanyId = ic.Id
WHERE cr.IsActive = 1;
```

### Enable External Calculation Service

To route specific policy types (e.g., Traffic) to an external calculation service, configure in `appsettings.json`:

```json
{
  "PremiumCalculation": {
    "ExternalCalculationPolicyTypes": [ "TRF" ],
    "FallbackToInternalOnFailure": true,
    "ExternalService": {
      "Enabled": true,
      "BaseUrl": "https://external-service.example.com",
      "ApiKey": "your-api-key",
      "TimeoutSeconds": 30
    }
  }
}
```

**Note:** Store API keys securely using User Secrets (dev) or Azure Key Vault (production):
```bash
dotnet user-secrets set "PremiumCalculation:ExternalService:ApiKey" "your-key"
```

See [External Premium Calculation Service](./CALCULATION_ARCHITECTURE.md#external-premium-calculation-service) for full details.

## Validation Helpers

### Check Calculator Exists for Policy Type

```csharp
try
{
    var calculator = _calculatorFactory.GetCalculator(policyTypeCode);
    // Calculator exists
}
catch (InvalidOperationException)
{
    // No calculator registered for this policy type
}
```

### Validate Policy Data Before Calculation

```csharp
var calculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);

if (!calculator.ValidateForCalculation(policy))
{
    // Policy data is incomplete
    throw new ValidationException("Policy data is incomplete for calculation");
}
```

## Common Patterns

### Creating a Policy

```csharp
// 1. Generate policy number
policy.PolicyNumber = await _policyNumberGenerator.GenerateAsync(...);

// 2. Calculate premium (if not manually entered)
if (policy.PremiumAmount <= 0)
{
    var calculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);
    policy.PremiumAmount = await calculator.CalculatePremiumAsync(policy);
}

// 3. Calculate commission
var (amount, rate) = await _commissionCalculator.CalculateCommissionAsync(...);
policy.CommissionAmount = amount;
policy.CommissionRate = rate;

// 4. Save
await _unitOfWork.Policies.AddAsync(policy);
await _unitOfWork.SaveChangesAsync();
```

### Updating a Policy

```csharp
// Store original values
var originalTypeId = policy.PolicyTypeId;
var originalCompanyId = policy.InsuranceCompanyId;

// Apply updates
_mapper.Map(updateDto, policy);

// Recalculate if type or company changed
if (originalTypeId != policy.PolicyTypeId ||
    originalCompanyId != policy.InsuranceCompanyId)
{
    // Recalculate premium
    var calculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);
    policy.PremiumAmount = await calculator.CalculatePremiumAsync(policy);

    // Recalculate commission
    var (amount, rate) = await _commissionCalculator.CalculateCommissionAsync(...);
    policy.CommissionAmount = amount;
    policy.CommissionRate = rate;
}

await _unitOfWork.SaveChangesAsync();
```

### Processing a Claim

```csharp
var policy = await _unitOfWork.Policies.GetByIdAsync(claim.PolicyId);

// 1. Validate eligibility
if (!_claimCalculator.IsPolicyEligibleForClaim(policy, claim.ClaimDate))
{
    return ValidationError("Policy not eligible");
}

// 2. Validate amount
if (!_claimCalculator.ValidateClaimAmount(policy, claim.ClaimAmount))
{
    return ValidationError("Claim amount too high");
}

// 3. Calculate payable
claim.ApprovedAmount = _claimCalculator.CalculatePayableAmount(
    policy,
    claim.ClaimAmount);

// 4. Save
await _unitOfWork.PolicyClaims.AddAsync(claim);
await _unitOfWork.SaveChangesAsync();
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Commission is always 10% | Commission rate not in database - add to `CommissionRates` table |
| Premium is 0 or very low | Missing required data (e.g., vehicle info) - check validation logs |
| Claim validation fails | Check policy status is `Active` and claim date is in coverage period |
| Calculator not found | Policy type code not mapped in `PolicyCalculatorFactory` |

## File Locations

```
src/IAMS.Application/Services/Calculations/
â”œâ”€â”€ Interfaces
â”‚   â”œâ”€â”€ IPolicyPremiumCalculator.cs
â”‚   â”œâ”€â”€ ICommissionCalculator.cs
â”‚   â”œâ”€â”€ IClaimCalculator.cs
â”‚   â””â”€â”€ IPolicyCalculatorFactory.cs
â”œâ”€â”€ Core
â”‚   â”œâ”€â”€ BasePolicyPremiumCalculator.cs
â”‚   â”œâ”€â”€ CommissionCalculator.cs
â”‚   â”œâ”€â”€ ClaimCalculator.cs
â”‚   â””â”€â”€ PolicyCalculatorFactory.cs
â””â”€â”€ Calculators
    â”œâ”€â”€ TrafficInsurancePremiumCalculator.cs
    â”œâ”€â”€ KaskoInsurancePremiumCalculator.cs
    â”œâ”€â”€ PropertyInsurancePremiumCalculator.cs
    â”œâ”€â”€ HealthInsurancePremiumCalculator.cs
    â”œâ”€â”€ LifeInsurancePremiumCalculator.cs
    â””â”€â”€ LiabilityInsurancePremiumCalculator.cs
```

## Need More Details?

See the full documentation: [CALCULATION_ARCHITECTURE.md](./CALCULATION_ARCHITECTURE.md)



---


