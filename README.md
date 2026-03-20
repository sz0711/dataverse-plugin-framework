# 🚀 Dataverse Plugin Framework

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
- 🔧 **Generic Plugin Base Class** with early binding and type safety
- 🏗️ **Lightweight IoC Container** with constructor-based dependency injection
- 🎯 **Service-Oriented Architecture** with clean separation of concerns
- ⚙️ **Delegate-Based Rule Engine** for flexible business logic composition
- ✅ **SOLID Principles** applied throughout the codebase
- 📦 **Reusable Shared Projects** for infrastructure and models
- 🛡️ **Thread-Safe** and production-tested patterns

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
│    │    - Service initialization             │           │
│    └────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│     Infrastructure & DI Container Layer                 │
│    ┌────────────────────────────────────────┐           │
│    │    PluginContainer (IoC)                │           │
│    │    - Service registration               │           │
│    │    - Constructor injection              │           │
│    │    - Dependency resolution              │           │
│    └────────────┬───────────────────────────┘           │
└─────────────────┼────────────────────────────────────────┘
                   │
        ┌──────────┼──────────┐
        │          │          │
┌───────▼───┐  ┌──▼──────┐  ┌▼──────────┐
│  Services │  │Services │  │ Rule      │
│   Layer   │  │ Config   │  │ Engine    │
│           │  │          │  │           │
│•IAccount- │  │•Service- │  │•Rule<T>  │
│  Service  │  │  Regist- │  │•Evaluate │
│•IRisk-    │  │  ration  │  │  All/Any │
│  Service  │  │•DI Setup │  │           │
│•BaseService   │          │  │           │
└───────────┘  └─────────┘  └───────────┘
```

### Project Structure

```
DataversePluginFramework.sln
├── DataversePluginFramework/              [Domain-Specific Plugin]
│   ├── Plugins/
│   │   └── AccountPlugin.cs              ← Plugin entry point
│   ├── Infrastructure/
│   │   └── ServiceRegistration.cs        ← Centralized DI
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
├── MODEL/                [Shared - Early-Bound Models]
└── PLUGIN_INFRASTRUCTURE/[Shared - Reusable Framework]
    ├── Infrastructure/PluginContainer.cs
    ├── Plugins/PluginBase.cs
    ├── Services/BaseService.cs
    └── Rules/RuleEngine.cs
```

---

## Business Scenario

### Strategic Customer Classification

**Rule:** If Account Revenue > €5,000,000 → Mark as "Strategic Customer"

### Execution Flow

```
Dataverse Event (Account Create/Update)
    ↓
AccountPlugin.Execute()
    ↓
PluginBase<Account>.InitializeServices()
    ↓
PluginContainer Setup (IoC)
    │
    ├── Register IOrganizationService
    ├── Register ITracingService
    ├── Register IAccountService → AccountService
    └── Register IRiskService → RiskService
    ↓
AccountService.Process(Account)
    ├── Evaluate: Revenue > €5,000,000?
    ├── Set: Description = "Strategic Customer"
    └── Delegate: RiskService.Evaluate(account)
```

---

## Core Components

### 1️⃣ PluginBase<T> - Generic Plugin Framework

**Location:** `PLUGIN_INFRASTRUCTURE/Plugins/PluginBase.cs`

A strongly-typed, template method-based plugin base class supporting any entity type.

**Features:**
- 🔄 **Template Method Pattern** for event routing (Pre/Post, Create/Update/Delete)
- 🎯 **Generic Type Safety** via `PluginBase<T> where T : Entity`
- 📦 **Automatic SDK Service Initialization** (ExecutionContext, OrgService, TracingService)
- 🔗 **IoC Container Integration** for service resolution
- 🛡️ **Comprehensive Exception Handling** with tracing

**Key Properties:**
```csharp
protected PluginContainer Container { get; }           // IoC container
protected IPluginExecutionContext Context { get; }     // Execution context
protected ITracingService Tracing { get; }             // Tracing service
protected T CurrentRecord { get; }                      // Strongly-typed entity

protected virtual void OnPreOperationCreate() { }
protected virtual void OnPreOperationUpdate() { }
protected virtual void OnPostOperationCreate() { }
```

---

### 2️⃣ PluginContainer - Lightweight IoC Container

**Location:** `PLUGIN_INFRASTRUCTURE/Infrastructure/PluginContainer.cs`

A simple, efficient dependency injection container without external dependencies.

**Features:**
- ✅ No external dependencies
- ✅ Constructor-based injection with automatic resolution
- ✅ Circular dependency detection
- ✅ Singleton instance caching
- ✅ Fallback to IServiceProvider for SDK services

```csharp
container.Register<IAccountService, AccountService>();
container.RegisterInstance<IOrganizationService>(orgService);
var service = container.Resolve<IAccountService>();
```

---

### 3️⃣ BaseService - Service Foundation

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

### 4️⃣ RuleEngine<T> - Composable Business Rules

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
    account => account.Revenue?.Value > 5000000;
bool result = engine.Evaluate(account, isStrategic);
```

