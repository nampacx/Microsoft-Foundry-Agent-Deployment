# 📊 Architecture Comparison

## Before: Duplicated Structure

```
┌─────────────────────────────────────────────────────────────┐
│                     OLD STRUCTURE                            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────────┐       ┌─────────────────────┐     │
│  │   DeployAgent       │       │  DeployAgentV2      │     │
│  │                     │       │                     │     │
│  │  • Models/          │  ❌   │  • Models/          │     │
│  │  • Services/        │  ❌   │  • Services/        │     │
│  │  • Program.cs       │  ❌   │  • Program.cs       │     │
│  │                     │       │                     │     │
│  │  Uses: V1 SDK       │       │  Uses: V2 SDK       │     │
│  └─────────────────────┘       └─────────────────────┘     │
│                                                              │
│  Problems:                                                   │
│  • 80% code duplication                                     │
│  • Changes needed in 2 places                               │
│  • Different executables for each version                   │
│  • Hard to maintain consistency                             │
└─────────────────────────────────────────────────────────────┘
```

## After: Unified Structure

```
┌─────────────────────────────────────────────────────────────┐
│                    NEW STRUCTURE                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│              ┌─────────────────────────┐                    │
│              │   DeployAgent.CLI       │                    │
│              │   (Single Entry Point)  │                    │
│              │                         │                    │
│              │  --sdk-version v1|v2    │                    │
│              └───────────┬─────────────┘                    │
│                          │                                   │
│           ┌──────────────┼──────────────┐                   │
│           │              │              │                   │
│           ▼              ▼              ▼                   │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐           │
│  │ V1 Service │  │ V2 Service │  │    Core    │           │
│  │            │  │            │  │            │           │
│  │ Persistent │  │ AI Project │  │  • Models  │           │
│  │   Agents   │  │   Client   │  │  • Services│           │
│  │            │  │            │  │  • Abstract│           │
│  └─────┬──────┘  └─────┬──────┘  └──────┬─────┘           │
│        │               │                │                   │
│        └───────────────┴────────────────┘                   │
│                        │                                     │
│          All implement IAgentDeploymentService              │
│                                                              │
│  Benefits:                                                   │
│  ✅ 0% code duplication                                     │
│  ✅ Changes made once in Core                               │
│  ✅ Single executable, runtime selection                    │
│  ✅ Easy to add V3, V4, etc.                                │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow Comparison

### Old Flow (Per SDK)
```
User → DeployAgent.exe → V1 SDK → Azure
User → DeployAgentV2.exe → V2 SDK → Azure
```

### New Flow (Unified)
```
User → DeployAgent.exe
       └─> --sdk-version v1 → V1 Service → V1 SDK → Azure
       └─> --sdk-version v2 → V2 Service → V2 SDK → Azure
```

## Component Breakdown

### Core Library (Shared)
```
DeployAgent.Core
├── Models/
│   ├── AgentDefinition      ← YAML agent config
│   ├── ToolDefinition        ← YAML tool config
│   └── DefinitionBase        ← Base class
├── Services/
│   ├── AgentDefinitionService    ← YAML parsing & validation
│   ├── ConfigurationService      ← CLI args & config
│   └── OpenApiService            ← Download OpenAPI specs
└── Abstractions/
    └── IAgentDeploymentService   ← Common interface
```

### V1 Implementation
```
DeployAgent.V1
└── Services/
    └── PersistentAgentDeploymentService
        ├── Implements: IAgentDeploymentService
        ├── Uses: PersistentAgentsClient
        └── Features:
            • OpenAPI tools ✅
            • Connected agents ✅
            • Agent updates ✅
```

### V2 Implementation
```
DeployAgent.V2
└── Services/
    └── AIProjectAgentDeploymentService
        ├── Implements: IAgentDeploymentService
        ├── Uses: AIProjectClient
        └── Features:
            • OpenAPI tools ✅
            • Connected agents ❌ (not yet)
            • Agent versioning ✅
```

### CLI Application
```
DeployAgent.CLI
└── Program.cs
    ├── Reads: ConfigurationService
    ├── Selects: SDK version (v1 or v2)
    ├── Creates: Appropriate service instance
    └── Executes: CreateAllAgentsAsync()
```

## SDK Version Selection Logic

```csharp
// Runtime selection based on --sdk-version parameter
var sdkVersion = args.Contains("--sdk-version") 
    ? args[Array.IndexOf(args, "--sdk-version") + 1] 
    : "v1"; // default

IAgentDeploymentService service = sdkVersion switch
{
    "v1" => new PersistentAgentDeploymentService(...),
    "v2" => new AIProjectAgentDeploymentService(...),
    _ => throw new NotSupportedException($"SDK version {sdkVersion} not supported")
};

await service.CreateAllAgentsAsync();
```

## Dependency Graph

```
                     ┌─────────────────┐
                     │ DeployAgent.CLI │
                     └────────┬────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
      ┌──────────┐    ┌──────────┐     ┌──────────┐
      │ Core     |    | V1       │     │ V2       │
      └──────────┘    └──────────┘     └────┬─────┘
                              │             │
                              └─────────────┘
                                     │
                              ┌──────▼──────┐
                              │   Core      │
                              └─────────────┘
```

## Package Dependencies

```
Core
├── YamlDotNet 16.2.1
├── Microsoft.Extensions.Configuration 10.0.0
└── Microsoft.Extensions.Configuration.Json 10.0.0

V1 → Core
├── Azure.AI.Agents.Persistent 1.1.0
└── Azure.Identity 1.17.1

V2 → Core
├── Azure.AI.Projects 1.2.0-beta.4
├── Azure.AI.Projects.OpenAI 1.0.0-beta.4
├── Azure.AI.Agents.Persistent 1.1.0
└── Azure.Identity 1.17.1

CLI → Core, V1, V2
└── (All dependencies transitive)
```

## Build Targets

```
dotnet build Microsoft-Foundry-Agent-Deployment.sln

Builds in order:
1. DeployAgent.Core          (no dependencies)
2. DeployAgent.V1            (depends on Core)
3. DeployAgent.V2            (depends on Core)
4. DeployAgent.CLI           (depends on Core, V1, V2)
5. DeployAgent (legacy)      (independent)
6. DeployAgentV2 (legacy)    (independent)
```

## Future Extensibility

### Adding V3 SDK Support

```
1. Create new project:
   DeployAgent.V3/
   └── Services/
       └── V3AgentDeploymentService.cs
           implements IAgentDeploymentService

2. Add reference in CLI:
   <ProjectReference Include="..\DeployAgent.V3\..." />

3. Update CLI Program.cs:
   case "v3":
       service = new V3AgentDeploymentService(...);
       break;

4. Done! No changes to Core needed.
```

This demonstrates the power of the new architecture - adding new SDK versions requires minimal changes and no modification to shared code.
