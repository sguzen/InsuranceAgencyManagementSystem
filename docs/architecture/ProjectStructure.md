InsuranceAgencyManagementSystem.sln
│
├── src/
│   │
│   ├── 1. Core Layer/
│   │   │
│   │   ├── IAMS.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── BaseEntity.cs
│   │   │   │   ├── Customer.cs
│   │   │   │   ├── Policy.cs
│   │   │   │   ├── InsuranceCompany.cs
│   │   │   │   ├── CustomerMapping.cs
│   │   │   │   ├── PolicyType.cs
│   │   │   │   ├── CommissionRate.cs
│   │   │   │   ├── PolicyPayment.cs
│   │   │   │   └── PolicyClaim.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Address.cs
│   │   │   │   ├── Money.cs
│   │   │   │   └── DateRange.cs
│   │   │   ├── Enums/
│   │   │   │   ├── PolicyStatus.cs
│   │   │   │   ├── PaymentStatus.cs
│   │   │   │   ├── ClaimStatus.cs
│   │   │   │   └── CustomerStatus.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── ITenantEntity.cs
│   │   │   │   ├── IAuditable.cs
│   │   │   │   └── ISoftDeletable.cs
│   │   │   ├── Services/
│   │   │   │   ├── IPolicyNumberGenerator.cs
│   │   │   │   ├── ICustomerCodeGenerator.cs
│   │   │   │   └── ICommissionCalculator.cs
│   │   │   ├── Events/
│   │   │   │   ├── PolicyCreatedEvent.cs
│   │   │   │   ├── CustomerRegisteredEvent.cs
│   │   │   │   └── PaymentProcessedEvent.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── DomainException.cs
│   │   │   │   ├── PolicyValidationException.cs
│   │   │   │   └── CustomerValidationException.cs
│   │   │   └── IAMS.Domain.csproj
│   │   │
│   │   └── IAMS.Application/
│   │       ├── Services/
│   │       │   ├── Customers/
│   │       │   │   ├── ICustomerService.cs
│   │       │   │   └── CustomerService.cs
│   │       │   ├── Policies/
│   │       │   │   ├── IPolicyService.cs
│   │       │   │   └── PolicyService.cs
│   │       │   └── InsuranceCompanies/
│   │       │       ├── IInsuranceCompanyService.cs
│   │       │       └── InsuranceCompanyService.cs
│   │       ├── DTOs/
│   │       │   ├── Customer/
│   │       │   │   ├── CustomerDto.cs
│   │       │   │   ├── CreateCustomerDto.cs
│   │       │   │   └── UpdateCustomerDto.cs
│   │       │   ├── Policy/
│   │       │   │   ├── PolicyDto.cs
│   │       │   │   ├── CreatePolicyDto.cs
│   │       │   │   └── UpdatePolicyDto.cs
│   │       │   └── Identity/
│   │       │       ├── LoginDto.cs
│   │       │       ├── RegisterUserDto.cs
│   │       │       ├── UserDto.cs
│   │       │       ├── RoleDto.cs
│   │       │       └── PermissionDto.cs
│   │       ├── Interfaces/
│   │       │   ├── IRepository.cs
│   │       │   ├── IUnitOfWork.cs
│   │       │   └── Repositories/
│   │       │       ├── ICustomerRepository.cs
│   │       │       ├── IPolicyRepository.cs
│   │       │       └── IInsuranceCompanyRepository.cs
│   │       ├── Models/
│   │       │   ├── PagedResult.cs
│   │       │   ├── UserQueryParams.cs
│   │       │   └── ApiResponse.cs
│   │       ├── Mappings/
│   │       │   ├── CustomerMappingProfile.cs
│   │       │   ├── PolicyMappingProfile.cs
│   │       │   └── IdentityMappingProfile.cs
│   │       ├── Validators/
│   │       │   ├── CreateCustomerValidator.cs
│   │       │   ├── UpdateCustomerValidator.cs
│   │       │   └── CreatePolicyValidator.cs
│   │       ├── Extensions/
│   │       │   └── ServiceCollectionExtensions.cs
│   │       └── IAMS.Application.csproj
│   │
│   ├── 2. Infrastructure Layer/
│   │   │
│   │   ├── IAMS.Infrastructure/
│   │   │   ├── Services/
│   │   │   │   ├── Email/
│   │   │   │   │   ├── IEmailService.cs
│   │   │   │   │   └── EmailService.cs
│   │   │   │   ├── Storage/
│   │   │   │   │   ├── IFileStorageService.cs
│   │   │   │   │   └── FileStorageService.cs
│   │   │   │   ├── Notifications/
│   │   │   │   │   ├── INotificationService.cs
│   │   │   │   │   └── NotificationService.cs
│   │   │   │   └── BackgroundJobs/
│   │   │   │       ├── PolicyReminderJob.cs
│   │   │   │       └── DataSyncJob.cs
│   │   │   ├── ExternalServices/
│   │   │   │   ├── InsuranceCompanyAdapters/
│   │   │   │   │   ├── IInsuranceCompanyAdapter.cs
│   │   │   │   │   ├── CompanyAAdapter.cs
│   │   │   │   │   └── CompanyBAdapter.cs
│   │   │   │   └── PaymentGateways/
│   │   │   │       ├── IPaymentGateway.cs
│   │   │   │       └── StripePaymentGateway.cs
│   │   │   ├── Logging/
│   │   │   │   └── ApplicationLogger.cs
│   │   │   ├── Caching/
│   │   │   │   ├── ICacheService.cs
│   │   │   │   └── RedisCacheService.cs
│   │   │   ├── Extensions/
│   │   │   │   └── ServiceCollectionExtensions.cs
│   │   │   └── IAMS.Infrastructure.csproj
│   │   │
│   │   ├── IAMS.Persistence/
│   │   │   ├── Data/
│   │   │   │   ├── AppDbContext.cs
│   │   │   │   ├── DbContextFactory.cs
│   │   │   │   └── Configurations/
│   │   │   │       ├── CustomerConfiguration.cs
│   │   │   │       ├── PolicyConfiguration.cs
│   │   │   │       └── InsuranceCompanyConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── Repository.cs
│   │   │   │   ├── CustomerRepository.cs
│   │   │   │   ├── PolicyRepository.cs
│   │   │   │   └── InsuranceCompanyRepository.cs
│   │   │   ├── UnitOfWork/
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Migrations/
│   │   │   │   └── (Auto-generated migration files)
│   │   │   ├── Seeders/
│   │   │   │   ├── ApplicationDataSeeder.cs
│   │   │   │   └── TestDataSeeder.cs
│   │   │   ├── Extensions/
│   │   │   │   └── ServiceCollectionExtensions.cs
│   │   │   └── IAMS.Persistence.csproj
│   │   │
│   │   └── IAMS.Identity/
│   │       ├── Data/
│   │       │   ├── AppIdentityDbContext.cs
│   │       │   └── IdentityDataSeeder.cs
│   │       ├── Models/
│   │       │   ├── ApplicationUser.cs
│   │       │   ├── ApplicationRole.cs
│   │       │   ├── Permission.cs
│   │       │   ├── RolePermission.cs
│   │       │   └── RefreshToken.cs
│   │       ├── Services/
│   │       │   ├── IIdentityService.cs
│   │       │   ├── IdentityService.cs
│   │       │   ├── ITokenService.cs
│   │       │   ├── TokenService.cs
│   │       │   ├── IPermissionService.cs
│   │       │   └── PermissionService.cs
│   │       ├── Authorization/
│   │       │   ├── PermissionRequirement.cs
│   │       │   ├── PermissionHandler.cs
│   │       │   ├── ModuleRequirement.cs
│   │       │   ├── ModuleHandler.cs
│   │       │   ├── HasPermissionAttribute.cs
│   │       │   ├── RequiresModuleAttribute.cs
│   │       │   └── PermissionAuthorizationPolicyProvider.cs
│   │       ├── Extensions/
│   │       │   └── ServiceCollectionExtensions.cs
│   │       └── IAMS.Identity.csproj
│   │
│   ├── 3. Presentation Layer/
│   │   │
│   │   ├── IAMS.Web/
│   │   │   ├── Controllers/
│   │   │   │   ├── HomeController.cs
│   │   │   │   ├── CustomersController.cs
│   │   │   │   ├── PoliciesController.cs
│   │   │   │   ├── AccountController.cs
│   │   │   │   └── AdminController.cs
│   │   │   ├── Views/
│   │   │   │   ├── Shared/
│   │   │   │   │   ├── _Layout.cshtml
│   │   │   │   │   ├── _LoginPartial.cshtml
│   │   │   │   │   └── Error.cshtml
│   │   │   │   ├── Home/
│   │   │   │   │   ├── Index.cshtml
│   │   │   │   │   └── Dashboard.cshtml
│   │   │   │   ├── Customers/
│   │   │   │   │   ├── Index.cshtml
│   │   │   │   │   ├── Create.cshtml
│   │   │   │   │   ├── Edit.cshtml
│   │   │   │   │   └── Details.cshtml
│   │   │   │   ├── Policies/
│   │   │   │   │   ├── Index.cshtml
│   │   │   │   │   ├── Create.cshtml
│   │   │   │   │   ├── Edit.cshtml
│   │   │   │   │   └── Details.cshtml
│   │   │   │   └── Account/
│   │   │   │       ├── Login.cshtml
│   │   │   │       ├── Register.cshtml
│   │   │   │       └── Profile.cshtml
│   │   │   ├── Models/
│   │   │   │   ├── ViewModels/
│   │   │   │   │   ├── CustomerViewModel.cs
│   │   │   │   │   ├── PolicyViewModel.cs
│   │   │   │   │   └── DashboardViewModel.cs
│   │   │   │   └── ErrorViewModel.cs
│   │   │   ├── wwwroot/
│   │   │   │   ├── css/
│   │   │   │   │   ├── site.css
│   │   │   │   │   └── bootstrap.min.css
│   │   │   │   ├── js/
│   │   │   │   │   ├── site.js
│   │   │   │   │   └── jquery.min.js
│   │   │   │   ├── images/
│   │   │   │   └── lib/
│   │   │   ├── Areas/
│   │   │   │   ├── Admin/
│   │   │   │   │   ├── Controllers/
│   │   │   │   │   │   ├── UsersController.cs
│   │   │   │   │   │   ├── RolesController.cs
│   │   │   │   │   │   └── SettingsController.cs
│   │   │   │   │   └── Views/
│   │   │   │   └── Reporting/
│   │   │   │       ├── Controllers/
│   │   │   │       │   └── ReportsController.cs
│   │   │   │       └── Views/
│   │   │   ├── Filters/
│   │   │   │   ├── TenantActionFilter.cs
│   │   │   │   └── ModuleAuthorizationFilter.cs
│   │   │   ├── Middleware/
│   │   │   │   └── ExceptionHandlingMiddleware.cs
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   └── IAMS.Web.csproj
│   │   │
│   │   └── IAMS.API/
│   │       ├── Controllers/
│   │       │   ├── AuthController.cs
│   │       │   ├── CustomersController.cs
│   │       │   ├── PoliciesController.cs
│   │       │   ├── InsuranceCompaniesController.cs
│   │       │   ├── UsersController.cs
│   │       │   ├── RolesController.cs
│   │       │   └── PermissionsController.cs
│   │       ├── Middleware/
│   │       │   ├── ApiExceptionMiddleware.cs
│   │       │   ├── ApiLoggingMiddleware.cs
│   │       │   └── RateLimitingMiddleware.cs
│   │       ├── Filters/
│   │       │   ├── ValidateModelFilter.cs
│   │       │   └── ApiAuthorizationFilter.cs
│   │       ├── Extensions/
│   │       │   ├── ControllerExtensions.cs
│   │       │   └── SwaggerExtensions.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       └── IAMS.API.csproj
│   │
│   ├── 4. Shared Layer/
│   │   │
│   │   └── IAMS.Shared/
│   │       ├── Constants/
│   │       │   ├── ApplicationConstants.cs
│   │       │   ├── PolicyConstants.cs
│   │       │   └── SecurityConstants.cs
│   │       ├── Extensions/
│   │       │   ├── StringExtensions.cs
│   │       │   ├── DateTimeExtensions.cs
│   │       │   └── EnumExtensions.cs
│   │       ├── Helpers/
│   │       │   ├── ValidationHelper.cs
│   │       │   ├── CurrencyHelper.cs
│   │       │   └── DateHelper.cs
│   │       ├── Models/
│   │       │   ├── Result.cs
│   │       │   ├── ApiError.cs
│   │       │   └── PaginationOptions.cs
│   │       ├── Utilities/
│   │       │   ├── JsonSerializer.cs
│   │       │   ├── PasswordGenerator.cs
│   │       │   └── FileHelper.cs
│   │       └── IAMS.Shared.csproj
│   │
│   └── 5. Multi-Tenancy/
│       │
│       └── IAMS.MultiTenancy/
│           ├── Data/
│           │   └── MasterDbContext.cs
│           ├── Entities/
│           │   ├── TenantEntity.cs
│           │   ├── TenantModule.cs
│           │   └── TenantSetting.cs
│           ├── Models/
│           │   ├── Tenant.cs
│           │   └── TenantContext.cs
│           ├── Interfaces/
│           │   ├── ITenantService.cs
│           │   └── ITenantContextAccessor.cs
│           ├── Services/
│           │   ├── TenantService.cs
│           │   ├── TenantContextAccessor.cs
│           │   └── ModuleService.cs
│           ├── Middleware/
│           │   └── TenantMiddleware.cs
│           ├── Extensions/
│           │   ├── ServiceCollectionExtensions.cs
│           │   ├── HttpContextExtensions.cs
│           │   └── ClaimsPrincipalExtensions.cs
│           └── IAMS.MultiTenancy.csproj
│
├── tests/
│   │
│   ├── IAMS.UnitTests/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── CustomerTests.cs
│   │   │   │   └── PolicyTests.cs
│   │   │   └── Services/
│   │   │       └── DomainServiceTests.cs
│   │   ├── Application/
│   │   │   ├── Services/
│   │   │   │   ├── CustomerServiceTests.cs
│   │   │   │   └── PolicyServiceTests.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfileTests.cs
│   │   ├── Infrastructure/
│   │   │   ├── Repositories/
│   │   │   │   ├── CustomerRepositoryTests.cs
│   │   │   │   └── PolicyRepositoryTests.cs
│   │   │   └── Services/
│   │   │       ├── EmailServiceTests.cs
│   │   │       └── FileStorageServiceTests.cs
│   │   ├── MultiTenancy/
│   │   │   ├── TenantServiceTests.cs
│   │   │   └── TenantContextAccessorTests.cs
│   │   ├── Helpers/
│   │   │   ├── TenantTestHelper.cs
│   │   │   └── DatabaseTestHelper.cs
│   │   └── IAMS.UnitTests.csproj
│   │
│   └── IAMS.IntegrationTests/
│       ├── API/
│       │   ├── Controllers/
│       │   │   ├── AuthControllerTests.cs
│       │   │   ├── CustomersControllerTests.cs
│       │   │   └── PoliciesControllerTests.cs
│       │   └── Middleware/
│       │       └── TenantMiddlewareTests.cs
│       ├── Database/
│       │   ├── RepositoryIntegrationTests.cs
│       │   └── DatabaseMigrationTests.cs
│       ├── MultiTenancy/
│       │   └── TenantIsolationTests.cs
│       ├── Fixtures/
│       │   ├── WebApplicationFactory.cs
│       │   └── DatabaseFixture.cs
│       └── IAMS.IntegrationTests.csproj
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API_DOCUMENTATION.md
│   ├── DEPLOYMENT_GUIDE.md
│   ├── USER_MANUAL.md
│   └── DEVELOPMENT_SETUP.md
│
├── scripts/
│   ├── database/
│   │   ├── create-tenant-database.sql
│   │   ├── seed-master-data.sql
│   │   └── backup-tenant-data.sql
│   ├── deployment/
│   │   ├── deploy-staging.sh
│   │   ├── deploy-production.sh
│   │   └── rollback.sh
│   └── development/
│       ├── setup-dev-environment.sh
│       └── run-tests.sh
│
├── docker/
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── .dockerignore
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── cd.yml
│       └── security-scan.yml
│
├── .gitignore
├── README.md
├── CHANGELOG.md
├── LICENSE
└── global.json