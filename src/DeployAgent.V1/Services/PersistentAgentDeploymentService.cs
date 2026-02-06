using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using DeployAgent.Core.Abstractions;
using DeployAgent.Core.Models;
using DeployAgent.Core.Services;
using AzureToolDefinition = Azure.AI.Agents.Persistent.ToolDefinition;
using CoreToolDefinition = DeployAgent.Core.Models.ToolDefinition;

namespace DeployAgent.V1.Services;

public class PersistentAgentDeploymentService : IAgentDeploymentService
{
    private readonly PersistentAgentsClient _client;
    private readonly AgentDefinitionService _definitionService;
    private readonly OpenApiService _openApiService;
    private readonly Dictionary<string, PersistentAgent> _createdAgents;
    private readonly Dictionary<string, byte[]> _openApiSpecs;

    public PersistentAgentDeploymentService(
        string projectEndpoint,
        string? tenantId,
        AgentDefinitionService definitionService,
        OpenApiService openApiService)
    {
        if (string.IsNullOrWhiteSpace(projectEndpoint))
        {
            throw new ArgumentException("Project endpoint cannot be null or empty", nameof(projectEndpoint));
        }

        ArgumentNullException.ThrowIfNull(definitionService);
        ArgumentNullException.ThrowIfNull(openApiService);

        Console.WriteLine("Initializing Persistent Agent Deployment Service (V1)...");

        var credentialOptions = new DefaultAzureCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            credentialOptions.TenantId = tenantId;
        }

        var credentials = new DefaultAzureCredential(credentialOptions);
        _client = new PersistentAgentsClient(projectEndpoint, credentials);
        _definitionService = definitionService;
        _openApiService = openApiService;
        _createdAgents = new Dictionary<string, PersistentAgent>(StringComparer.OrdinalIgnoreCase);
        _openApiSpecs = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<int> CreateAllAgentsAsync(Dictionary<string, string>? placeholders = null)
    {
        Console.WriteLine("\n=== Starting Agent Deployment Process ===\n");

        var (agentDefinitions, toolDefinitions) = await ParseDefinitionsAsync();
        ValidateDefinitions(agentDefinitions, toolDefinitions);

        var orderedAgents = _definitionService.GetAgentsInDependencyOrder(agentDefinitions, toolDefinitions);
        Console.WriteLine($"✓ Agents will be created in order: {string.Join(", ", orderedAgents.Select(a => a.Name))}\n");

        await DownloadOpenApiSpecificationsAsync(toolDefinitions);

        foreach (var agentDef in orderedAgents)
        {
            var agent = await CreateAgentAsync(agentDef, toolDefinitions, placeholders);
            _createdAgents[agentDef.Name] = agent;
        }

        Console.WriteLine($"\n=== All {_createdAgents.Count} agents created successfully! ===\n");
        return _createdAgents.Count;
    }

    public bool HasAgent(string agentName)
    {
        return _createdAgents.ContainsKey(agentName);
    }

    private async Task<(List<AgentDefinition> Agents, List<CoreToolDefinition> Tools)> ParseDefinitionsAsync()
    {
        Console.WriteLine("Parsing agent definitions...");

        try
        {
            var (agents, tools) = await _definitionService.ParseDefinitionsAsync();
            Console.WriteLine($"✓ Successfully parsed {agents.Count} agent definitions and {tools.Count} tool definitions.\n");
            return (agents, tools);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse definitions: {ex.Message}", ex);
        }
    }

    private void ValidateDefinitions(List<AgentDefinition> agents, List<CoreToolDefinition> tools)
    {
        var (isValid, validationErrors) = _definitionService.ValidateDefinitions(agents, tools);

        if (!isValid)
        {
            Console.WriteLine("Validation errors found:");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($"  - {error}");
            }
            throw new InvalidOperationException($"Definition validation failed with {validationErrors.Count} error(s)");
        }

