USE [test]
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Cities] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [NameTr] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Cities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Countries] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [NameTr] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [PhoneCode] nvarchar(10) NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Currencies] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Symbol] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [NameTr] nvarchar(max) NOT NULL,
        [DecimalPlaces] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsBaseCurrency] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Currencies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [InsuranceCompanies] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ContactPerson] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [Website] nvarchar(500) NULL,
        [MasterInsuranceCompanyId] int NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_InsuranceCompanies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Occupations] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [NameTr] nvarchar(150) NOT NULL,
        [NameEn] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Occupations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Module] nvarchar(100) NULL,
        [IsSystem] bit NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [Category] nvarchar(max) NULL,
        [MinimumTermMonths] int NULL,
        [MaximumTermMonths] int NULL,
        [MinimumPremium] decimal(18,2) NULL,
        [MaximumPremium] decimal(18,2) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_PolicyTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] nvarchar(450) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsDefault] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [TenantSettings] (
        [Id] int NOT NULL IDENTITY,
        [SettingKey] nvarchar(100) NOT NULL,
        [SettingValue] nvarchar(max) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Category] nvarchar(50) NOT NULL,
        [IsSystemSetting] bit NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_TenantSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastLogin] datetime2 NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiry] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [VehicleBrands] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [LogoUrl] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_VehicleBrands] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Districts] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [NameTr] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [CityId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Districts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Districts_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [CurrencyExchangeRates] (
        [Id] int NOT NULL IDENTITY,
        [FromCurrencyId] int NOT NULL,
        [ToCurrencyId] int NOT NULL,
        [Rate] decimal(18,2) NOT NULL,
        [EffectiveDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NULL,
        [Source] nvarchar(max) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CurrencyExchangeRates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CurrencyExchangeRates_Currencies_FromCurrencyId] FOREIGN KEY ([FromCurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CurrencyExchangeRates_Currencies_ToCurrencyId] FOREIGN KEY ([ToCurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [ImportConfigurations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [InsuranceCompanyId] int NOT NULL,
        [SourceType] int NOT NULL,
        [ApiBaseUrl] nvarchar(500) NULL,
        [ApiKey] nvarchar(500) NULL,
        [ApiUsername] nvarchar(200) NULL,
        [ApiPassword] nvarchar(500) NULL,
        [CustomHeaders] nvarchar(max) NULL,
        [PoliciesEndpoint] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [EnableAutoSync] bit NOT NULL,
        [SyncIntervalMinutes] int NULL,
        [LastSyncDate] datetime2 NULL,
        [LastSyncStatus] nvarchar(100) NULL,
        [AdditionalSettings] nvarchar(max) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ImportConfigurations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ImportConfigurations_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [CommissionRates] (
        [Id] int NOT NULL IDENTITY,
        [InsuranceCompanyId] int NOT NULL,
        [PolicyTypeId] int NOT NULL,
        [Rate] decimal(18,2) NOT NULL,
        [CurrencyId] int NOT NULL,
        [EffectiveDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CommissionRates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommissionRates_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommissionRates_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommissionRates_PolicyTypes_PolicyTypeId] FOREIGN KEY ([PolicyTypeId]) REFERENCES [PolicyTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] nvarchar(450) NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [UserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [UserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [VehicleModels] (
        [Id] int NOT NULL IDENTITY,
        [BrandId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [MinYear] int NULL,
        [MaxYear] int NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_VehicleModels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VehicleModels_VehicleBrands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [VehicleBrands] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Subdistricts] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [NameTr] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [DistrictId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Subdistricts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Subdistricts_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [ImportHistories] (
        [Id] int NOT NULL IDENTITY,
        [SourceType] int NOT NULL,
        [ImportConfigurationId] int NULL,
        [InsuranceCompanyId] int NOT NULL,
        [FileName] nvarchar(500) NULL,
        [FileSize] bigint NULL,
        [StartedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [Status] nvarchar(50) NOT NULL,
        [TotalRecords] int NOT NULL,
        [SuccessCount] int NOT NULL,
        [FailureCount] int NOT NULL,
        [SkippedCount] int NOT NULL,
        [ErrorMessages] nvarchar(max) NULL,
        [Notes] nvarchar(2000) NULL,
        [ImportedBy] nvarchar(200) NOT NULL,
        [DurationSeconds] int NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ImportHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ImportHistories_ImportConfigurations_ImportConfigurationId] FOREIGN KEY ([ImportConfigurationId]) REFERENCES [ImportConfigurations] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ImportHistories_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Villages] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [NameTr] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [SubdistrictId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Villages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Villages_Subdistricts_SubdistrictId] FOREIGN KEY ([SubdistrictId]) REFERENCES [Subdistricts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [CustomerCode] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Address1] nvarchar(500) NULL,
        [Address2] nvarchar(500) NULL,
        [CityId] int NULL,
        [DistrictId] int NULL,
        [SubdistrictId] int NULL,
        [VillageId] int NULL,
        [OccupationId] int NULL,
        [NationalityCountryId] int NULL,
        [MobilePhoneCountryCode] nvarchar(10) NULL,
        [MobilePhoneNumber] nvarchar(20) NULL,
        [HomePhone] nvarchar(20) NULL,
        [IdentificationNumber] nvarchar(11) NOT NULL,
        [DateOfBirth] datetime2 NULL,
        [Status] int NOT NULL,
        [Type] int NOT NULL,
        [IdentificationType] int NOT NULL,
        [Gender] int NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Customers_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Countries_NationalityCountryId] FOREIGN KEY ([NationalityCountryId]) REFERENCES [Countries] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Occupations_OccupationId] FOREIGN KEY ([OccupationId]) REFERENCES [Occupations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Subdistricts_SubdistrictId] FOREIGN KEY ([SubdistrictId]) REFERENCES [Subdistricts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Villages_VillageId] FOREIGN KEY ([VillageId]) REFERENCES [Villages] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomerInsuranceCompanies] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [InsuranceCompanyId] int NOT NULL,
        [ExternalCustomerId] nvarchar(max) NOT NULL,
        [RegisteredDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [LastSyncDate] datetime2 NULL,
        [SyncStatus] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CustomerInsuranceCompanies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerInsuranceCompanies_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CustomerInsuranceCompanies_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomerPayments] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [PaymentMethod] int NOT NULL,
        [CurrencyId] int NOT NULL,
        [ExchangeRateToBase] decimal(18,2) NULL,
        [AmountInBaseCurrency] decimal(18,2) NULL,
        [Reference] nvarchar(100) NULL,
        [Notes] nvarchar(1000) NULL,
        [Status] int NOT NULL DEFAULT 1,
        [AllocatedAmount] decimal(18,2) NOT NULL DEFAULT 0.0,
        [AllocationStatus] int NOT NULL DEFAULT 0,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CustomerPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerPayments_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CustomerPayments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [Id] int NOT NULL IDENTITY,
        [PlateNumber] nvarchar(max) NOT NULL,
        [ChassisNumber] nvarchar(max) NOT NULL,
        [EngineNumber] nvarchar(max) NULL,
        [RegistrationNumber] nvarchar(max) NULL,
        [BrandId] int NULL,
        [BrandName] nvarchar(max) NULL,
        [ModelId] int NULL,
        [ModelName] nvarchar(max) NULL,
        [ModelYear] int NULL,
        [VehicleType] int NOT NULL,
        [FuelType] int NOT NULL,
        [UsageType] int NOT NULL,
        [Color] nvarchar(max) NULL,
        [EngineVolume] int NULL,
        [EnginePower] int NULL,
        [SeatCount] int NULL,
        [Weight] decimal(18,2) NULL,
        [LoadCapacity] int NULL,
        [FirstRegistrationDate] datetime2 NULL,
        [TrafficRegistrationDate] datetime2 NULL,
        [LastInspectionDate] datetime2 NULL,
        [NextInspectionDate] datetime2 NULL,
        [CurrentValue] decimal(18,2) NULL,
        [CurrencyId] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CustomerId] int NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Vehicles_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Vehicles_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Vehicles_VehicleBrands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [VehicleBrands] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Vehicles_VehicleModels_ModelId] FOREIGN KEY ([ModelId]) REFERENCES [VehicleModels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Policies] (
        [Id] int NOT NULL IDENTITY,
        [PolicyNumber] nvarchar(50) NOT NULL,
        [TecditNumber] nvarchar(max) NULL,
        [CustomerId] int NOT NULL,
        [EnsuredEntity] nvarchar(500) NULL,
        [InsuranceCompanyId] int NOT NULL,
        [PolicyTypeId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [PremiumAmount] decimal(18,2) NOT NULL,
        [CommissionAmount] decimal(18,2) NOT NULL,
        [CommissionRate] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CurrencyId] int NOT NULL,
        [ExchangeRateToBase] decimal(18,2) NULL,
        [PremiumAmountInBaseCurrency] decimal(18,2) NULL,
        [VehicleId] int NULL,
        [TrafficPolicyNumber] nvarchar(max) NULL,
        [TrafficStartDate] datetime2 NULL,
        [TrafficEndDate] datetime2 NULL,
        [ComprehensivePolicyNumber] nvarchar(max) NULL,
        [DeductibleAmount] decimal(18,2) NULL,
        [HasGlassCoverage] bit NULL,
        [HasTheftCoverage] bit NULL,
        [HasNaturalDisasterCoverage] bit NULL,
        [HasDriverAccidentCoverage] bit NULL,
        [DriverAccidentCoverageAmount] int NULL,
        [NoClaimDiscountRate] decimal(18,2) NULL,
        [NoClaimYears] int NULL,
        [FleetDiscountRate] decimal(18,2) NULL,
        [PreviousPolicyNumber] nvarchar(max) NULL,
        [PreviousInsuranceCompanyId] int NULL,
        [PreviousPolicyEndDate] datetime2 NULL,
        [ParentPolicyId] int NULL,
        [InnerCode] nvarchar(3) NOT NULL DEFAULT N'000',
        [StateType] int NOT NULL,
        [OriginalPolicyId] int NULL,
        [BranchCode] nvarchar(50) NULL,
        [DriverAge] int NULL,
        [DriverType] int NULL,
        [Marketer] nvarchar(200) NULL,
        [VehicleId1] int NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Policies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Policies_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_InsuranceCompanies_PreviousInsuranceCompanyId] FOREIGN KEY ([PreviousInsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]),
        CONSTRAINT [FK_Policies_Policies_OriginalPolicyId] FOREIGN KEY ([OriginalPolicyId]) REFERENCES [Policies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Policies_ParentPolicyId] FOREIGN KEY ([ParentPolicyId]) REFERENCES [Policies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_PolicyTypes_PolicyTypeId] FOREIGN KEY ([PolicyTypeId]) REFERENCES [PolicyTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Vehicles_VehicleId1] FOREIGN KEY ([VehicleId1]) REFERENCES [Vehicles] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [PolicyId] int NOT NULL,
        [CustomerId] int NOT NULL,
        [InvoiceDate] datetime2 NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [SubtotalAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [TaxRate] decimal(18,2) NOT NULL,
        [PaymentStatus] int NOT NULL,
        [PaymentDate] datetime2 NULL,
        [CurrencyId] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [PaymentReference] nvarchar(100) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [PaymentAllocations] (
        [Id] int NOT NULL IDENTITY,
        [CustomerPaymentId] int NOT NULL,
        [PolicyId] int NOT NULL,
        [AllocatedAmount] decimal(18,2) NOT NULL,
        [AllocationDate] datetime2 NOT NULL,
        [AllocationType] int NOT NULL DEFAULT 0,
        [Notes] nvarchar(500) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_PaymentAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentAllocations_CustomerPayments_CustomerPaymentId] FOREIGN KEY ([CustomerPaymentId]) REFERENCES [CustomerPayments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PaymentAllocations_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyClaims] (
        [Id] int NOT NULL IDENTITY,
        [PolicyId] int NOT NULL,
        [ClaimNumber] nvarchar(max) NOT NULL,
        [ClaimDate] datetime2 NOT NULL,
        [ClaimAmount] decimal(18,2) NOT NULL,
        [SettledAmount] decimal(18,2) NULL,
        [Status] int NOT NULL,
        [ClaimType] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Currency] nvarchar(max) NOT NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_PolicyClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PolicyClaims_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyPayments] (
        [Id] int NOT NULL IDENTITY,
        [PolicyId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [DueDate] datetime2 NULL,
        [PaymentMethod] int NOT NULL,
        [Reference] nvarchar(100) NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CurrencyId] int NOT NULL,
        [ExchangeRateToBase] decimal(18,2) NULL,
        [AmountInBaseCurrency] decimal(18,2) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_PolicyPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PolicyPayments_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PolicyPayments_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceItems] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [RowVersion] rowversion NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_InvoiceItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Cities]'))
        SET IDENTITY_INSERT [Cities] ON;
    EXEC(N'INSERT INTO [Cities] ([Id], [Code], [CreatedBy], [CreatedOn], [DeletedBy], [DeletedOn], [DisplayOrder], [IsActive], [IsDeleted], [ModifiedBy], [ModifiedOn], [NameEn], [NameTr])
    VALUES (1, N''01'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 1, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Nicosia'', N''Lefkoşa''),
    (2, N''02'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Famagusta'', N''Gazimağusa''),
    (3, N''03'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 3, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Kyrenia'', N''Girne''),
    (4, N''04'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 4, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Morphou'', N''Güzelyurt''),
    (5, N''05'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 5, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Lefka'', N''Lefke''),
    (6, N''06'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 6, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Trikomo'', N''İskele'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Cities]'))
        SET IDENTITY_INSERT [Cities] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DecimalPlaces', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsBaseCurrency', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'Name', N'NameTr', N'Symbol') AND [object_id] = OBJECT_ID(N'[Currencies]'))
        SET IDENTITY_INSERT [Currencies] ON;
    EXEC(N'INSERT INTO [Currencies] ([Id], [Code], [CreatedBy], [CreatedOn], [DecimalPlaces], [DeletedBy], [DeletedOn], [DisplayOrder], [IsActive], [IsBaseCurrency], [IsDeleted], [ModifiedBy], [ModifiedOn], [Name], [NameTr], [Symbol])
    VALUES (1, N''TRY'', N''System'', ''2024-01-01T00:00:00.0000000Z'', 2, NULL, NULL, 1, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Turkish Lira'', N''Türk Lirası'', N''₺''),
    (2, N''USD'', N''System'', ''2024-01-01T00:00:00.0000000Z'', 2, NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), NULL, NULL, N''US Dollar'', N''Amerikan Doları'', N''$''),
    (3, N''EUR'', N''System'', ''2024-01-01T00:00:00.0000000Z'', 2, NULL, NULL, 3, CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), NULL, NULL, N''Euro'', N''Euro'', N''€''),
    (4, N''GBP'', N''System'', ''2024-01-01T00:00:00.0000000Z'', 2, NULL, NULL, 4, CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), NULL, NULL, N''British Pound'', N''İngiliz Sterlini'', N''£'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DecimalPlaces', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsBaseCurrency', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'Name', N'NameTr', N'Symbol') AND [object_id] = OBJECT_ID(N'[Currencies]'))
        SET IDENTITY_INSERT [Currencies] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Occupations]'))
        SET IDENTITY_INSERT [Occupations] ON;
    EXEC(N'INSERT INTO [Occupations] ([Id], [Code], [CreatedBy], [CreatedOn], [DeletedBy], [DeletedOn], [Description], [DisplayOrder], [IsActive], [IsDeleted], [ModifiedBy], [ModifiedOn], [NameEn], [NameTr])
    VALUES (1, N''001'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 1, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Doctor'', N''Doktor''),
    (2, N''002'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Engineer'', N''Mühendis''),
    (3, N''003'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 3, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Teacher'', N''Öğretmen''),
    (4, N''004'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 4, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Lawyer'', N''Avukat''),
    (5, N''005'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 5, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Accountant'', N''Muhasebeci''),
    (6, N''006'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 6, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Business Owner'', N''İşletme Sahibi''),
    (7, N''007'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 7, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Civil Servant'', N''Memur''),
    (8, N''008'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 8, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Retired'', N''Emekli''),
    (9, N''009'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 9, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Student'', N''Öğrenci''),
    (10, N''010'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 10, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Homemaker'', N''Ev Hanımı''),
    (11, N''011'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 11, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Freelancer'', N''Serbest Meslek''),
    (12, N''012'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 12, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Technician'', N''Teknisyen''),
    (13, N''013'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 13, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Sales Consultant'', N''Satış Danışmanı''),
    (14, N''014'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 14, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Banker'', N''Bankacı''),
    (15, N''015'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 15, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Nurse'', N''Hemşire''),
    (16, N''999'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, 99, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Other'', N''Diğer'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'Description', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Occupations]'))
        SET IDENTITY_INSERT [Occupations] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'Description', N'IsActive', N'IsDeleted', N'MaximumPremium', N'MaximumTermMonths', N'MinimumPremium', N'MinimumTermMonths', N'ModifiedBy', N'ModifiedOn', N'Name') AND [object_id] = OBJECT_ID(N'[PolicyTypes]'))
        SET IDENTITY_INSERT [PolicyTypes] ON;
    EXEC(N'INSERT INTO [PolicyTypes] ([Id], [Category], [Code], [CreatedBy], [CreatedOn], [DeletedBy], [DeletedOn], [Description], [IsActive], [IsDeleted], [MaximumPremium], [MaximumTermMonths], [MinimumPremium], [MinimumTermMonths], [ModifiedBy], [ModifiedOn], [Name])
    VALUES (1, NULL, N''01'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''YANGIN''),
    (2, NULL, N''02'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''TİCARİ PAKET''),
    (3, NULL, N''10'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''NAKLİYAT EMTEA''),
    (4, NULL, N''15'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''ZORUNLU TRAFİK''),
    (5, NULL, N''16'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''KISMI KASKO''),
    (6, NULL, N''17'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''KASKO''),
    (7, NULL, N''19'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''FERDİ KAZA''),
    (8, NULL, N''20'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''İŞVEREN MALİ SORUMLULUK''),
    (9, NULL, N''21'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''3.ŞAHIS MALİ SORUMLULUK''),
    (10, NULL, N''22'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''ASANSÖR MALİ SORUMLULUK''),
    (11, NULL, N''23'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''CAM KIRILMASI''),
    (12, NULL, N''24'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''HIRSIZLIK''),
    (13, NULL, N''32'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''EMNİYETİ SUİSTİMAL''),
    (14, NULL, N''33'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''TAŞINAN PARA''),
    (15, NULL, N''34'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''SEYAHAT SAĞLIK''),
    (16, NULL, N''35'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''KREDİ KARTI''),
    (17, NULL, N''36'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''HAYAT KREDİ''),
    (18, NULL, N''37'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''MESLEKİ SORUMLULUK''),
    (19, NULL, N''39'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''TEHLİKELİ HASTALIKLAR''),
    (20, NULL, N''46'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''EĞİTİM GÜVENCESİ''),
    (21, NULL, N''65'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''MAKİNA KIRILMASI''),
    (22, NULL, N''66'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''MONTAJ''),
    (23, NULL, N''67'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''İNŞAAT''),
    (24, NULL, N''68'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, NULL, NULL, NULL, NULL, N''ELEKTRONİK CİHAZ'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Category', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'Description', N'IsActive', N'IsDeleted', N'MaximumPremium', N'MaximumTermMonths', N'MinimumPremium', N'MinimumTermMonths', N'ModifiedBy', N'ModifiedOn', N'Name') AND [object_id] = OBJECT_ID(N'[PolicyTypes]'))
        SET IDENTITY_INSERT [PolicyTypes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CityId', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Districts]'))
        SET IDENTITY_INSERT [Districts] ON;
    EXEC(N'INSERT INTO [Districts] ([Id], [CityId], [Code], [CreatedBy], [CreatedOn], [DeletedBy], [DeletedOn], [DisplayOrder], [IsActive], [IsDeleted], [ModifiedBy], [ModifiedOn], [NameEn], [NameTr])
    VALUES (1, 1, N''0101'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 1, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Köşklüçiftlik'', N''Köşklüçiftlik''),
    (2, 1, N''0102'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Ortaköy'', N''Ortaköy''),
    (3, 1, N''0103'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 3, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Gönyeli'', N''Gönyeli''),
    (4, 1, N''0104'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 4, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Hamitköy'', N''Hamitköy''),
    (5, 2, N''0201'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 1, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Tuzla'', N''Tuzla''),
    (6, 2, N''0202'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Salamis'', N''Salamis''),
    (7, 3, N''0301'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 1, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Çatalköy'', N''Çatalköy''),
    (8, 3, N''0302'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 2, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Lapta'', N''Lapta''),
    (9, 3, N''0303'', N''System'', ''2024-01-01T00:00:00.0000000Z'', NULL, NULL, 3, CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''Alsancak'', N''Alsancak'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CityId', N'Code', N'CreatedBy', N'CreatedOn', N'DeletedBy', N'DeletedOn', N'DisplayOrder', N'IsActive', N'IsDeleted', N'ModifiedBy', N'ModifiedOn', N'NameEn', N'NameTr') AND [object_id] = OBJECT_ID(N'[Districts]'))
        SET IDENTITY_INSERT [Districts] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Cities_Code] ON [Cities] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommissionRates_CurrencyId] ON [CommissionRates] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommissionRates_InsuranceCompanyId] ON [CommissionRates] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommissionRates_PolicyTypeId] ON [CommissionRates] ([PolicyTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Countries_Code] ON [Countries] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurrencyExchangeRates_FromCurrencyId] ON [CurrencyExchangeRates] ([FromCurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurrencyExchangeRates_ToCurrencyId] ON [CurrencyExchangeRates] ([ToCurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerInsuranceCompanies_CustomerId] ON [CustomerInsuranceCompanies] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerInsuranceCompanies_InsuranceCompanyId] ON [CustomerInsuranceCompanies] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_AllocationStatus] ON [CustomerPayments] ([AllocationStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_CurrencyId] ON [CustomerPayments] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_CustomerId] ON [CustomerPayments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_PaymentDate] ON [CustomerPayments] ([PaymentDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerPayments_Status] ON [CustomerPayments] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_CityId] ON [Customers] ([CityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_CustomerCode] ON [Customers] ([CustomerCode]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_DistrictId] ON [Customers] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_Email] ON [Customers] ([Email]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_IdentificationType_IdentificationNumber] ON [Customers] ([IdentificationType], [IdentificationNumber]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_NationalityCountryId] ON [Customers] ([NationalityCountryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_OccupationId] ON [Customers] ([OccupationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_SubdistrictId] ON [Customers] ([SubdistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_VillageId] ON [Customers] ([VillageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Districts_CityId_Code] ON [Districts] ([CityId], [Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportConfigurations_EnableAutoSync] ON [ImportConfigurations] ([EnableAutoSync]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportConfigurations_InsuranceCompanyId] ON [ImportConfigurations] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportConfigurations_IsActive] ON [ImportConfigurations] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ImportConfigurations_Name] ON [ImportConfigurations] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportConfigurations_SourceType] ON [ImportConfigurations] ([SourceType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_ImportConfigurationId] ON [ImportHistories] ([ImportConfigurationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_InsuranceCompanyId] ON [ImportHistories] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_SourceType] ON [ImportHistories] ([SourceType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_StartedAt] ON [ImportHistories] ([StartedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_StartedAt_Status] ON [ImportHistories] ([StartedAt], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ImportHistories_Status] ON [ImportHistories] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InsuranceCompanies_Code] ON [InsuranceCompanies] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_InvoiceId] ON [InvoiceItems] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_CurrencyId] ON [Invoices] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_CustomerId] ON [Invoices] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_PolicyId] ON [Invoices] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Occupations_Code] ON [Occupations] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_AllocationDate] ON [PaymentAllocations] ([AllocationDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_CustomerPaymentId] ON [PaymentAllocations] ([CustomerPaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_PolicyId] ON [PaymentAllocations] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_BranchCode] ON [Policies] ([BranchCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_CreatedOn] ON [Policies] ([CreatedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_CurrencyId] ON [Policies] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_CustomerId] ON [Policies] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_EndDate] ON [Policies] ([EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_InnerCode] ON [Policies] ([InnerCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_InsuranceCompanyId] ON [Policies] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Policies_InsuranceCompanyId_PolicyTypeId_PolicyNumber_InnerCode] ON [Policies] ([InsuranceCompanyId], [PolicyTypeId], [PolicyNumber], [InnerCode]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_OriginalPolicyId] ON [Policies] ([OriginalPolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_ParentPolicyId] ON [Policies] ([ParentPolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_PolicyTypeId] ON [Policies] ([PolicyTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_PreviousInsuranceCompanyId] ON [Policies] ([PreviousInsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_Status] ON [Policies] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_Status_CreatedOn] ON [Policies] ([Status], [CreatedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_Status_EndDate] ON [Policies] ([Status], [EndDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_VehicleId] ON [Policies] ([VehicleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_VehicleId1] ON [Policies] ([VehicleId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyClaims_PolicyId] ON [PolicyClaims] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyPayments_CurrencyId] ON [PolicyPayments] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyPayments_DueDate] ON [PolicyPayments] ([DueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyPayments_PaymentDate] ON [PolicyPayments] ([PaymentDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyPayments_PolicyId] ON [PolicyPayments] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyPayments_Status] ON [PolicyPayments] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]) WHERE [Name] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Subdistricts_DistrictId_Code] ON [Subdistricts] ([DistrictId], [Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TenantSettings_Category] ON [TenantSettings] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TenantSettings_SettingKey] ON [TenantSettings] ([SettingKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VehicleModels_BrandId] ON [VehicleModels] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_BrandId] ON [Vehicles] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_CurrencyId] ON [Vehicles] ([CurrencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_CustomerId] ON [Vehicles] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_ModelId] ON [Vehicles] ([ModelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Villages_SubdistrictId_Code] ON [Villages] ([SubdistrictId], [Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210085156_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260210085156_InitialCreate', N'9.0.5');
END;

COMMIT;
GO

