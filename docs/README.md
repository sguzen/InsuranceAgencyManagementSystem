# Insurance Agency Management System - Documentation

Welcome to the IAMS documentation. This folder contains comprehensive guides and references for the system architecture, deployments, operations, and development guides.

---

## 📚 Documentation Index

### 🏗️ Architecture
Documents detailing the core system design, project structure, and the calculation engine logic.

* **[System Architecture](architecture/SystemArchitecture.md)**: High-level system design and architectural reviews.
* **[Calculation Engine](architecture/CalculationEngine.md)**: Complete architecture for premium calculators, commission rates, and claim validations for all policy types.
* **[Project Structure](architecture/ProjectStructure.md)**: Repository layout and folder structure overview.

### 🚀 Deployment
Everything you need to know about getting IAMS into production.

* **[Deployment Guide](deployment/DeploymentGuide.md)**: Comprehensive guide for deploying IAMS, including single-tenant and on-premise strategies.
* **[Deployment Checklist](deployment/DeploymentChecklist.md)**: Step-by-step checklist to ensure a smooth deployment.
* **[Production Readiness](deployment/ProductionReadiness.md)**: Status and readiness report for production.

### 📖 Guides
Development guides, best practices, and tutorials.

* **[Quick Reference](guides/QuickReference.md)**: Quick start instructions, summary of changes, common tasks, and troubleshooting.
* **[Exception Handling](guides/ExceptionHandling.md)**: Custom domain exceptions, result patterns, and generic exception anti-patterns.
* **[Policy Import](guides/PolicyImport.md)**: How the policy import and endorsement features work.
* **[Refactoring Guide](guides/RefactoringGuide.md)**: Breaking down God components, CQRS with MediatR, and removing the service layer anti-pattern.
* **[Roles And Permissions](guides/RolesAndPermissions.md)**: Guide on configuring and applying roles within the system.

### ⚙️ Operations
Guides for maintaining the system, optimizing performance, and analyzing logs.

* **[Logging Guide](operations/LoggingGuide.md)**: Detailed overview of logging implementation, improvements, and quick reference.
* **[Performance Optimizations](operations/PerformanceOptimizations.md)**: Summary of performance upgrades and recommendations.

---

## 🚀 Getting Started Checklist

For new developers or those just pulling the repository:

- [ ] Read the [Quick Reference](guides/QuickReference.md)
- [ ] Configure User Secrets
- [ ] Run tests: `dotnet test`
- [ ] Review [System Architecture](architecture/SystemArchitecture.md)
- [ ] Bookmark this README for future reference

---

## 📞 Additional Resources

### Within This Repository
- `/src` - Source code with improvements applied
- `/tests` - Unit tests
- `/docs` - This documentation folder

### Code Comments
Explanatory comments can be found in:
- `ApplicationDbContext.cs` - Audit timestamp logic
- `ServiceCollectionExtensions.cs` - Lifetime choices
- `ApplicationConstants.cs` - Constant organization

---

## 🔄 Keep This Updated

When adding new documentation:

1. Place the markdown file in the appropriate `/docs` subdirectory (`architecture/`, `deployment/`, `guides/`, or `operations/`).
2. Update this README with a link in the appropriate section.
3. Commit with a descriptive message.
