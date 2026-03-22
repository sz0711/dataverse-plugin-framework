# 🚀 Dataverse Plugin Framework
[![License](https://img.shields.io/badge/license-MIT-green.svg)](#license)

> **Enterprise-Grade Plugin Architecture for Microsoft Dataverse/Dynamics 365**  
> Best practices, clean code, SOLID principles, and dependency injection in action.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Key Features](#key-features)
3. [Architecture](#architecture)
4. [Business Scenario](#business-scenario)
5. [Core Components](#core-components)
6. [Getting Started](#getting-started)
7. [Implementation Guide](#implementation-guide)
8. [Design Patterns](#design-patterns)
9. [Best Practices](#best-practices)
10. [Testing](#testing)

---

## Overview

**DataversePluginFramework** is a production-ready, reusable plugin development framework for Microsoft Dataverse and Dynamics 365. It demonstrates industry best practices through enterprise-grade architecture patterns and clean code principles.

This is a comprehensive, extensible framework designed to solve real-world plugin development challenges while maintaining code quality, testability, and maintainability.

---

## Key Features

✨ **Architecture Excellence**
- 🔧 **Generic Plugin Base Class** with early binding, type safety, and correlation tracing
- 🏗️ **Lightweight IoC Container** with greedy constructor injection, instance caching, and clear error messages
- 🎯 **Service-Oriented Architecture** with clean separation of concerns
- ⚙️ **Delegate-Based Rule Engine** for flexible business logic composition
- 🔒 **Centralized Constants** eliminating magic values across services
- ✅ **SOLID Principles** applied throughout the codebase
- 📦 **Reusable Shared Projects** for infrastructure and models
- 🧪 **14 Unit Tests** (xUnit + Moq) covering services and infrastructure
- 🛡️ **Resilient Error Handling** with non-critical operation isolation

---

## Architecture

### System Layered Architecture

```
┌─────────────────────────────────────────────────────────┐
│            DATAVERSE / DYNAMICS 365                      │
│           (Plugin Execution Pipeline)                    │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│        Plugin Entry Point Layer                          │
│    ┌────────────────────────────────────────┐           │
│    │    AccountPlugin : PluginBase<Account>  │           │
│    │    [Domain-Specific Plugin]             │           │
│    └────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│        Framework Base Layer (PLUGIN_INFRASTRUCTURE)     │
│    ┌────────────────────────────────────────┐           │
│    │    PluginBase<T>                        │           │
│    │    - Template Method Pattern            │           │
│    │    - Event routing (Pre/Post)           │           │
│    │    - Correlation tracing                │           │
│    │    - Service initialization             │           │
│    └────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│     Infrastructure & DI Container Layer                 │
│    ┌────────────────────────────────────────┐           │
│    │    PluginContainer (IoC)                │           │
│    │    - Service registration               │           │
│    │    - Greedy constructor injection       │           │
│    │    - Instance & constructor caching     │           │
│    │    - Dependency resolution              │           │
│    └────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                   │
        ┌──────────┼──────────┐
        │          │          │
┌───────▼───┐  ┌──▼──────┐  ┌▼──────────┐
│  Services │  │Constants│  │ Rule      │
│   Layer   │  │& Config │  │ Engine    │
│           │  │          │  │           │
│•IAccount- │  │•Plugin-  │  │•Rule<T>  │
│  Service  │  │ Constants│  │•Evaluate │
│•IRisk-    │  │•Service- │  │  All/Any │
│  Service  │  │  Regist- │  │           │
│•BaseService│ │  ration  │  │           │
└───────────┘  └─────────┘  └───────────┘
```

### Project Structure

```
DataversePluginFramework.sln
├── DataversePluginFramework/              [Domain-Specific Plugin Assembly]
│   ├── Plugins/
│   │   └── AccountPlugin.cs              ← Plugin entry point
│   ├── Infrastructure/
│   │   └── ServiceRegistration.cs        ← Centralized DI configuration
│   ├── Services/
│   │   ├── AccountServices/
│   │   │   ├── IAccountService.cs
│   │   │   └── AccountService.cs
│   │   └── Risk/
│   │       ├── IRiskService.cs
│   │       └── RiskService.cs
│   └── Properties/
│       └── AssemblyInfo.cs
│
├── DataversePluginFramework.Tests/        [Unit Test Project]
│   ├── Services/
│   │   ├── AccountServiceTests.cs        ← 6 tests
│   │   └── RiskServiceTests.cs           ← 4 tests
│   └── Infrastructure/
│       └── PluginContainerTests.cs        ← 4 tests
│
├── MODEL/                                 [Shared - Early-Bound Models]
│   └── Model/
│       ├── Entities/ (account.cs, contact.cs)
│       ├── OptionSets/
│       ├── OrgContext.cs
│       └── EntityOptionSetEnum.cs
│
└── PLUGIN_INFRASTRUCTURE/                 [Shared - Reusable Framework]
    ├── Constants/PluginConstants.cs        ← Centralized business constants
    ├── Infrastructure/PluginContainer.cs   ← IoC container
    ├── Plugins/PluginBase.cs              ← Generic plugin base
    ├── Services/BaseService.cs            ← Service foundation
    └── Rules/RuleEngine.cs               ← Composable rule engine
```

---

## Business Scenario

### Strategic Customer Classification

**Rule:** If Account Revenue > €5,000,000 → Mark as "Strategic Customer"

The threshold and description label are centralized in `PluginConstants` to avoid magic values.

### Execution Flow

```
Dataverse Event (Account Create/Update)
    ↓
AccountPlugin.Execute()
    ↓
PluginBase<Account>
    ├── Initialize SDK services
    ├── Log: [AccountPlugin] Start: Create | Stage=20 | Entity=... | Correlation=...
    └── Route → OnPreOperationCreate()
    ↓
PluginContainer Setup (IoC)
    │
    ├── RegisterInstance: IOrganizationService
    ├── RegisterInstance: ITracingService
    ├── Register: IAccountService → AccountService
    └── Register: IRiskService → RiskService
    ↓
AccountService.Process(Account)
    ├── Guard: null account / null revenue → trace & return
    ├── Evaluate: Revenue > PluginConstants.StrategicRevenueThreshold?
    ├── Set: Description = PluginConstants.StrategicCustomerDescription
    └── try { RiskService.Evaluate(account) }
        catch { trace warning, continue }   ← non-critical
```

---

## Core Components

### 1️⃣ PluginBase\<T\> - Generic Plugin Framework

**Location:** `PLUGIN_INFRASTRUCTURE/Plugins/PluginBase.cs`

A strongly-typed, template method-based plugin base class supporting any entity type.

**Features:**
- 🔄 **Template Method Pattern** for event routing (PreValidation/PreOperation/PostOperation × Create/Update/Delete)
- 🎯 **Generic Type Safety** via `PluginBase<T> where T : Entity`
- 📦 **Automatic SDK Service Initialization** (ExecutionContext, OrgService, TracingService)
- 🔗 **IoC Container Integration** for service resolution
- 📊 **Correlation Tracing** — every execution logs `PluginName`, `MessageName`, `Stage`, `EntityId`, and `CorrelationId`
- 🛡️ **Two-Level Exception Handling** — route-level catch with context info + top-level FATAL catch

**Key Properties & Methods:**
```csharp
protected PluginContainer Container { get; }           // IoC container
protected IPluginExecutionContext Context { get; }     // Execution context
protected ITracingService Tracing { get; }             // Tracing service
protected T CurrentRecord { get; }                      // Strongly-typed entity

// Event handlers (override in derived plugins)
protected virtual void OnPreOperationCreate() { }
protected virtual void OnPreOperationUpdate() { }
protected virtual void OnPreOperationDelete() { }
protected virtual void OnPostOperationCreate() { }
protected virtual void OnPostOperationUpdate() { }
protected virtual void OnPostOperationDelete() { }

// Service registration hook (Template Method)
protected virtual void RegisterDomainServices(PluginContainer container) { }
```

**Tracing Output:**
```
[AccountPlugin] Start: Create | Stage=20 | Entity=a1b2c3d4-... | Correlation=e5f6a7b8-...
[AccountPlugin] Error in Create Stage=20: System.InvalidOperationException: ...
[AccountPlugin] PluginBase.Execute FATAL: System.Exception: ...
```

---

### 2️⃣ PluginContainer - Lightweight IoC Container

**Location:** `PLUGIN_INFRASTRUCTURE/Infrastructure/PluginContainer.cs`

A simple, efficient dependency injection container without external dependencies.

**Features:**
- ✅ No external dependencies
- ✅ **Greedy constructor selection** — picks the constructor with the most parameters
- ✅ **Constructor caching** — `ConstructorInfo` resolved once per type via `_ctorCache`
- ✅ **Instance caching** — resolved services are cached for the plugin execution lifetime
- ✅ **Descriptive error messages** — on resolution failure, logs parameter name, type, and target
- ✅ Fallback to `IServiceProvider` for SDK services

```csharp
container.Register<IAccountService, AccountService>();
container.RegisterInstance<IOrganizationService>(orgService);

var service = container.Resolve<IAccountService>();
// → Greedy ctor: selects AccountService(IOrganizationService, ITracingService, IRiskService)
// → Resolved instance is cached for subsequent Resolve<IAccountService>() calls
```

---

### 3️⃣ PluginConstants - Centralized Business Constants

**Location:** `PLUGIN_INFRASTRUCTURE/Constants/PluginConstants.cs`

Single source of truth for magic values used across services.

```csharp
public static class PluginConstants
{
    public const decimal StrategicRevenueThreshold = 5_000_000m;
    public const string StrategicCustomerDescription = "Strategic Customer";
}
```

All services reference these constants instead of hard-coded values, making threshold changes a one-line edit.

---

### 4️⃣ BaseService - Service Foundation

**Location:** `PLUGIN_INFRASTRUCTURE/Services/BaseService.cs`

The abstract base class for all domain services, providing SDK service access.

```csharp
public abstract class BaseService
{
    protected IOrganizationService Service { get; }
    protected ITracingService Trace { get; }
    
    protected BaseService(
        IOrganizationService service,
        ITracingService trace)
}
```

---

### 5️⃣ RuleEngine\<T\> - Composable Business Rules

**Location:** `PLUGIN_INFRASTRUCTURE/Rules/RuleEngine.cs`

A flexible, delegate-based rule engine for evaluating business logic.

```csharp
public class RuleEngine<T> where T : Entity
{
    public delegate bool Rule(T entity);
    
    public bool Evaluate(T entity, Rule rule)              // Single rule
    public bool EvaluateAll(T entity, params Rule[] rules) // AND logic
    public bool EvaluateAny(T entity, params Rule[] rules) // OR logic
}
```

**Example:**
```csharp
var engine = new RuleEngine<Account>();
RuleEngine<Account>.Rule isStrategic = 
    account => account.Revenue?.Value > PluginConstants.StrategicRevenueThreshold;
bool result = engine.Evaluate(account, isStrategic);
```

---

### 6️⃣ ServiceRegistration - Centralized DI Configuration

**Location:** `DataversePluginFramework/Infrastructure/ServiceRegistration.cs`

Single source of truth for all service-to-implementation mappings.

```csharp
public static class ServiceRegistration
{
    public static void RegisterServices(PluginContainer container)
    {
        container.Register<IAccountService, AccountService>();
        container.Register<IRiskService, RiskService>();
    }
}
```

---

### 7️⃣ AccountService & RiskService - Business Logic

**AccountService:** Evaluates account revenue, applies strategic classification, and delegates risk assessment with resilient error handling.

```csharp
public class AccountService : BaseService, IAccountService
{
    private readonly IRiskService _risk;

    public void Process(Account account)
    {
        if (account == null) { Trace.Trace("..."); return; }
        if (account.Revenue == null) { Trace.Trace("..."); return; }

        if (account.Revenue.Value > PluginConstants.StrategicRevenueThreshold)
        {
            account.Description = PluginConstants.StrategicCustomerDescription;

            try
            {
                _risk.Evaluate(account);
            }
            catch (Exception ex)
            {
                Trace.Trace($"AccountService: Risk evaluation failed: {ex.Message}");
                // Risk evaluation is non-critical; processing continues.
            }
        }
    }
}
```

**RiskService:** Handles financial risk assessment.

```csharp
public class RiskService : BaseService, IRiskService
{
    public void Evaluate(Account account)
    {
        if (account == null) return;

        if (account.Revenue?.Value > PluginConstants.StrategicRevenueThreshold)
        {
            Trace.Trace($"High-value account risk assessment: {account.Name}");
        }
    }
}
```

---

## Getting Started

### Prerequisites
- ✅ Visual Studio 2019+ (2022 recommended)
- ✅ .NET Framework 4.6.2
- ✅ Microsoft Dataverse SDK (CRM SDK) 9.0.2+

### Quick Start

#### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/DataversePluginFramework.git
cd DataversePluginFramework
```

#### 2. Restore & Build
```bash
nuget restore DataversePluginFramework.sln
msbuild DataversePluginFramework.sln -verbosity:minimal
```

> **Note:** This is a .NET Framework 4.6.2 solution using `packages.config`. Use `nuget.exe` + MSBuild (not `dotnet build`).

The compiled plugin assembly: `DataversePluginFramework\bin\Debug\DataversePluginFramework.dll`

#### 3. Run Tests
```bash
vstest.console.exe DataversePluginFramework.Tests\bin\Debug\DataversePluginFramework.Tests.dll ^
  /TestAdapterPath:packages\xunit.runner.visualstudio.2.8.2\build\net462
```

Expected: **14 tests passed** (6 AccountService + 4 RiskService + 4 PluginContainer).

#### 4. Deploy to Dataverse

Use the **Plugin Registration Tool**:
1. Assembly: `DataversePluginFramework.dll`
2. Step: `DataversePluginFramework.Plugins.AccountPlugin`
3. Message: `Create` / `Update`
4. Entity: `Account`
5. Stage: `Pre-Operation`

---

## Implementation Guide

### Creating a New Plugin

**Step 1: Define Service Interface**
```csharp
public interface IMyService
{
    void Execute(MyEntity entity);
}

public class MyService : BaseService, IMyService
{
    public MyService(IOrganizationService svc, ITracingService trace)
        : base(svc, trace) { }

    public void Execute(MyEntity entity)
    {
        Trace.Trace($"Processing: {entity.Name}");
    }
}
```

**Step 2: Register Service**
```csharp
// In ServiceRegistration.cs
container.Register<IMyService, MyService>();
```

**Step 3: Create Plugin Class**
```csharp
public class MyPlugin : PluginBase<MyEntity>
{
    protected override void RegisterDomainServices(PluginContainer container)
    {
        ServiceRegistration.RegisterServices(container);
    }

    protected override void OnPreOperationCreate()
    {
        var service = Container.Resolve<IMyService>();
        service.Execute(CurrentRecord);
    }
}
```

---

## Design Patterns

### 1. Template Method Pattern
The `PluginBase<T>` class enforces a standard execution flow while allowing derived classes to customize specific steps via `OnPreOperationCreate()`, `OnPostOperationUpdate()`, etc.

### 2. Dependency Injection
Services registered in `ServiceRegistration.cs` are resolved through the IoC container, promoting loose coupling and testability. The container uses a **greedy constructor** strategy for automatic resolution.

### 3. Strategy Pattern (via RuleEngine)
Business rules as delegate strategies enable flexible rule composition without inheritance hierarchies.

### 4. Singleton Pattern
The IoC container caches resolved instances, ensuring consistent state across a single plugin execution.

---

## Best Practices

### ✅ DO

**Use Centralized Constants:**
```csharp
// PluginConstants.cs — single source of truth
if (account.Revenue.Value > PluginConstants.StrategicRevenueThreshold)
{
    account.Description = PluginConstants.StrategicCustomerDescription;
}
```

**Constructor Injection:**
```csharp
public class AccountService : BaseService
{
    private readonly IRiskService _risk;
    
    public AccountService(
        IOrganizationService svc,
        ITracingService trace,
        IRiskService risk)
        : base(svc, trace)
```

**Isolate Non-Critical Operations:**
```csharp
try { _risk.Evaluate(account); }
catch (Exception ex)
{
    Trace.Trace($"Risk evaluation failed: {ex.Message}");
    // Continue — risk is non-critical
}
```

**Comprehensive Tracing (with context):**
```csharp
Trace.Trace($"AccountService: Strategic account detected: {account.Name} with revenue {account.Revenue.Value}");
```

### ❌ DON'T

**Hard-Code Magic Values:**
```csharp
// BAD — scattered magic values are unmaintainable
if (account.Revenue?.Value > 5000000)
    account.Description = "Strategic Customer";
```

**Hidden Dependencies:**
```csharp
public void SetRisk(IRiskService risk) // Property injection is bad
{
    _risk = risk;
}
```

**Skip Tracing:**
```csharp
// No logging at all — impossible to debug in production!
account.Description = "Strategic Customer";
```

---

## Testing

The solution includes a dedicated test project (`DataversePluginFramework.Tests`) with **14 xUnit tests** covering all service and infrastructure logic.

### Test Overview

| Test Class | Tests | Covers |
|------------|-------|--------|
| `AccountServiceTests` | 6 | Null handling, revenue thresholds, strategic marking, risk isolation |
| `RiskServiceTests` | 4 | Null handling, revenue-based tracing |
| `PluginContainerTests` | 4 | Instance registration, constructor DI, fallback, null resolution |

### Example: AccountService Tests

```csharp
public class AccountServiceTests
{
    private readonly Mock<IOrganizationService> _orgService;
    private readonly Mock<ITracingService> _tracing;
    private readonly Mock<IRiskService> _riskService;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _orgService = new Mock<IOrganizationService>();
        _tracing = new Mock<ITracingService>();
        _riskService = new Mock<IRiskService>();
        _sut = new AccountService(_orgService.Object, _tracing.Object, _riskService.Object);
    }

    [Fact]
    public void Process_RevenueAboveThreshold_MarksStrategicAndEvaluatesRisk()
    {
        var account = new Account
        {
            Name = "BigCorp",
            Revenue = new Money(10_000_000m)
        };

        _sut.Process(account);

        Assert.Equal(PluginConstants.StrategicCustomerDescription, account.Description);
        _riskService.Verify(r => r.Evaluate(account), Times.Once);
    }

    [Fact]
    public void Process_RiskServiceThrows_ContinuesWithoutRethrowing()
    {
        var account = new Account
        {
            Name = "RiskyCo",
            Revenue = new Money(10_000_000m)
        };
        _riskService.Setup(r => r.Evaluate(It.IsAny<Account>()))
            .Throws(new InvalidOperationException("Risk engine failure"));

        // Should NOT throw — risk evaluation is non-critical
        _sut.Process(account);

        Assert.Equal(PluginConstants.StrategicCustomerDescription, account.Description);
        _tracing.Verify(
            t => t.Trace(It.Is<string>(s => s.Contains("Risk evaluation failed"))),
            Times.Once);
    }
}
```

---

## SOLID Principles in Practice

| Principle | Implementation | Benefit |
|-----------|----------------|---------|
| **S**ingle Responsibility | Each service has one clear job | Easy to understand, modify |
| **O**pen/Closed | New rules added without changing code | Extensible without breaking |
| **L**iskov Substitution | Services are interchangeable | Flexible testing & composition |
| **I**nterface Segregation | Small, focused interfaces | Services don't depend on unused methods |
| **D**ependency Inversion | IoC container resolves dependencies | Loosely coupled, testable code |

---

## NuGet Dependencies

### Plugin Assembly

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.CrmSdk.CoreAssemblies | 9.0.2.51 | Dataverse SDK |
| Microsoft.CrmSdk.Workflow | 9.0.2.42 | Workflow integration |
| Microsoft.CrmSdk.XrmTooling.CoreAssembly | 9.1.1.65 | XrmTooling services |

### Test Project

| Package | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.9.3 | Test framework |
| Moq | 4.20.72 | Mocking library |
| Castle.Core | 5.1.1 | Moq runtime dependency |
| xunit.runner.visualstudio | 2.8.2 | VS Test adapter |

---

## Learning Resources

- [Microsoft Dataverse Documentation](https://docs.microsoft.com/en-us/power-apps/developer/data-platform/)
- [Dynamics 365 Plugin Development Best Practices](https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/plug-ins)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Design Patterns in C#](https://refactoring.guru/design-patterns)

---

**⭐ If you find this framework helpful, consider starring the repository!**

*Last Updated: March 2026 | Framework Version: 2.0*

## ⚠️ Disclaimer

This repository is a private project.

It is provided without any warranty, guarantee, or representation of any kind.
Use is entirely at your own risk. The author accepts no liability for direct or
indirect damages, data loss, outages, or any other consequences resulting from
the use of this project.

---