        Console.WriteLine("✓ All definitions validated successfully.");
    }

    private async Task DownloadOpenApiSpecificationsAsync(List<CoreToolDefinition> toolDefinitions)
    {
        var openApiTools = toolDefinitions
            .Where(t => t.Kind.Equals("OpenAPI", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (openApiTools.Count == 0)
        {
            Console.WriteLine("No OpenAPI tools to download.");
            return;
        }

        Console.WriteLine($"Downloading {openApiTools.Count} OpenAPI specification(s)...");

        foreach (var tool in openApiTools)
        {
            if (string.IsNullOrEmpty(tool.SpecUrl))
            {
                Console.WriteLine($"  ⚠ Warning: Tool '{tool.Name}' is missing spec_url");
                continue;
            }

            try
            {
                var spec = await _openApiService.DownloadOpenApiSpecAsync(tool.SpecUrl);
                _openApiSpecs[tool.Name] = spec;
                Console.WriteLine($"  ✓ Downloaded OpenAPI spec for '{tool.Name}'");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download OpenAPI spec for '{tool.Name}': {ex.Message}", ex);
            }
        }

        Console.WriteLine();
    }

    private async Task<PersistentAgent> CreateAgentAsync(
        AgentDefinition agentDef,
        List<CoreToolDefinition> toolDefinitions,
        Dictionary<string, string>? placeholders = null)
    {
        Console.WriteLine($"Creating agent: {agentDef.Name}");

        var instructions = ReplacePlaceholders(agentDef.Instructions, placeholders);
        var agentTools = await BuildToolsForAgentAsync(agentDef, toolDefinitions);
        var agent = await GetOrCreateAgentAsync(agentDef.Name, agentDef.Model, instructions, agentTools);

        Console.WriteLine($"  ✓ Agent '{agentDef.Name}' created with {agentTools.Count} tool(s)\n");
        return agent;
    }

    private static string ReplacePlaceholders(string instructions, Dictionary<string, string>? placeholders)
    {
        if (placeholders == null || placeholders.Count == 0)
        {
            return instructions;
        }

        var result = instructions;
        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{key}}}", value);
        }
        return result;
    }

    private async Task<List<AzureToolDefinition>> BuildToolsForAgentAsync(
        AgentDefinition agentDef,
        List<CoreToolDefinition> toolDefinitions)
    {
        var agentTools = new List<AzureToolDefinition>();

        foreach (var toolName in agentDef.Tools)
        {
            var toolDef = _definitionService.GetToolDefinitionByName(toolDefinitions, toolName);

            if (toolDef == null)
            {
                Console.WriteLine($"  ⚠ Warning: Tool '{toolName}' not found");
                continue;
            }

            if (toolDef.Kind.Equals("OpenAPI", StringComparison.OrdinalIgnoreCase))
            {
                var openApiTool = CreateOpenApiTool(toolDef);
                if (openApiTool != null)
                {
                    agentTools.Add(openApiTool);
                    Console.WriteLine($"  + Added OpenAPI tool: {toolDef.Name}");
                }
            }
            else if (toolDef.Kind.Equals("agent", StringComparison.OrdinalIgnoreCase))
            {
                var connectedAgentTool = CreateConnectedAgentTool(toolDef);
                if (connectedAgentTool != null)
                {
                    agentTools.Add(connectedAgentTool);
                    Console.WriteLine($"  + Added connected agent tool: {toolDef.Name}");
                }
            }
        }

        return agentTools;
    }

    private OpenApiToolDefinition? CreateOpenApiTool(CoreToolDefinition toolDef)
    {
        if (!_openApiSpecs.TryGetValue(toolDef.Name, out var spec))
        {
            Console.WriteLine($"  ⚠ Warning: OpenAPI spec not found for '{toolDef.Name}'");
            return null;
        }

        var oaiAuth = new OpenApiAnonymousAuthDetails();
        return new OpenApiToolDefinition(
            name: toolDef.Name,
            description: toolDef.Description,
            spec: BinaryData.FromBytes(spec),
            openApiAuthentication: oaiAuth,
            defaultParams: ["format"]
        );
    }

    private ConnectedAgentToolDefinition? CreateConnectedAgentTool(CoreToolDefinition toolDef)
    {
        if (!_createdAgents.TryGetValue(toolDef.Name, out var referencedAgent))
        {
            Console.WriteLine($"  ⚠ Warning: Referenced agent '{toolDef.Name}' not yet created");
            return null;
        }

        return new ConnectedAgentToolDefinition(
            new ConnectedAgentDetails(
                id: referencedAgent.Id,
                name: referencedAgent.Name,
                description: toolDef.Description
            )
        );
    }

    private async Task<PersistentAgent> GetOrCreateAgentAsync(
        string agentName,
        string modelDeploymentName,
        string instructions,
        List<AzureToolDefinition> tools)
    {
        PersistentAgent? agent = null;

        try
        {
            agent = FindExistingAgent(agentName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Warning: Error checking for existing agents: {ex.Message}");
        }

        if (agent == null)
        {
            var toolsArray = tools.Count > 0 ? tools.ToArray() : null;
            try
            {
                Console.WriteLine("  → No existing agent found, creating a new one...");
                agent = await _client.Administration.CreateAgentAsync(
                    model: modelDeploymentName,
                    name: agentName,
                    instructions: instructions,
                    tools: toolsArray
                );

                Console.WriteLine($"  → Created new agent: {agent.Id}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create agent '{agentName}': {ex.Message}", ex);
            }
        }
        else if (agent.Instructions != instructions || agent.Tools.Count != tools.Count|| agent.Model != modelDeploymentName)
        {
            Console.WriteLine("  → Updating existing agent.");

            try
            {
                agent = await _client.Administration.UpdateAgentAsync(
                    agent.Id,
                    model: modelDeploymentName,
                    instructions: instructions,
                    tools: tools.ToArray()
                );
                Console.WriteLine($"  → Updated existing agent: {agent.Id}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update agent '{agentName}': {ex.Message}", ex);
            }
        }

        return agent;
    }

    private PersistentAgent? FindExistingAgent(string agentName)
    {
        var existingAgents = _client.Administration.GetAgents();
        foreach (var existingAgent in existingAgents)
        {
            if (existingAgent.Name == agentName)
            {
                Console.WriteLine($"  → Found existing agent: {existingAgent.Id}");
                return existingAgent;
            }
        }
        return null;
    }
}
