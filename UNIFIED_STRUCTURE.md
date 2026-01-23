# Unified Agent Deployment Structure

This document explains the new unified structure that supports both V1 (Persistent Agents) and V2 (AI Projects) SDK versions.

## 📁 Project Structure

```
src/
├── DeployAgent.Core/              # Shared core library
│   ├── Models/                    # Shared data models
│   │   ├── AgentDefinition.cs
│   │   ├── ToolDefinition.cs
│   │   └── DefinitionBase.cs
│   ├── Services/                  # Shared business logic
│   │   ├── AgentDefinitionService.cs
│   │   ├── ConfigurationService.cs
│   │   └── OpenApiService.cs
│   └── Abstractions/              # Interfaces
│       └── IAgentDeploymentService.cs
│
├── DeployAgent.V1/                # V1 SDK implementation
│   └── Services/
│       └── PersistentAgentDeploymentService.cs
│
├── DeployAgent.V2/                # V2 SDK implementation
│   └── Services/
│       └── AIProjectAgentDeploymentService.cs
│
└── DeployAgent.CLI/               # Unified command-line interface
    ├── Program.cs
    └── appsettings.json
```

## 🔄 Migration from Old Structure

The old structure had two separate projects (`DeployAgent` and `DeployAgentV2`) with duplicated code. The new structure:

- **Eliminates duplication**: All shared code is in `DeployAgent.Core`
- **Maintains compatibility**: Both SDK versions are still supported
- **Single entry point**: One CLI that can use either SDK version
- **Easier maintenance**: Changes to shared logic only need to be made once

## 🚀 Building the Project

### Build All Projects

```powershell
dotnet build Microsoft-Foundry-Agent-Deployment.sln
```

### Build Individual Projects

```powershell
# Build only the CLI (automatically builds dependencies)
dotnet build src/DeployAgent.CLI/DeployAgent.CLI.csproj

# Build only V1 components
dotnet build src/DeployAgent.V1/DeployAgent.V1.csproj

# Build only V2 components
dotnet build src/DeployAgent.V2/DeployAgent.V2.csproj
```

## 🎯 Running the CLI

### Using V1 SDK (Persistent Agents)

```powershell
dotnet run --project src/DeployAgent.CLI `
  agents.yaml `
  --project-endpoint "https://example.services.ai.azure.com/api/projects/myproject" `
  --sdk-version v1 `
  --tenant-id "your-tenant-id"
```

### Using V2 SDK (AI Projects)

```powershell
dotnet run --project src/DeployAgent.CLI `
  agents.yaml `
  --project-endpoint "https://example.services.ai.azure.com/api/projects/myproject" `
  --sdk-version v2 `
  --tenant-id "your-tenant-id"
```

### Default Behavior

If `--sdk-version` is not specified, **V1 is used by default**.

## 📝 Command-Line Arguments

| Argument | Short | Required | Description | Default |
|----------|-------|----------|-------------|---------|
| `<yaml-file>` | - | ✅ Yes | Path to YAML configuration file | - |
| `--project-endpoint` | `-p` | ✅ Yes | Azure AI Foundry project endpoint | - |
| `--sdk-version` | `-v` | ❌ No | SDK version to use (`v1` or `v2`) | `v1` |
| `--tenant-id` | `-t` | ❌ No | Azure tenant ID | - |

### Example

```powershell
DeployAgent sample/weather-agent.yaml -p https://myproject.openai.azure.com -v v2
```

## 🆚 V1 vs V2 SDK Differences

### V1 (Persistent Agents)
- **Package**: `Azure.AI.Agents.Persistent` (1.1.0)
- **Client**: `PersistentAgentsClient`
- **Features**: 
  - ✅ OpenAPI tools
  - ✅ Connected agents (multi-agent orchestration)
  - ✅ Agent updates

### V2 (AI Projects)
- **Packages**: 
  - `Azure.AI.Projects` (1.2.0-beta.4)
  - `Azure.AI.Projects.OpenAI` (1.0.0-beta.4)
- **Client**: `AIProjectClient`
- **Features**:
  - ✅ OpenAPI tools
  - ❌ Connected agents (not yet supported)
  - ✅ Agent versioning

## 🛠️ Development

### Adding a New SDK Version

To add support for a new SDK version (e.g., V3):

1. Create a new project: `src/DeployAgent.V3/DeployAgent.V3.csproj`
2. Reference `DeployAgent.Core`
3. Implement `IAgentDeploymentService`
4. Update CLI's `Program.cs` to handle `v3` option

### Extending Shared Functionality

To add new features used by both SDKs:

1. Add models to `DeployAgent.Core/Models/`
2. Add services to `DeployAgent.Core/Services/`
3. Update the interface in `DeployAgent.Core/Abstractions/` if needed
4. Implement in both V1 and V2 services

## 📦 NuGet Package Dependencies

### Core
- Microsoft.Extensions.Configuration (10.0.0)
- Microsoft.Extensions.Configuration.Json (10.0.0)
- YamlDotNet (16.2.1)

### V1
- Azure.AI.Agents.Persistent (1.1.0)
- Azure.Identity (1.17.1)

### V2
- Azure.AI.Projects (1.2.0-beta.4)
- Azure.AI.Agents.Persistent (1.1.0)
- Azure.AI.Projects.OpenAI (1.0.0-beta.4)
- Azure.Identity (1.17.1)

## 🎓 Best Practices

1. **Default to V1**: Unless you need V2-specific features, use V1 as it's more stable
2. **Specify SDK version explicitly**: Always use `--sdk-version` to be explicit about which SDK you're using
3. **Keep YAML files compatible**: Both SDKs use the same YAML format for agent definitions
4. **Test both versions**: If you're migrating, test your agents with both SDK versions