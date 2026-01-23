# 🚀 Quick Start Guide - Unified Agent Deployment

## ⚡ Quick Commands

### V1 (Persistent Agents) - Recommended
```powershell
dotnet run --project src/DeployAgent.CLI `
  sample/weather-agent.yaml `
  -p https://your-project.openai.azure.com `
  -v v1
```

### V2 (AI Projects) - Beta
```powershell
dotnet run --project src/DeployAgent.CLI `
  sample/weather-agent.yaml `
  -p https://your-project.openai.azure.com `
  -v v2
```

## 📋 Command Arguments

| Argument | Short | Required | Description |
|----------|-------|----------|-------------|
| `<yaml-file>` | - | ✅ | Path to your agent definition YAML |
| `--project-endpoint` | `-p` | ✅ | Your Azure AI Foundry project endpoint |
| `--sdk-version` | `-v` | ❌ | `v1` or `v2` (default: `v1`) |
| `--tenant-id` | `-t` | ❌ | Your Azure tenant ID (optional) |

## 🏗️ Project Structure

```
New Unified Structure (Recommended):
├── DeployAgent.Core      ← Shared models & services
├── DeployAgent.V1        ← V1 SDK implementation
├── DeployAgent.V2        ← V2 SDK implementation
└── DeployAgent.CLI       ← Unified CLI (use this!)

Legacy Structure (Still works):
├── DeployAgent           ← V1 standalone
└── DeployAgentV2         ← V2 standalone
```

## 🎯 Which SDK Version?

### Use V1 if:
- ✅ You need multi-agent orchestration (connected agents)
- ✅ You want a stable, production-ready SDK
- ✅ You need to update existing agents

### Use V2 if:
- ✅ You need the latest Azure AI Projects features
- ✅ You're okay with beta features
- ⚠️ Note: Connected agents not yet supported in V2

## 🔨 Build Commands

```powershell
# Build everything
dotnet build Microsoft-Foundry-Agent-Deployment.sln

# Build just the CLI
dotnet build src/DeployAgent.CLI/DeployAgent.CLI.csproj

# Build and run
dotnet run --project src/DeployAgent.CLI your-agents.yaml -p <endpoint> -v v1
```

## 💡 Tips

1. **Default is V1**: If you don't specify `--sdk-version`, V1 is used
2. **Same YAML format**: Both versions use identical YAML definitions
3. **Legacy projects**: Old `DeployAgent` and `DeployAgentV2` still work
4. **Tenant ID**: Usually optional, only needed for specific auth scenarios

## 📖 More Information

- Full documentation: See [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md)
- Original README: See [README.md](README.md)