---

### 5️⃣ ServiceRegistration - Centralized DI Configuration

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

### 6️⃣ AccountService & RiskService - Business Logic

**AccountService:** Evaluates account revenue and delegates risk assessment.

```csharp
public class AccountService : BaseService, IAccountService
{
    private readonly IRiskService _riskService;

    public void Process(Account account)
    {
        if (account.Revenue?.Value > 5000000)
        {
            account.Description = "Strategic Customer";
            _riskService.Evaluate(account);
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
        Trace.Trace($"Evaluating risk for account: {account.Name}");
    }
}
```

---

## Getting Started

### Prerequisites
- ✅ Visual Studio 2019+
- ✅ .NET Framework 4.6.2
- ✅ Microsoft Dataverse SDK (CRM SDK) 9.0.2+

### Quick Start

#### 1. Clone/Extract the Project
```bash
git clone https://github.com/yourusername/DataversePluginFramework.git
cd DataversePluginFramework
```

#### 2. Build the Plugin Assembly
```bash
dotnet build DataversePluginFramework.sln -c Release
```

The compiled plugin will be: `bin/Release/DataversePluginFramework.dll`

#### 3. Deploy to Dataverse

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
    public void Execute(MyEntity entity)
    {
        Trace.Trace($"Processing: {entity.Name}");
    }
}
```

**Step 2: Register Service**
```csharp
container.Register<IMyService, MyService>();
```

**Step 3: Create Plugin Class**
```csharp
public class MyPlugin : PluginBase<MyEntity>
{
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
The `PluginBase<T>` class enforces a standard execution flow while allowing derived classes to customize specific steps.

### 2. Dependency Injection
Services registered in `ServiceRegistration.cs` are resolved through the IoC container, promoting loose coupling and testability.

### 3. Strategy Pattern (via RuleEngine)
Business rules as delegate strategies enable flexible rule composition without inheritance hierarchies.

### 4. Singleton Pattern
The IoC container manages singleton instances of services, ensuring consistent state across execution.

---

## Best Practices

### ✅ DO

**Clear, Intent-Revealing Service Names:**
```csharp
public interface IAccountValidationService { }
public interface IRevenueCalculationService { }
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

**Comprehensive Tracing:**
```csharp
Trace.Trace($"Processing account: {account.Name}");
if (account.Revenue?.Value > 5000000)
{
    Trace.Trace("  → High revenue account marked as strategic");
}
```

### ❌ DON'T

**Vague Names:**
```csharp
public interface IService1 { }
public interface IHelper { }
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
// No logging at all - impossible to debug in production!
account.Description = "Strategic Customer";
```

---

## Testing

The framework's design supports comprehensive unit testing:

```csharp
[TestClass]
public class AccountServiceTests
{
    [TestMethod]
    public void Process_WithHighRevenue_MarksAsStrategic()
    {
        // Arrange
        var mockOrgService = new Mock<IOrganizationService>();
        var mockTracingService = new Mock<ITracingService>();
        var mockRiskService = new Mock<IRiskService>();
        
        var account = new Account { Revenue = new Money(6000000) };
        var service = new AccountService(
            mockOrgService.Object,
            mockTracingService.Object,
            mockRiskService.Object);
        
        // Act
        service.Process(account);
        
        // Assert
        Assert.AreEqual("Strategic Customer", account.Description);
        mockRiskService.Verify(r => r.Evaluate(account), Times.Once);
    }
    
    [TestMethod]
    public void Process_WithLowRevenue_DoesNotMark()
    {
        // Arrange
        var mockOrgService = new Mock<IOrganizationService>();
        var mockTracingService = new Mock<ITracingService>();
        var mockRiskService = new Mock<IRiskService>();
        
        var account = new Account { Revenue = new Money(3000000) };
        var service = new AccountService(
            mockOrgService.Object,
            mockTracingService.Object,
            mockRiskService.Object);
        
        // Act
        service.Process(account);
        
        // Assert - Should NOT be marked as strategic
        mockRiskService.Verify(
            r => r.Evaluate(It.IsAny<Account>()), 
            Times.Never);
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

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.CrmSdk.CoreAssemblies | 9.0.2.51 | Dataverse SDK |
| Microsoft.CrmSdk.Workflow | 9.0.2.42 | Workflow integration |
| Microsoft.CrmSdk.XrmTooling.CoreAssembly | 9.1.1.65 | XrmTooling services |

---

## Learning Resources

- [Microsoft Dataverse Documentation](https://docs.microsoft.com/en-us/power-apps/developer/data-platform/)
- [Dynamics 365 Plugin Development Best Practices](https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/plug-ins)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Design Patterns in C#](https://refactoring.guru/design-patterns)

---

**⭐ If you find this framework helpful, consider starring the repository!**

*Last Updated: March 2026 | Framework Version: 1.0*
