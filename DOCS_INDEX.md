# 📚 Documentation Index

Welcome to the Microsoft Foundry Agent Deployment project documentation!

## 🚀 Getting Started

**New to this project?** Start here:

1. **[QUICKSTART.md](QUICKSTART.md)** ⚡
   - Quick commands to get started
   - SDK selection guide
   - Common usage patterns

## 📖 Main Documentation

### For Users

- **[README.md](README.md)** 📘
  - Project overview
  - Background and features
  - YAML configuration examples
  - Basic usage instructions

### For Developers

- **[UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md)** 🏗️
  - Complete architecture details
  - Project structure explanation
  - Build and publish instructions
  - SDK comparison
  - Development guidelines

- **[ARCHITECTURE.md](ARCHITECTURE.md)** 📊
  - Visual diagrams
  - Component breakdown
  - Dependency graphs
  - Data flow illustrations

## 📁 Project Structure Quick Reference

```
src/
├── DeployAgent.Core/       ← Shared models & services
├── DeployAgent.V1/         ← V1 SDK implementation
├── DeployAgent.V2/         ← V2 SDK implementation
├── DeployAgent.CLI/        ← Unified CLI (use this!)
├── DeployAgent/            ← Legacy V1 (still works)
└── DeployAgentV2/          ← Legacy V2 (still works)
```

## 🎯 Which Document Should I Read?

### "I just want to deploy agents quickly"
→ **[QUICKSTART.md](QUICKSTART.md)**

### "I'm currently using the old DeployAgent or DeployAgentV2"
→ **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)**

### "I want to understand the architecture"
→ **[ARCHITECTURE.md](ARCHITECTURE.md)**

### "I need complete documentation for development"
→ **[UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md)**

### "I want to know what changed and why"
→ **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)**

### "I need YAML configuration examples"
→ **[README.md](README.md)** (Configuration section)

## 🔍 Find Information By Topic

### Building
- **Quick build**: [QUICKSTART.md](QUICKSTART.md#-build-commands)
- **Detailed build**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-building-the-project)
- **CI/CD pipelines**: [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md#scenario-1-cicd-pipeline)

### Publishing
- **Quick publish**: [QUICKSTART.md](QUICKSTART.md#-publish-commands)
- **Detailed publish**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-publishing-the-cli)
- **Platform-specific**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-publishing-the-cli)

### SDK Versions
- **V1 vs V2 comparison**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-v1-vs-v2-sdk-differences)
- **SDK selection**: [QUICKSTART.md](QUICKSTART.md#-which-sdk-version)
- **Version switching**: [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md#step-2-test-with-your-existing-yaml-files)

### Architecture
- **Visual diagrams**: [ARCHITECTURE.md](ARCHITECTURE.md)
- **Component details**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-proposed-unified-structure)
- **Dependencies**: [ARCHITECTURE.md](ARCHITECTURE.md#dependency-graph)

### Development
- **Adding new features**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#extending-shared-functionality)
- **Adding new SDK versions**: [ARCHITECTURE.md](ARCHITECTURE.md#adding-v3-sdk-support)
- **Best practices**: [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md#-best-practices)

### Troubleshooting
- **Common issues**: [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md#-troubleshooting)
- **Build problems**: Check build output in terminal
- **Runtime errors**: Review YAML validation in README

## 📊 Documentation Statistics

- **Total Documents**: 6 comprehensive guides
- **Total Lines**: ~2,000+ lines of documentation
- **Code Examples**: 50+ working examples
- **Diagrams**: Multiple ASCII art visualizations
- **Last Updated**: January 23, 2026

## 🎓 Learning Path

### Beginner Path
1. Read [QUICKSTART.md](QUICKSTART.md)
2. Try example commands
3. Read [README.md](README.md) for YAML examples
4. Run your first deployment

### Intermediate Path
1. Complete Beginner Path
2. Read [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)
3. Migrate existing deployments
4. Experiment with both SDK versions

### Advanced Path
1. Complete Intermediate Path
2. Study [ARCHITECTURE.md](ARCHITECTURE.md)
3. Review [UNIFIED_STRUCTURE.md](UNIFIED_STRUCTURE.md)
4. Read [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)
5. Contribute to development

## 🔗 External Resources

### Azure Documentation
- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-services/)
- [Azure AI Agents](https://learn.microsoft.com/azure/ai-services/agents/)

### SDK References
- [Azure.AI.Agents.Persistent](https://www.nuget.org/packages/Azure.AI.Agents.Persistent/)
- [Azure.AI.Projects](https://www.nuget.org/packages/Azure.AI.Projects/)

### .NET Resources
- [.NET 10.0 Documentation](https://learn.microsoft.com/dotnet/)
- [C# Documentation](https://learn.microsoft.com/dotnet/csharp/)

## 📝 Quick Command Reference

```powershell
# Build
dotnet build src/DeployAgent.CLI/DeployAgent.CLI.csproj

# Run with V1
dotnet run --project src/DeployAgent.CLI agents.yaml -p <endpoint> -v v1

# Run with V2
dotnet run --project src/DeployAgent.CLI agents.yaml -p <endpoint> -v v2

# Publish
dotnet publish src/DeployAgent.CLI/DeployAgent.CLI.csproj -c Release -r win-x64
```

## 💡 Tips for Documentation Navigation

1. **Use your editor's search**: Search across all .md files for specific terms
2. **Follow the links**: Documents cross-reference each other extensively
3. **Check the index above**: Find information by topic
4. **Start simple**: Begin with QUICKSTART, progress to detailed docs
5. **Use examples**: All documents include working code examples

## 🤝 Contributing to Documentation

If you find:
- **Errors or typos**: Please report them
- **Missing information**: Suggest additions
- **Unclear explanations**: Request clarification
- **Better examples**: Share them

## 📅 Version History

- **v2.0** (2026-01-23): Unified structure implementation
- **v1.0**: Original dual-project structure

---

**Ready to start?** Head over to [QUICKSTART.md](QUICKSTART.md)! 🚀
