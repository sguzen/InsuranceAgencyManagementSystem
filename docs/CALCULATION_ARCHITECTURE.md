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
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │     PolicyCalculatorFactory                          │   │
│  │     (Resolves appropriate calculator)                │   │
│  └────────────────┬────────────────────────────────────┘   │
│                   │                                           │
│         ┌─────────┴──────────┬──────────────┬─────────┐    │
│         ▼                    ▼              ▼         ▼     │
│  ┌──────────┐    ┌───────────────┐  ┌──────────┐  ┌──────┐│
│  │ Traffic  │    │    Kasko      │  │   Life   │  │ Prop │││
│  │Calculator│    │  Calculator   │  │Calculator│  │ ...  │││
│  └──────────┘    └───────────────┘  └──────────┘  └──────┘││
│                                                               │
│  ┌──────────────────────┐      ┌────────────────────────┐  │
│  │ CommissionCalculator │      │   ClaimCalculator      │  │
│  │ (Database lookup)    │      │   (Deductibles, etc)   │  │
│  └──────────────────────┘      └────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
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
- **TRF** (Trafik Sigortası - Traffic Insurance)
  - Calculator: `TrafficInsurancePremiumCalculator`
  - Factors: Vehicle age, engine power, vehicle type
  - Location: `Services/Calculations/TrafficInsurancePremiumCalculator.cs`

- **KAS, MINKAS** (Kasko - Comprehensive Insurance)
  - Calculator: `KaskoInsurancePremiumCalculator`
  - Factors: Vehicle market value, vehicle age, optional coverages
  - Optional coverages: Glass, Theft, Natural Disaster, Driver Accident
  - Location: `Services/Calculations/KaskoInsurancePremiumCalculator.cs`

#### 2. Property Insurance
- **KNT** (Konut - Home), **DASK** (Earthquake), **ISY** (İşyeri - Business)
  - Calculator: `PropertyInsurancePremiumCalculator`
  - Factors: Coverage period, property value (when available)
  - Location: `Services/Calculations/PropertyInsurancePremiumCalculator.cs`

#### 3. Health Insurance
- **TSS** (Tamamlayıcı Sağlık), **OSS** (Özel Sağlık), **SEY** (Seyahat - Travel)
  - Calculator: `HealthInsurancePremiumCalculator`
  - Factors: Coverage period, coverage type
  - Location: `Services/Calculations/HealthInsurancePremiumCalculator.cs`

#### 4. Life Insurance
- **HAY** (Hayat - Life), **FK** (Ferdi Kaza - Personal Accident)
  - Calculator: `LifeInsurancePremiumCalculator`
  - Factors: Term length (supports up to 20 years)
  - Location: `Services/Calculations/LifeInsurancePremiumCalculator.cs`

#### 5. Liability Insurance
- **MMS** (Mesleki Mali Sorumluluk), **USS** (Üçüncü Şahıs)
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
1. Check if premium already set → Use as base
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
Commission Amount = Premium Amount × (Commission Rate / 100)
```

### Example

```csharp
// Policy: TRF (Traffic)
// Company: ABC Insurance
// Premium: 1,500 TL

// Database lookup: TRF + ABC Insurance = 12% commission rate
// Commission Amount = 1,500 × (12 / 100) = 180 TL
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
Max Claimable = Premium Amount × 1.5

For policies with Driver Accident Coverage:
Max Claimable = (Premium Amount × 1.5) + Driver Accident Coverage Amount
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

❌ **Don't:**
```csharp
// Old way - manual calculation
policy.CommissionAmount = policy.PremiumAmount * (policy.CommissionRate / 100);
```

✅ **Do:**
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
✅ Type-specific premium calculations
✅ Database-driven commission rates
✅ Automatic discount application
✅ Claim validation with deductibles
✅ Easy extensibility for new policy types
✅ Comprehensive logging and audit trail

For questions or issues, please refer to the inline code documentation or contact the development team.
