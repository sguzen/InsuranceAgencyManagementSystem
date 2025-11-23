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
- No-Claim Discount (Base × NoClaimDiscountRate / 100)
- Fleet Discount (Base × FleetDiscountRate / 100)
= Final Premium
```

### Commission
```
Commission Amount = Premium Amount × (Commission Rate / 100)
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
├── Interfaces
│   ├── IPolicyPremiumCalculator.cs
│   ├── ICommissionCalculator.cs
│   ├── IClaimCalculator.cs
│   └── IPolicyCalculatorFactory.cs
├── Core
│   ├── BasePolicyPremiumCalculator.cs
│   ├── CommissionCalculator.cs
│   ├── ClaimCalculator.cs
│   └── PolicyCalculatorFactory.cs
└── Calculators
    ├── TrafficInsurancePremiumCalculator.cs
    ├── KaskoInsurancePremiumCalculator.cs
    ├── PropertyInsurancePremiumCalculator.cs
    ├── HealthInsurancePremiumCalculator.cs
    ├── LifeInsurancePremiumCalculator.cs
    └── LiabilityInsurancePremiumCalculator.cs
```

## Need More Details?

See the full documentation: [CALCULATION_ARCHITECTURE.md](./CALCULATION_ARCHITECTURE.md)
