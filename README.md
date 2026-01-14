# Microsoft Foundry Agent Deployment

A .NET console application for deploying and managing Microsoft Foundry Agents using YAML configuration files. This tool automates the deployment of AI agents with OpenAPI-based tools to Azure AI Foundry projects.

## Overview

DeployAgent is a deployment automation tool that:
- Reads agent and tool definitions from YAML configuration files
- Creates agents in Azure AI Foundry projects
- Configures agents with OpenAPI-based tools
- Supports bulk deployment of multiple agents

## Features

- **YAML-based Configuration**: Define agents and tools in a simple, declarative format
- **OpenAPI Tool Integration**: Automatically fetches and configures OpenAPI specifications for agent tools
- **Azure Identity Integration**: Uses DefaultAzureCredential for secure authentication
- **Batch Deployment**: Deploy multiple agents from a single configuration file
- **Error Handling**: Comprehensive error messages and validation

## Prerequisites

- .NET 10.0 SDK or later
- Azure subscription with access to Azure AI Foundry
- Azure credentials configured (via Azure CLI, environment variables, or managed identity)
- An Azure AI Foundry project

## Configuration

### agents.yaml

Define your agents and tools in a YAML file:

```yaml
agents:
  - type: agent
    name: WeatherAgent
    model: gpt-4o-mini-deployment
    instructions: |
      You are a specialized agent for retrieving weather information...
    tools:
      - WeatherServiceAPI

tools:
  - type: tool
    kind: OpenAPI
    name: WeatherServiceAPI
    spec_url: https://api.example.com/openapi/v3.json
    description: Retrieves weather information for a given location.
```

## Usage

### Basic Usage

```bash
dotnet run --project src/DeployAgent/DeployAgent.csproj -- sample/agents.yaml --project-endpoint <your-endpoint>
```

### Command Line Arguments

- First positional argument: Path to the YAML configuration file (required)
- `--project-endpoint` or `-p`: Azure AI Foundry project endpoint URL (required)
- `--tenant-id` or `-t`: Azure tenant ID (optional)

### Examples

```bash
# Deploy agents with required parameters
dotnet run -- sample/agents.yaml -p https://your-project.services.ai.azure.com/api/projects/your-project

dotnet run -- ../../sample/agents.yaml -p https://nampacx-fndry-2.services.ai.azure.com/api/projects/nampacx-fndry-prjct

# Include tenant ID if needed
dotnet run -- sample/agents.yaml -p https://your-project.services.ai.azure.com/api/projects/your-project -t your-tenant-id

# Using long-form arguments
dotnet run --project src/DeployAgent/DeployAgent.csproj -- path/to/agents.yaml --project-endpoint https://your-project.services.ai.azure.com/api/projects/your-project --tenant-id your-tenant-id
```

## Project Structure

```
│   └── DeployAgent/
│       ├── Program.cs                  # Application entry point
│       ├── DeployAgent.csproj          # Project file
│       ├── appsettings.json            # Configuration settings
│       ├── Models/
│       │   ├── AgentDefinition.cs      # Agent model
│       │   ├── ToolDefinition.cs       # Tool model
│       │   └── DefinitionBase.cs       # Base definition class
│       └── Services/
│           ├── AgentDefinitionService.cs      # YAML parsing service
│           ├── AgentDeploymentService.cs      # Agent deployment service
│           ├── ConfigurationService.cs        # Configuration management
│           └── OpenApiService.cs              # OpenAPI spec fetching
└── sample/
    └── agents.yaml                     # Sample configuration
```

## Dependencies

- **Azure.AI.Agents.Persistent** (v1.1.0): Azure AI Agents SDK
- **Azure.Identity** (v1.17.1): Azure authentication
- **Microsoft.Extensions.Configuration** (v10.0.0): Configuration management
- **YamlDotNet** (v16.2.1): YAML parsing

## How It Works

1. **Configuration Loading**: Reads `appsettings.json` and command-line arguments
2. **YAML Parsing**: Parses the YAMLcommand-line arguments for project endpoint and YAML file path
3. **OpenAPI Fetching**: Downloads OpenAPI specifications for each tool
4. **Agent Creation**: Creates agents in Azure AI Foundry with configured tools
5. **Deployment**: Deploys all agents and reports success/failure

## Authentication

The application uses `DefaultAzureCredential` which supports multiple authentication methods in the following order:
1. Environment variables
2. Managed Identity
3. Visual Studio
4. Azure CLI
5. Azure PowerShell
6. Interactive browser

Ensure you're authenticated via one of these methods before running the application.

## Error Handling

The application provides detailed error messages for common issues:
- Missing or invalid YAML file
- Invalid configuration settings
- Authentication failures
- Agent creation errors
- OpenAPI specification fetch failures

## Example Output

```
=== Agent Deployment System ===

Using YAML file: sample/agents.yaml

Initializing Agent Orchestration Service...

=== Starting Agent Creation Process ===

Creating agent: WeatherAgent
✓ Successfully created agent: WeatherAgent

=== Successfully deployed 1 agent(s) ===
```

## License

See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
