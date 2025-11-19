# Large Component Refactoring Guide

## Problem: God Components

Several Blazor components in the application violate the Single Responsibility Principle by being too large and handling too many concerns.

### Large Components Identified

| Component | Lines | Issues |
|-----------|-------|--------|
| PolicyForm.razor | 902 | Contains 6 distinct sections, complex validation, multiple services |
| FinancialReports.razor | 663 | Multiple report types in one component |
| ExpiringPolicies.razor | 654 | Complex filtering, sorting, data display |
| PoliciesList.razor | 649 | Table, filters, search, pagination in one file |
| CustomerForm.razor | 631 | Similar to PolicyForm - multiple sections |

---

## Refactoring Strategy: PolicyForm.razor

### Current Structure (902 lines)

**Single monolithic component with 6 sections:**
1. Basic Information (Status, Policy Type) - ~50 lines
2. Customer & Company Selection - ~90 lines
3. Vehicle Information (conditional) - ~80 lines
4. Policy Period (Start/End dates) - ~40 lines
5. Financial Information (Premium, Commission) - ~70 lines
6. Additional Information (Notes) - ~30 lines
7. Form submission logic - ~100 lines
8. Code-behind (@code block) - ~442 lines

### Refactored Structure (Recommended)

**Main component + 6 child components:**

```
PolicyForm.razor (150 lines) - Orchestrator
├── PolicyBasicInfoSection.razor (80 lines)
├── PolicyCustomerSection.razor (120 lines)
├── PolicyVehicleSection.razor (100 lines)
├── PolicyPeriodSection.razor (70 lines)
├── PolicyFinancialSection.razor (90 lines)
└── PolicyAdditionalInfoSection.razor (60 lines)
```

**Total lines remain similar, but:**
- Each component has **single responsibility**
- Components are **reusable**
- **Easier to test** individual sections
- **Better maintainability**
- **Clearer code organization**

---

## Implementation Plan

### Step 1: Create Shared DTO/Model

**File**: `IAMS.Web/Models/PolicyFormModel.cs`

```csharp
public class PolicyFormModel
{
    // Basic Info
    public PolicyStatus Status { get; set; }
    public int PolicyTypeId { get; set; }
    public string? PolicyNumber { get; set; }

    // Customer & Company
    public int? CustomerId { get; set; }
    public int? InsuranceCompanyId { get; set; }

    // Vehicle (optional)
    public int? VehicleId { get; set; }
    public bool HasVehicle { get; set; }

    // Period
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Financial
    public decimal PremiumAmount { get; set; }
    public decimal? CommissionRate { get; set; }
    public decimal? CommissionAmount { get; set; }

    // Additional
    public string? Notes { get; set; }
}
```

### Step 2: Create Section Components

#### Example: PolicyBasicInfoSection.razor

**File**: `IAMS.Web/Components/Policies/Sections/PolicyBasicInfoSection.razor`

```razor
@using IAMS.Application.DTOs.PolicyType
@using IAMS.Domain.Enums

<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">
                <MudIcon Icon="@Icons.Material.Filled.Info" Class="mr-2" />
                Temel Bilgiler
            </MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudGrid>
            @if (!IsEdit)
            {
                <MudItem xs="12">
                    <MudAlert Severity="Severity.Info" Dense="true" Class="mb-3">
                        Poliçe numarası otomatik olarak oluşturulacaktır
                    </MudAlert>
                </MudItem>
            }
            else
            {
                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="Model.PolicyNumber"
                                Label="Poliçe Numarası"
                                Variant="Variant.Outlined"
                                Disabled="true" />
                </MudItem>
            }

            <MudItem xs="12" md="@(IsEdit ? 6 : 12)">
                <MudSelect @bind-Value="Model.Status"
                         Label="Durum *"
                         Variant="Variant.Outlined"
                         Required="true"
                         T="PolicyStatus">
                    <MudSelectItem Value="@PolicyStatus.Draft">Taslak</MudSelectItem>
                    <MudSelectItem Value="@PolicyStatus.Active">Aktif</MudSelectItem>
                    <MudSelectItem Value="@PolicyStatus.Cancelled">İptal</MudSelectItem>
                </MudSelect>
            </MudItem>

            <MudItem xs="12">
                <MudSelect @bind-Value="Model.PolicyTypeId"
                         Label="Poliçe Tipi *"
                         Variant="Variant.Outlined"
                         Required="true"
                         ToStringFunc="@(pt => pt?.Name ?? "")"
                         T="PolicyTypeDto">
                    @foreach (var policyType in PolicyTypes)
                    {
                        <MudSelectItem Value="@policyType">@policyType.Name</MudSelectItem>
                    }
                </MudSelect>
            </MudItem>
        </MudGrid>
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public PolicyFormModel Model { get; set; } = null!;
    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public List<PolicyTypeDto> PolicyTypes { get; set; } = new();
}
```

