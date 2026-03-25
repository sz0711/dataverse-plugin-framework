# 🧩 Dataverse Plugin Framework

> Production-ready, reusable plugin development framework for Microsoft Dataverse / Dynamics 365.

[![.NET Framework 4.6.2](https://img.shields.io/badge/.NET%20Framework-4.6.2-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](#license)
[![Status](https://img.shields.io/badge/status-stable-blue)]()

---

## Overview

Reusable framework demonstrating enterprise-grade plugin architecture for Microsoft Dataverse and Dynamics 365. Applies SOLID principles, clean code practices, and dependency injection without any external DI container dependencies.

### Key Capabilities

- **Generic Plugin Base Class** — Template Method pattern with event routing (Pre/Post × Create/Update/Delete), correlation tracing, and automatic SDK service initialization
- **Lightweight IoC Container** — Greedy constructor injection, instance caching, constructor caching, and descriptive error messages; no external dependencies
- **Delegate-Based Rule Engine** — `Evaluate` / `EvaluateAll` / `EvaluateAny` for composable business logic
- **Centralized Constants** — Single source of truth for magic values (thresholds, labels) via `PluginConstants`
- **Reusable Shared Projects** — `PLUGIN_INFRASTRUCTURE` (framework) and `MODEL` (early-bound entities) as `.shproj` shared projects
- **14 Unit Tests** — xUnit + Moq covering services and infrastructure

### Business Scenario

**Strategic Customer Classification:** If `Account.Revenue > €5,000,000` → set `Description = "Strategic Customer"` and trigger risk assessment. Both the threshold and label are defined in `PluginConstants`, making changes a one-line edit.

---

## Architecture

```
DataversePluginFramework.sln
├── DataversePluginFramework/       # Domain plugin assembly
│   ├── Plugins/AccountPlugin.cs   # Plugin entry point
│   ├── Infrastructure/            # ServiceRegistration (DI config)
│   └── Services/                  # AccountService, RiskService
│
├── DataversePluginFramework.Tests/ # Unit test project (14 tests)
│   ├── Services/                  # AccountServiceTests, RiskServiceTests
│   └── Infrastructure/            # PluginContainerTests
│
├── MODEL/                          # Shared — early-bound entity models
│   └── Model/Entities/            # account.cs, contact.cs, OrgContext.cs
│
└── PLUGIN_INFRASTRUCTURE/          # Shared — reusable framework
    ├── Constants/PluginConstants.cs
    ├── Infrastructure/PluginContainer.cs
    ├── Plugins/PluginBase.cs
    ├── Rules/RuleEngine.cs
    └── Services/BaseService.cs
```

**5 framework components** · **14 tests** · **0 external DI dependencies**

### Execution Pipeline

```
Dataverse Event → PluginBase<T> → IoC Container → Domain Services → Rule Engine
```

---

## Getting Started

### Prerequisites

- Visual Studio 2019+ (2022 recommended)
- .NET Framework 4.6.2
- `nuget.exe` CLI and MSBuild (this is a `packages.config` solution — use `nuget` + `msbuild`, not `dotnet`)
- Microsoft Dataverse SDK 9.0.2+

### Build & Test

```bash
nuget restore DataversePluginFramework.sln
msbuild DataversePluginFramework.sln -verbosity:minimal

vstest.console.exe DataversePluginFramework.Tests\bin\Debug\DataversePluginFramework.Tests.dll ^
  /TestAdapterPath:packages\xunit.runner.visualstudio.2.8.2\build\net462
```

Expected: **14 tests passed** (6 AccountService + 4 RiskService + 4 PluginContainer).

---

## Deployment

Deploy the compiled assembly to Dataverse using the **Plugin Registration Tool**:

| Setting | Value |
|---------|-------|
| Assembly | `DataversePluginFramework\bin\Debug\DataversePluginFramework.dll` |
| Plugin class | `DataversePluginFramework.Plugins.AccountPlugin` |
| Message | `Create` / `Update` |
| Primary Entity | `account` |
| Stage | `Pre-Operation` |

---

## Creating a New Plugin

**Step 1 — Define a service interface and implementation:**

```csharp
public interface IMyService { void Execute(MyEntity entity); }

public class MyService : BaseService, IMyService
{
    public MyService(IOrganizationService svc, ITracingService trace) : base(svc, trace) { }
    public void Execute(MyEntity entity) => Trace.Trace($"Processing: {entity.Name}");
}
```

**Step 2 — Register in `ServiceRegistration.cs`:**

```csharp
container.Register<IMyService, MyService>();
```

**Step 3 — Create the plugin class:**

```csharp
public class MyPlugin : PluginBase<MyEntity>
{
    protected override void RegisterDomainServices(PluginContainer container)
        => ServiceRegistration.RegisterServices(container);

    protected override void OnPreOperationCreate()
        => Container.Resolve<IMyService>().Execute(CurrentRecord);
}
```

---

## Testing

```bash
vstest.console.exe DataversePluginFramework.Tests\bin\Debug\DataversePluginFramework.Tests.dll ^
  /TestAdapterPath:packages\xunit.runner.visualstudio.2.8.2\build\net462
```

| Test Class | Tests | Covers |
|------------|-------|--------|
| `AccountServiceTests` | 6 | Null guards, revenue thresholds, strategic marking, risk isolation |
| `RiskServiceTests` | 4 | Null guards, revenue-based tracing |
| `PluginContainerTests` | 4 | Instance registration, constructor DI, fallback, null resolution |

---

## Technology Stack

| Component | Version |
|-----------|---------|
| .NET Framework | 4.6.2 |
| Microsoft.CrmSdk.CoreAssemblies | 9.0.2.51 |
| Microsoft.CrmSdk.Workflow | 9.0.2.42 |
| Microsoft.CrmSdk.XrmTooling.CoreAssembly | 9.1.1.65 |
| xUnit | 2.9.3 |
| Moq | 4.20.72 |

---

## License

MIT

## Disclaimer

Private project. Provided without warranty. Use at your own risk.

---

> Built with ❤️ for Dynamics 365 developers and architects.
