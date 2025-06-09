# IAMS - Multi-Tenant Insurance Agency Management System
## Architecture Documentation

### Executive Summary

The Insurance Agency Management System (IAMS) is a comprehensive multi-tenant SaaS solution designed specifically for insurance agencies in the Turkish Republic of Northern Cyprus. The system provides core insurance management functionality with optional modular extensions, ensuring agencies can scale their feature set based on their specific needs and budget.

---

## Table of Contents

1. [Business Overview](#business-overview)
2. [System Architecture](#system-architecture)
3. [Solution Structure](#solution-structure)
4. [Core Architectural Patterns](#core-architectural-patterns)
5. [Multi-Tenancy Design](#multi-tenancy-design)
6. [Security Architecture](#security-architecture)
7. [Data Architecture](#data-architecture)
8. [Module System](#module-system)
9. [Integration Strategy](#integration-strategy)
10. [Deployment Architecture](#deployment-architecture)
11. [Performance & Scalability](#performance--scalability)
12. [Development Guidelines](#development-guidelines)

---

## Business Overview

### Target Market
- **Primary Users**: Insurance agencies in Northern Cyprus
- **Agency Size**: Small to medium-sized agencies (5-50 employees)
- **Use Cases**: Customer management, policy administration, multi-company integrations, reporting, accounting

### Key Business Drivers
- **Multi-Company Support**: Agencies work with multiple insurance companies
- **Customer ID Mapping**: Agency customer IDs differ from insurance company customer IDs
- **Modular Pricing**: Basic package with optional premium modules
- **Regulatory Compliance**: Northern Cyprus insurance regulations
- **Multi-Language Support**: Turkish and English interfaces

### Business Value Proposition
- **Centralized Management**: Single system for managing relationships with multiple insurance companies
- **Scalable Pricing**: Pay-as-you-grow modular architecture
- **Data Isolation**: Complete tenant separation for data security and compliance
- **Integration Ready**: Built-in capability to connect with insurance company systems

---

## System Architecture

### Architectural Style
**Clean Architecture with Multi-Tenant SaaS Pattern**

The system follows Clean Architecture principles with clear separation of concerns, dependency inversion, and testability. The multi-tenant aspect is implemented at the infrastructure level, ensuring complete data isolation between agencies while maintaining a single application instance.

### Core Architectural Principles

#### 1. **Separation of Concerns**
Each layer has a distinct responsibility:
- **Domain Layer**: Business logic and entities
- **Application Layer**: Use cases and application services
- **Infrastructure Layer**: Data access and external integrations
- **Presentation Layer**: User interfaces and API endpoints

#### 2. **Dependency Inversion**
All dependencies flow inward toward the domain layer. Infrastructure depends on application abstractions, never the reverse.

#### 3. **Single Responsibility**
Each component, class, and module has a single, well-defined purpose.

#### 4. **Tenant Isolation**
Complete separation of tenant data, settings, and configurations while sharing the same application codebase.

#### 5. **Modular Design**
Core functionality with pluggable modules that can be enabled/disabled per tenant.

---

## Solution Structure

### Project Organization

#### **IAMS.Domain (Class Library)**
**Purpose**: Contains the core business logic and entities
**Responsibilities**:
- Domain entities (Customer, Policy, InsuranceCompany, etc.)
- Domain services and business rules
- Value objects and domain events
- Domain interfaces and contracts
- Business exceptions and validations

**Key Characteristics**:
- No external dependencies
- Pure C# business logic
- Framework-agnostic
- Contains invariants and business rules

#### **IAMS.Application (Class Library)**
**Purpose**: Orchestrates business operations and defines application boundaries
**Responsibilities**:
- Application services and use cases
- Data Transfer Objects (DTOs)
- Application interfaces (repositories, services)
- Input validation and mapping
- Business workflow coordination

**Key Characteristics**:
- Depends only on Domain layer
- Contains application logic (not business logic)
- Defines contracts for infrastructure
- Handles cross-cutting concerns coordination

#### **IAMS.Infrastructure (Class Library)**
**Purpose**: Implements external service integrations and technical concerns
**Responsibilities**:
- Email services and notifications
- File storage and document management
- External API integrations
- Logging and monitoring implementations
- Background job processing

**Key Characteristics**:
- Implements Application layer interfaces
- Contains framework-specific code
- Handles external dependencies
- No business logic

#### **IAMS.Persistence (Class Library)**
**Purpose**: Manages data access and database operations
**Responsibilities**:
- Entity Framework DbContext configurations
- Repository pattern implementations
- Database migrations and seeding
- Data access optimization
- Tenant-specific connection management

**Key Characteristics**:
- Implements repository interfaces from Application layer
- Contains database-specific code
- Manages tenant data isolation
- Handles connection string resolution

#### **IAMS.Identity (Class Library)**
**Purpose**: Handles authentication, authorization, and user management
**Responsibilities**:
- JWT token generation and validation
- User authentication flows
- Role-based access control (RBAC)
- Permission management
- Multi-tenant user isolation

**Key Characteristics**:
- Manages security concerns
- Implements tenant-aware user management
- Handles password policies and security
- Integrates with ASP.NET Core Identity

#### **IAMS.MultiTenancy (Class Library)**
**Purpose**: Provides multi-tenant infrastructure and tenant resolution
**Responsibilities**:
- Tenant identification and resolution
- Tenant context management
- Module enablement/disablement
- Tenant-specific settings management
- Subscription and billing support

**Key Characteristics**:
- Core multi-tenancy infrastructure
- Tenant lifecycle management
- Performance optimization through caching
- Subscription validation

#### **IAMS.Web (ASP.NET Core MVC/Blazor)**
**Purpose**: Provides the user interface for agency staff
**Responsibilities**:
- Web-based user interface
- Client-side validation
- User experience optimization
- Responsive design
- Accessibility compliance

**Key Characteristics**:
- Server-side rendered or Blazor components
- Integrates with API layer
- Handles user sessions
- Supports multiple languages

#### **IAMS.API (ASP.NET Core Web API)**
**Purpose**: Exposes application functionality through RESTful APIs
**Responsibilities**:
- REST API endpoints
- Request/response handling
- API documentation (Swagger)
- Rate limiting and throttling
- CORS configuration

**Key Characteristics**:
- Stateless design
- Comprehensive API documentation
- Versioning support
- Integration-ready endpoints

#### **IAMS.Shared (Class Library)**
**Purpose**: Contains shared utilities and common models
**Responsibilities**:
- Common constants and enumerations
- Shared helper methods
- Cross-cutting utilities
- Common extension methods

**Key Characteristics**:
- No business logic
- Utility functions only
- Minimal dependencies
- Reusable across projects

---

## Core Architectural Patterns

### Repository Pattern
**Purpose**: Abstracts data access logic and provides a consistent interface for data operations.

**Benefits**:
- Testability through interface abstraction
- Consistent data access patterns
- Separation of data access from business logic
- Support for multiple data sources

**Implementation Strategy**:
- Generic repository for common operations
- Specialized repositories for complex domain-specific queries
- Unit of Work pattern for transaction management

### Unit of Work Pattern
**Purpose**: Maintains a list of objects affected by business transactions and coordinates writing out changes.

**Benefits**:
- Transaction management
- Change tracking
- Atomic operations
- Performance optimization through batching

### Mediator Pattern (via MediatR)
**Purpose**: Defines how a set of objects interact with each other, promoting loose coupling.

**Benefits**:
- Decoupled request/response handling
- Cross-cutting concern management
- Pipeline behavior support
- Testable command/query separation

### Dependency Injection
**Purpose**: Implements Inversion of Control for managing object dependencies.

**Benefits**:
- Loose coupling
- Testability
- Configuration flexibility
- Lifecycle management

---

## Multi-Tenancy Design

### Tenant Isolation Strategy
**Database-per-Tenant Model**: Each tenant has a separate database for complete data isolation.

**Benefits**:
- Complete data isolation
- Independent scaling
- Backup and recovery per tenant
- Compliance and security

**Trade-offs**:
- Higher infrastructure costs
- More complex deployment
- Schema migration complexity

### Tenant Resolution
**Multi-Strategy Approach**:
1. **Subdomain-based**: tenant.yourdomain.com
2. **Header-based**: X-Tenant-ID header
3. **Path-based**: /api/tenant/endpoint
4. **Query parameter**: ?tenant=identifier

### Tenant Context Management
**Scoped Context**: Tenant information is available throughout the request lifecycle via dependency injection.

**Features**:
- Automatic tenant detection
- Context propagation
- Background task support
- Performance caching

### Module Management
**Per-Tenant Feature Flags**: Modules can be enabled/disabled per tenant for flexible pricing.

**Supported Modules**:
- **Core**: Basic customer and policy management (always enabled)
- **Reporting**: Advanced analytics and custom reports
- **Accounting**: Financial tracking and commission management
- **Integration**: Insurance company API integrations

---

## Security Architecture

### Authentication
**JWT-Based Authentication** with refresh token support

**Features**:
- Stateless authentication
- Automatic token refresh
- Session management
- Multi-device support

### Authorization
**Role-Based Access Control (RBAC)** with granular permissions

**Permission Levels**:
- **System-wide**: Administrative functions
- **Module-specific**: Feature access control
- **Data-level**: Record-specific permissions
- **Tenant-scoped**: All permissions are tenant-isolated

### Data Protection
**Multi-Layered Security**:
- Tenant data isolation
- Encryption at rest and in transit
- Audit logging
- Input validation and sanitization

---

## Data Architecture

### Master Database
**Purpose**: Stores tenant metadata and system configuration

**Contains**:
- Tenant registration and settings
- Module enablement flags
- Subscription information
- System-wide configurations

### Tenant Databases
**Purpose**: Stores tenant-specific application data

**Contains**:
- Customer information
- Policy data
- User accounts and permissions
- Audit logs and system data

### Data Flow
1. **Request arrives** → Tenant identification
2. **Tenant validation** → Access control check
3. **Connection resolution** → Tenant database selection
4. **Data operations** → Tenant-scoped queries
5. **Response** → Filtered tenant data only

---

## Module System

### Core Module (Always Enabled)
**Customer Management**:
- Customer CRUD operations
- Contact information management
- Customer search and filtering

**Policy Management**:
- Policy lifecycle management
- Insurance company mappings
- Basic reporting

**User Management**:
- User authentication
- Role assignment
- Basic permissions

### Optional Modules

#### **Reporting Module**
- Custom report builder
- Scheduled report generation
- Export capabilities (PDF, Excel, CSV)
- Advanced analytics dashboards
- Performance metrics

#### **Accounting Module**
- Commission tracking
- Financial reporting
- Invoice generation
- Payment processing integration
- Tax calculations

#### **Integration Module**
- Insurance company API connections
- Customer ID mapping and synchronization
- Policy data synchronization
- Document exchange
- Real-time status updates

---

## Integration Strategy

### Insurance Company Integrations
**Adapter Pattern**: Each insurance company has a specific adapter implementing a common interface.

**Integration Types**:
- **REST APIs**: Modern insurance companies
- **SOAP Services**: Legacy insurance systems
- **File-based**: CSV/XML file exchanges
- **Database connections**: Direct database access where available

### Customer ID Mapping
**Challenge**: Agency customer IDs differ from insurance company customer IDs

**Solution**: Mapping table linking agency customers to insurance company customer records
- One-to-many relationships (one agency customer, multiple insurance company records)
- Synchronization tracking
- Conflict resolution

---

## Deployment Architecture

### SaaS Deployment Model
**Centralized Application**: Single application instance serving multiple tenants

**Infrastructure Components**:
- **Load Balancer**: Traffic distribution and SSL termination
- **Application Servers**: Horizontally scalable web application instances
- **Master Database**: Tenant metadata and configuration
- **Tenant Databases**: Isolated data storage per tenant
- **Cache Layer**: Performance optimization
- **Background Services**: Async processing and integrations

### Environment Strategy
- **Development**: Single tenant for development and testing
- **Staging**: Multi-tenant environment for pre-production testing
- **Production**: Full multi-tenant deployment with monitoring

---

## Performance & Scalability

### Caching Strategy
**Multi-Level Caching**:
- **Tenant metadata**: In-memory caching for frequently accessed tenant information
- **Application data**: Redis for session and application data
- **Database query results**: Entity Framework query caching
- **Static content**: CDN for assets and documents

### Database Optimization
- **Connection pooling** per tenant
- **Read replicas** for reporting workloads
- **Indexing strategy** for multi-tenant queries
- **Partitioning** for large datasets

### Monitoring and Observability
- **Application Performance Monitoring (APM)**
- **Tenant-specific metrics**
- **Error tracking and alerting**
- **Resource utilization monitoring**

---

## Development Guidelines

### Code Organization
- **Feature-based folder structure** within each project
- **Consistent naming conventions** across all layers
- **Separation of concerns** at the method and class level
- **Dependency injection** for all external dependencies

### Testing Strategy
- **Unit tests** for business logic and services
- **Integration tests** for data access and API endpoints
- **End-to-end tests** for critical user workflows
- **Multi-tenant testing** with tenant isolation verification

### Documentation Requirements
- **API documentation** with Swagger/OpenAPI
- **Database schema documentation**
- **Deployment guides** for different environments
- **User manuals** for each module

### Quality Assurance
- **Code reviews** for all changes
- **Automated testing** in CI/CD pipeline
- **Security scanning** for vulnerabilities
- **Performance testing** for scalability validation

---

## Conclusion

The IAMS architecture provides a robust, scalable foundation for serving multiple insurance agencies while maintaining complete data isolation and flexible feature enablement. The clean architecture approach ensures maintainability and testability, while the multi-tenant design supports the SaaS business model effectively.

The modular system allows agencies to start with core functionality and expand as their needs grow, providing a clear path for business growth and feature adoption. The comprehensive security model ensures compliance with insurance industry regulations while providing the flexibility needed for diverse agency requirements.