#### Example: PolicyCustomerSection.razor

```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">
                <MudIcon Icon="@Icons.Material.Filled.Business" Class="mr-2" />
                Müşteri ve Şirket Bilgileri
            </MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudGrid>
            <MudItem xs="12" md="4">
                <MudStack Row AlignItems="AlignItems.Center" Spacing="2">
                    <MudTextField @bind-Value="customerSearch"
                                Label="Müşteri Ara"
                                Variant="Variant.Outlined"
                                Placeholder="İsim veya TC No ile arayın"
                                Adornment="Adornment.Start"
                                AdornmentIcon="@Icons.Material.Filled.Search" />
                    <MudIconButton Icon="@Icons.Material.Filled.Add"
                                 Color="Color.Primary"
                                 Variant="Variant.Filled"
                                 Size="Size.Large"
                                 OnClick="OpenCreateCustomerDialog"
                                 Title="Yeni Müşteri" />
                </MudStack>
            </MudItem>

            <MudItem xs="12" md="8">
                <MudAutocomplete @bind-Value="selectedCustomer"
                               Label="Müşteri Seçin *"
                               SearchFunc="@SearchCustomers"
                               ToStringFunc="@(c => c?.FullName ?? "")"
                               ResetValueOnEmptyText="true"
                               CoerceText="true"
                               CoerceValue="false"
                               Variant="Variant.Outlined"
                               T="CustomerDto"
                               ValueChanged="OnCustomerSelected">
                    <ItemTemplate Context="customer">
                        <MudStack Spacing="0">
                            <MudText Typo="Typo.body1">@customer.FullName</MudText>
                            <MudText Typo="Typo.caption" Color="Color.Secondary">
                                @customer.IdentificationNumber - @customer.Email
                            </MudText>
                        </MudStack>
                    </ItemTemplate>
                </MudAutocomplete>
            </MudItem>

            <MudItem xs="12">
                <MudSelect @bind-Value="Model.InsuranceCompanyId"
                         Label="Sigorta Şirketi *"
                         Variant="Variant.Outlined"
                         Required="true"
                         T="int?">
                    @foreach (var company in InsuranceCompanies)
                    {
                        <MudSelectItem Value="@((int?)company.Id)">@company.Name</MudSelectItem>
                    }
                </MudSelect>
            </MudItem>
        </MudGrid>
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public PolicyFormModel Model { get; set; } = null!;
    [Parameter] public EventCallback<CustomerDto> OnCustomerSelected { get; set; }
    [Parameter] public EventCallback OnCreateCustomer { get; set; }
    [Parameter] public List<CustomerDto> Customers { get; set; } = new();
    [Parameter] public List<InsuranceCompanyDto> InsuranceCompanies { get; set; } = new();

    private string customerSearch = "";
    private CustomerDto? selectedCustomer;

    private async Task<IEnumerable<CustomerDto>> SearchCustomers(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Customers;

        return Customers.Where(c =>
            c.FullName.Contains(value, StringComparison.OrdinalIgnoreCase) ||
            c.IdentificationNumber?.Contains(value) == true ||
            c.Email?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
    }

    private Task OpenCreateCustomerDialog()
    {
        return OnCreateCustomer.InvokeAsync();
    }
}
```

### Step 3: Update Main Component

