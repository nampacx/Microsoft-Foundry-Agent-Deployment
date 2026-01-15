# 🤖 Microsoft Foundry Agent Deployment

A .NET console application for deploying and managing Microsoft Foundry Agents using YAML configuration files. This tool automates the deployment of AI agents with OpenAPI-based tools to Azure AI Foundry projects.

## 📖 Background

This project was created to simplify the creation of **persistent agents** in Microsoft AI Foundry (classic portal). Since Azure AI Foundry does not currently provide an Infrastructure as Code (IaC) way to create agents, and the new Foundry portal displays agents as YAML code, this tool brings similar YAML-based configuration capabilities to the classic portal.

**⚠️ Important Disclaimers:**
- This tool is designed for **classic agents** in Azure AI Foundry
- Currently implements **OpenAPI Tools** and **Connected Agents** - other tool types can be added in the future
- The YAML format used by this tool differs from the new portal's YAML format, so migrating to the new portal would require code modifications

## 🚀 Overview

DeployAgent is a deployment automation tool that:
- Reads agent and tool definitions from YAML configuration files
- Creates agents in Azure AI Foundry projects
- Configures agents with OpenAPI-based tools
- Supports connected agents for multi-agent orchestration
- Supports bulk deployment of multiple agents

## ✨ Features

- **📝 YAML-based Configuration**: Define agents and tools in a simple, declarative format
- **🔌 OpenAPI Tool Integration**: Automatically fetches and configures OpenAPI specifications for agent tools
- **🔗 Connected Agents**: Support for multi-agent orchestration where agents can use other agents as tools
- **🔐 Azure Identity Integration**: Uses DefaultAzureCredential for secure authentication
- **📦 Batch Deployment**: Deploy multiple agents from a single configuration file
- **🛡️ Error Handling**: Comprehensive error messages and validation

## 📋 Prerequisites

- .NET 10.0 SDK or later
- Azure subscription with access to Azure AI Foundry
- Azure credentials configured (via Azure CLI, environment variables, or managed identity)
- An Azure AI Foundry project

## ⚙️ Configuration

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

#### Connected Agents (Multi-Agent Orchestration)

You can create connected agent systems where agents use other agents as tools. This enables complex workflows with specialized agents:

```yaml
agents:
  - type: agent
    name: EmployeeInfoAgent
    model: gpt-4o-mini-deployment
    instructions: |
      You retrieve and provide employee CV information...
    tools:
      - EmployeeAPI

  - type: agent
    name: OrchestratorAgent
    model: gpt-4o-mini-deployment
    instructions: |
      You coordinate multiple specialized agents to fulfill user requests...
    tools:
      - EmployeeInfoAgent  # Reference to another agent
      - ConverterAgent

tools:
  - type: tool
    kind: OpenAPI
    name: EmployeeAPI
    spec_url: https://api.example.com/employees/openapi.json
    description: Retrieves employee information.

  # Define agents as tools for other agents
  - type: tool
    kind: agent
    name: EmployeeInfoAgent
    description: Retrieves and formats employee CV information.

  - type: tool
    kind: agent
    name: ConverterAgent
    description: Converts content to different formats.
```

See [sample/multi-agents.yaml](sample/multi-agents.yaml) for a complete connected agent example.

## 💻 Usage

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

## ⚡ How It Works

1. **📥 Configuration Loading**: Reads command-line arguments
2. **📄 YAML Parsing**: Parses the YAML file with de agent and tool definitions
3. **🌐 OpenAPI Fetching**: Downloads OpenAPI specifications for each tool
4. **🏗️ Agent Creation**: Creates agents in Azure AI Foundry with configured tools
5. **🚀 Deployment**: Deploys all agents and reports success/failure

## 🔑 Service Principal for CI/CD Pipelines

For automated deployments in CI/CD pipelines, you'll need to create a service principal with appropriate permissions.

### 👤 Creating a Service Principal

```bash
# Create a service principal
az ad sp create-for-rbac --name "DeployAgentSP" --role contributor --scopes /subscriptions/{subscription-id}
```

This command will output credentials including:
- `appId` (Client ID)
- `password` (Client Secret)
- `tenant` (Tenant ID)

**🔒 Important**: Save these credentials securely. The client secret will only be shown once.

### Assigning Azure AI User Role

After creating the service principal, assign the Azure AI User role at the resource group level:

```bash
az role assignment create \
  --assignee {appId-from-previous-step} \
  --role "Azure AI User" \
  --scope /subscriptions/{subscriptId}/resourceGroups/{resourceGroupName}
```

### Using Service Principal in Pipelines

Set the following environment variables in your CI/CD pipeline:

```bash
export AZURE_CLIENT_ID="{appId}"
export AZURE_CLIENT_SECRET="{password}"
export AZURE_TENANT_ID="{tenant}"
```

With these variables set, `DefaultAzureCredential` will automatically use the service principal for authentication.

## 🛡️ Error Handling

The application provides detailed error messages for common issues:
- Missing or invalid YAML file
- Invalid configuration settings
- Authentication failures
- Agent creation errors
- OpenAPI specification fetch failures

## 📄 License

See the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
