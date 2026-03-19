USE [TenantDb]
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
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE TABLE [Tenants] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Identifier] nvarchar(100) NOT NULL,
        [ConnectionString] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        [SubscriptionPlan] nvarchar(50) NOT NULL,
        [SubscriptionExpiry] datetime2 NULL,
        [MaxUsers] int NOT NULL,
        [MaxStorageBytes] bigint NOT NULL,
        [ContactEmail] nvarchar(200) NOT NULL,
        [ContactPhone] nvarchar(50) NOT NULL,
        [TimeZone] nvarchar(50) NOT NULL,
        [Currency] nvarchar(10) NOT NULL,
        [Language] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE TABLE [TenantModules] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ModuleName] nvarchar(100) NOT NULL,
        [IsEnabled] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        CONSTRAINT [PK_TenantModules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantModules_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE TABLE [TenantSettings] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [SettingKey] nvarchar(100) NOT NULL,
        [SettingValue] nvarchar(1000) NOT NULL,
        [SettingType] nvarchar(50) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastUpdated] datetime2 NULL,
        CONSTRAINT [PK_TenantSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantSettings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConnectionString', N'ContactEmail', N'ContactPhone', N'CreatedOn', N'Currency', N'Identifier', N'IsActive', N'Language', N'LastUpdated', N'MaxStorageBytes', N'MaxUsers', N'Name', N'SubscriptionExpiry', N'SubscriptionPlan', N'TimeZone') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] ON;
    EXEC(N'INSERT INTO [Tenants] ([Id], [ConnectionString], [ContactEmail], [ContactPhone], [CreatedOn], [Currency], [Identifier], [IsActive], [Language], [LastUpdated], [MaxStorageBytes], [MaxUsers], [Name], [SubscriptionExpiry], [SubscriptionPlan], [TimeZone])
    VALUES (1, N''Server=(localdb)\mssqllocaldb;Database=IAMS_Default;Trusted_Connection=true;MultipleActiveResultSets=true'', N''admin@default-agency.com'', N'''', ''2025-07-02T08:36:04.4295451Z'', N''TRY'', N''default'', CAST(1 AS bit), N''tr'', NULL, CAST(5368709120 AS bigint), 50, N''Default Insurance Agency'', NULL, N''Premium'', N''Europe/Istanbul'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConnectionString', N'ContactEmail', N'ContactPhone', N'CreatedOn', N'Currency', N'Identifier', N'IsActive', N'Language', N'LastUpdated', N'MaxStorageBytes', N'MaxUsers', N'Name', N'SubscriptionExpiry', N'SubscriptionPlan', N'TimeZone') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedOn', N'IsEnabled', N'LastUpdated', N'ModuleName', N'TenantId') AND [object_id] = OBJECT_ID(N'[TenantModules]'))
        SET IDENTITY_INSERT [TenantModules] ON;
    EXEC(N'INSERT INTO [TenantModules] ([Id], [CreatedOn], [IsEnabled], [LastUpdated], [ModuleName], [TenantId])
    VALUES (1, ''2025-07-02T08:36:04.4310996Z'', CAST(1 AS bit), NULL, N''Policy'', 1),
    (2, ''2025-07-02T08:36:04.4311325Z'', CAST(1 AS bit), NULL, N''Customer'', 1),
    (3, ''2025-07-02T08:36:04.4311326Z'', CAST(1 AS bit), NULL, N''Reporting'', 1),
    (4, ''2025-07-02T08:36:04.4311327Z'', CAST(1 AS bit), NULL, N''Accounting'', 1),
    (5, ''2025-07-02T08:36:04.4311328Z'', CAST(1 AS bit), NULL, N''Integration'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedOn', N'IsEnabled', N'LastUpdated', N'ModuleName', N'TenantId') AND [object_id] = OBJECT_ID(N'[TenantModules]'))
        SET IDENTITY_INSERT [TenantModules] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TenantModules_TenantId_ModuleName] ON [TenantModules] ([TenantId], [ModuleName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tenants_Identifier] ON [Tenants] ([Identifier]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TenantSettings_TenantId_SettingKey] ON [TenantSettings] ([TenantId], [SettingKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702083605_initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702083605_initial', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [ConnectionString] = N''Data Source=localhost;Initial Catalog=TenantDb;Integrated Security=True;Trust Server Certificate=True'', [CreatedOn] = ''2024-01-01T00:00:00.0000000Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702084409_initial2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702084409_initial2', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227090234_agencynumber'
)
BEGIN
    DROP TABLE [TenantSettings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227090234_agencynumber'
)
BEGIN
    ALTER TABLE [Tenants] ADD [ExternalId] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227090234_agencynumber'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [ExternalId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227090234_agencynumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251227090234_agencynumber', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [MaxPolicies] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [ModuleSettings] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Settings] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [SubscriptionType] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    ALTER TABLE [Tenants] ADD [TrialExpiryDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    CREATE TABLE [AgencyInsuranceCompanies] (
        [Id] int NOT NULL IDENTITY,
        [AgencyId] int NOT NULL,
        [InsuranceCompanyId] int NOT NULL,
        [InsuranceCompanyName] nvarchar(100) NOT NULL,
        [InsuranceCompanyCode] nvarchar(50) NOT NULL,
        [DbServer] nvarchar(200) NULL,
        [DbName] nvarchar(200) NULL,
        [DbUsername] nvarchar(200) NULL,
        [DbPassword] nvarchar(500) NULL,
        [ConnectionString] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedOn] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_AgencyInsuranceCompanies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AgencyInsuranceCompanies_Tenants_AgencyId] FOREIGN KEY ([AgencyId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [ExternalId] = N''A001'', [MaxPolicies] = 20000, [ModuleSettings] = NULL, [Settings] = NULL, [Status] = 0, [SubscriptionType] = 2, [TrialExpiryDate] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AgencyInsuranceCompanies_AgencyId_InsuranceCompanyId] ON [AgencyInsuranceCompanies] ([AgencyId], [InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206112816_tennants'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260206112816_tennants', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    CREATE TABLE [InsuranceCompanies] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [Website] nvarchar(200) NULL,
        [ApiEndpoint] nvarchar(200) NULL,
        [IntegrationSettings] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_InsuranceCompanies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApiEndpoint', N'Code', N'CreatedBy', N'CreatedOn', N'DisplayOrder', N'IntegrationSettings', N'IsActive', N'LogoUrl', N'ModifiedBy', N'ModifiedOn', N'Name', N'Website') AND [object_id] = OBJECT_ID(N'[InsuranceCompanies]'))
        SET IDENTITY_INSERT [InsuranceCompanies] ON;
    EXEC(N'INSERT INTO [InsuranceCompanies] ([Id], [ApiEndpoint], [Code], [CreatedBy], [CreatedOn], [DisplayOrder], [IntegrationSettings], [IsActive], [LogoUrl], [ModifiedBy], [ModifiedOn], [Name], [Website])
    VALUES (1, NULL, N''ANADOLU'', NULL, ''2024-01-01T00:00:00.0000000Z'', 1, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Anadolu Sigorta'', NULL),
    (2, NULL, N''ALLIANZ'', NULL, ''2024-01-01T00:00:00.0000000Z'', 2, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Allianz Sigorta'', NULL),
    (3, NULL, N''AXA'', NULL, ''2024-01-01T00:00:00.0000000Z'', 3, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Axa Sigorta'', NULL),
    (4, NULL, N''AKSIGORTA'', NULL, ''2024-01-01T00:00:00.0000000Z'', 4, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Aksigorta'', NULL),
    (5, NULL, N''SOMPO'', NULL, ''2024-01-01T00:00:00.0000000Z'', 5, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Sompo Sigorta'', NULL),
    (6, NULL, N''MAPFRE'', NULL, ''2024-01-01T00:00:00.0000000Z'', 6, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Mapfre Sigorta'', NULL),
    (7, NULL, N''HDI'', NULL, ''2024-01-01T00:00:00.0000000Z'', 7, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''HDI Sigorta'', NULL),
    (8, NULL, N''ZURICH'', NULL, ''2024-01-01T00:00:00.0000000Z'', 8, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Zurich Sigorta'', NULL),
    (9, NULL, N''GROUPAMA'', NULL, ''2024-01-01T00:00:00.0000000Z'', 9, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Groupama Sigorta'', NULL),
    (10, NULL, N''TURKIYE'', NULL, ''2024-01-01T00:00:00.0000000Z'', 10, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Türkiye Sigorta'', NULL),
    (11, NULL, N''GENERALI'', NULL, ''2024-01-01T00:00:00.0000000Z'', 11, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Generali Sigorta'', NULL),
    (12, NULL, N''DOGA'', NULL, ''2024-01-01T00:00:00.0000000Z'', 12, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Doga Sigorta'', NULL),
    (13, NULL, N''NEOVA'', NULL, ''2024-01-01T00:00:00.0000000Z'', 13, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Neova Sigorta'', NULL),
    (14, NULL, N''QUICK'', NULL, ''2024-01-01T00:00:00.0000000Z'', 14, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Quick Sigorta'', NULL),
    (15, NULL, N''HEPIYI'', NULL, ''2024-01-01T00:00:00.0000000Z'', 15, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Hepiyi Sigorta'', NULL),
    (16, NULL, N''MAGDEBURGER'', NULL, ''2024-01-01T00:00:00.0000000Z'', 16, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Magdeburger Sigorta'', NULL),
    (17, NULL, N''KORU'', NULL, ''2024-01-01T00:00:00.0000000Z'', 17, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Koru Sigorta'', NULL),
    (18, NULL, N''TURKNIPPON'', NULL, ''2024-01-01T00:00:00.0000000Z'', 18, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Turk Nippon Sigorta'', NULL),
    (19, NULL, N''CORPUS'', NULL, ''2024-01-01T00:00:00.0000000Z'', 19, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Corpus Sigorta'', NULL),
    (20, NULL, N''ORIENT'', NULL, ''2024-01-01T00:00:00.0000000Z'', 20, NULL, CAST(1 AS bit), NULL, NULL, NULL, N''Orient Sigorta'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApiEndpoint', N'Code', N'CreatedBy', N'CreatedOn', N'DisplayOrder', N'IntegrationSettings', N'IsActive', N'LogoUrl', N'ModifiedBy', N'ModifiedOn', N'Name', N'Website') AND [object_id] = OBJECT_ID(N'[InsuranceCompanies]'))
        SET IDENTITY_INSERT [InsuranceCompanies] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    CREATE INDEX [IX_AgencyInsuranceCompanies_InsuranceCompanyId] ON [AgencyInsuranceCompanies] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InsuranceCompanies_Code] ON [InsuranceCompanies] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    ALTER TABLE [AgencyInsuranceCompanies] ADD CONSTRAINT [FK_AgencyInsuranceCompanies_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260206164936_insuranceComp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260206164936_insuranceComp', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209044347_insuranceComp2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209044347_insuranceComp2', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209092043_importjobs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209092043_importjobs', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209093021_importjobs2'
)
BEGIN
    CREATE TABLE [ImportJobs] (
        [Id] int NOT NULL IDENTITY,
        [AgencyId] int NOT NULL,
        [InsuranceCompanyId] int NOT NULL,
        [Status] int NOT NULL,
        [StartedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [TotalRecords] int NOT NULL,
        [ImportedRecords] int NOT NULL,
        [FailedRecords] int NOT NULL,
        [SkippedRecords] int NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ImportLog] nvarchar(max) NULL,
        [FilterStartDate] datetime2 NULL,
        [FilterEndDate] datetime2 NULL,
        [RequestedBy] nvarchar(256) NOT NULL,
        [CreatedBy] nvarchar(256) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedBy] nvarchar(256) NULL,
        [ModifiedOn] datetime2 NULL,
        CONSTRAINT [PK_ImportJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ImportJobs_InsuranceCompanies_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [InsuranceCompanies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ImportJobs_Tenants_AgencyId] FOREIGN KEY ([AgencyId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209093021_importjobs2'
)
BEGIN
    CREATE INDEX [IX_ImportJobs_AgencyId] ON [ImportJobs] ([AgencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209093021_importjobs2'
)
BEGIN
    CREATE INDEX [IX_ImportJobs_InsuranceCompanyId] ON [ImportJobs] ([InsuranceCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209093021_importjobs2'
)
BEGIN
    CREATE INDEX [IX_ImportJobs_Status] ON [ImportJobs] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209093021_importjobs2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209093021_importjobs2', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    ALTER TABLE [Tenants] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    ALTER TABLE [TenantModules] ADD [ModifiedOn] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [ModifiedOn] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [ModifiedOn] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [ModifiedOn] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [ModifiedOn] = NULL
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [TenantModules] SET [ModifiedOn] = NULL
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [ConnectionString] = N''Data Source=localhost;Initial Catalog=DefaultAgencyDb;Integrated Security=True;Trust Server Certificate=True'', [IsDeleted] = CAST(0 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209215807_importjobs3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209215807_importjobs3', N'9.0.5');
END;

COMMIT;
GO