**File**: `IAMS.Web/Components/Pages/Policies/PolicyForm.razor` (Reduced to ~150 lines)

```razor
@page "/policies/create"
@page "/policies/{PolicyId:int}/edit"
@layout Layout.MainLayout

@inject IPolicyService PolicyService
@inject ICustomerService CustomerService
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

<PageTitle>@(IsEdit ? "Poliçe Düzenle" : "Yeni Poliçe")</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="pa-6">
    <MudStack Spacing="4">
        <MudStack Row Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center">
            <MudText Typo="Typo.h4">
                @(IsEdit ? "Poliçe Düzenle" : "Yeni Poliçe")
            </MudText>
            <MudButton Variant="Variant.Outlined" OnClick="GoBack">Geri</MudButton>
        </MudStack>

        @if (loading)
        {
            <MudProgressCircular Indeterminate="true" />
        }
        else
        {
            <EditForm Model="@model" OnValidSubmit="OnValidSubmit">
                <FluentValidationValidator />

                <MudGrid>
                    <MudItem xs="12">
                        <PolicyBasicInfoSection Model="@model"
                                              IsEdit="@IsEdit"
                                              PolicyTypes="@policyTypes" />
                    </MudItem>

                    <MudItem xs="12">
                        <PolicyCustomerSection Model="@model"
                                             Customers="@customers"
                                             InsuranceCompanies="@insuranceCompanies"
                                             OnCustomerSelected="HandleCustomerSelected"
                                             OnCreateCustomer="OpenCreateCustomerDialog" />
                    </MudItem>

                    @if (selectedPolicyType?.RequiresVehicle == true)
                    {
                        <MudItem xs="12">
                            <PolicyVehicleSection Model="@model"
                                                Vehicles="@vehicles"
                                                CustomerId="@model.CustomerId" />
                        </MudItem>
                    }

                    <MudItem xs="12">
                        <PolicyPeriodSection Model="@model" />
                    </MudItem>

                    <MudItem xs="12">
                        <PolicyFinancialSection Model="@model"
                                              OnPremiumChanged="CalculateCommission" />
                    </MudItem>

                    <MudItem xs="12">
                        <PolicyAdditionalInfoSection Model="@model" />
                    </MudItem>

                    <MudItem xs="12">
                        <MudStack Row Justify="Justify.End" Spacing="2">
                            <MudButton Variant="Variant.Outlined" OnClick="GoBack">
                                İptal
                            </MudButton>
                            <MudButton Variant="Variant.Filled"
                                     Color="Color.Primary"
                                     ButtonType="ButtonType.Submit"
                                     Disabled="@saving">
                                @if (saving)
                                {
                                    <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                                }
                                else
                                {
                                    @(IsEdit ? "Güncelle" : "Kaydet")
                                }
                            </MudButton>
                        </MudStack>
                    </MudItem>
                </MudGrid>
            </EditForm>
        }
    </MudStack>
</MudContainer>

@code {
    [Parameter] public int? PolicyId { get; set; }

    private bool IsEdit => PolicyId.HasValue;
    private PolicyFormModel model = new();
    private bool loading = true;
    private bool saving = false;

    // Data lists
    private List<PolicyTypeDto> policyTypes = new();
    private List<CustomerDto> customers = new();
    private List<InsuranceCompanyDto> insuranceCompanies = new();
    private List<VehicleDto> vehicles = new();

    // Selected items
    private PolicyTypeDto? selectedPolicyType;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();

        if (IsEdit)
        {
            await LoadPolicy();
        }

        loading = false;
    }

    private async Task LoadData()
    {
        // Load lookup data in parallel
        var tasks = new[]
        {
            LoadPolicyTypes(),
            LoadCustomers(),
            LoadInsuranceCompanies()
        };

        await Task.WhenAll(tasks);
    }

    private async Task OnValidSubmit()
    {
        saving = true;
        try
        {
            var dto = MapToDto();

            var result = IsEdit
                ? await PolicyService.UpdatePolicyAsync(PolicyId.Value, dto)
                : await PolicyService.CreatePolicyAsync(dto);

            if (result.IsSuccess)
            {
                Snackbar.Add("Poliçe başarıyla kaydedildi", Severity.Success);
                Navigation.NavigateTo("/policies");
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Error);
            }
        }
        finally
        {
            saving = false;
        }
    }

    // Event handlers
    private async Task HandleCustomerSelected(CustomerDto customer)
    {
        model.CustomerId = customer.Id;
        await LoadCustomerVehicles(customer.Id);
    }

    private void CalculateCommission()
    {
        if (model.CommissionRate.HasValue)
        {
            model.CommissionAmount = model.PremiumAmount * (model.CommissionRate.Value / 100);
        }
    }

    // Helper methods...
}
```

---

## Benefits of Refactoring

### Before (Monolithic)
❌ **902 lines** in one file
❌ **Multiple responsibilities** mixed together
❌ **Hard to test** individual sections
❌ **Difficult to reuse** sections
❌ **Complex navigation** in large file
❌ **Merge conflicts** likely

### After (Component-Based)
✅ **~150 lines** per file (7 files total)
✅ **Single responsibility** per component
✅ **Easy to test** each section
✅ **Reusable** components
✅ **Clear organization** by feature
✅ **Fewer merge conflicts**

---

## Refactoring Checklist

### For Each Large Component:

- [ ] **Identify logical sections** - Look for MudCard boundaries
- [ ] **Extract shared models** - Create form models if needed
- [ ] **Create section components** - One component per section
- [ ] **Define clear parameters** - Model, data lists, event callbacks
- [ ] **Move event handlers** - To appropriate section or keep in parent
- [ ] **Update main component** - Use section components
- [ ] **Test functionality** - Ensure nothing breaks
- [ ] **Update tests** - Test individual sections
- [ ] **Document components** - Add XML documentation

---

## Priority Components to Refactor

1. **PolicyForm.razor** (902 lines) - HIGHEST PRIORITY
   - 6 clear sections
   - High complexity
   - Used frequently

2. **CustomerForm.razor** (631 lines) - HIGH PRIORITY
   - Similar structure to PolicyForm
   - Can reuse pattern

3. **FinancialReports.razor** (663 lines) - MEDIUM PRIORITY
   - Multiple report types
   - Can extract report components

4. **PoliciesList.razor** (649 lines) - MEDIUM PRIORITY
   - Extract filters, table, pagination

5. **ExpiringPolicies.razor** (654 lines) - LOW PRIORITY
   - Similar to PoliciesList

---

## File Organization

### Recommended Structure:

```
src/IAMS.Web/Components/
├── Pages/
│   ├── Policies/
│   │   ├── PolicyForm.razor (orchestrator)
│   │   ├── PoliciesList.razor
│   │   └── PolicyDetails.razor
│   └── Customers/
│       ├── CustomerForm.razor (orchestrator)
│       └── CustomerList.razor
├── Policies/
│   └── Sections/
│       ├── PolicyBasicInfoSection.razor
│       ├── PolicyCustomerSection.razor
│       ├── PolicyVehicleSection.razor
│       ├── PolicyPeriodSection.razor
│       ├── PolicyFinancialSection.razor
│       └── PolicyAdditionalInfoSection.razor
└── Customers/
    └── Sections/
        ├── CustomerBasicInfoSection.razor
        ├── CustomerContactSection.razor
        └── CustomerAddressSection.razor
```

---

## Migration Timeline

### Week 1: PolicyForm Refactoring
- Day 1-2: Extract sections
- Day 3: Test and fix issues
- Day 4-5: Code review and refinement

### Week 2: CustomerForm Refactoring
- Day 1-2: Apply same pattern
- Day 3: Testing
- Day 4-5: Other large components

### Week 3: Remaining Components
- Refactor FinancialReports, PoliciesList, ExpiringPolicies
- Final testing and documentation

---

## Conclusion

Breaking down large components into smaller, focused components:
- **Improves maintainability**
- **Enables reusability**
- **Simplifies testing**
- **Reduces complexity**
- **Follows Single Responsibility Principle**

Start with PolicyForm.razor as it's the largest and most complex, then apply the same pattern to other components.
