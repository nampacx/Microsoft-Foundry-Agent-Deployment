using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using DeployAgent.Core.Abstractions;
using DeployAgent.Core.Models;
using DeployAgent.Core.Services;
using CoreAgentDefinition = DeployAgent.Core.Models.AgentDefinition;
using CoreToolDefinition = DeployAgent.Core.Models.ToolDefinition;

namespace DeployAgent.V2.Services;

public class AIProjectAgentDeploymentService : IAgentDeploymentService
{
    private readonly AIProjectClient _projectClient;
    private readonly AgentDefinitionService _definitionService;
    private readonly OpenApiService _openApiService;
    private readonly Dictionary<string, AgentReference> _createdAgents;
    private readonly Dictionary<string, byte[]> _openApiSpecs;

    public AIProjectAgentDeploymentService(
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

        Console.WriteLine("Initializing AI Project Client (V2)...");

        var credentialOptions = new DefaultAzureCredentialOptions();
        if (!string.IsNullOrEmpty(tenantId))
        {
            credentialOptions.TenantId = tenantId;
        }

        var credentials = new DefaultAzureCredential(credentialOptions);

        // Connect to your project using the endpoint from your project page
        _projectClient = new AIProjectClient(endpoint: new Uri(projectEndpoint), tokenProvider: credentials);
        
        _definitionService = definitionService;
        _openApiService = openApiService;
        _createdAgents = new Dictionary<string, AgentReference>(StringComparer.OrdinalIgnoreCase);
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

    private async Task<(List<CoreAgentDefinition> Agents, List<CoreToolDefinition> Tools)> ParseDefinitionsAsync()
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

    private void ValidateDefinitions(List<CoreAgentDefinition> agents, List<CoreToolDefinition> tools)
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

    private async Task<AgentReference> CreateAgentAsync(
        CoreAgentDefinition agentDef,
        List<CoreToolDefinition> toolDefinitions,
        Dictionary<string, string>? placeholders = null)
    {
        Console.WriteLine($"Creating agent: {agentDef.Name}");

        var instructions = ReplacePlaceholders(agentDef.Instructions, placeholders);
        var openApiTools = await BuildToolsForAgentAsync(agentDef, toolDefinitions);
        var agent = await GetOrCreateAgentAsync(agentDef.Name, agentDef.Model, instructions, openApiTools);
        
        Console.WriteLine($"  ✓ Agent '{agentDef.Name}' created with {openApiTools.Length} tool(s)\n");
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

    private async Task<OpenAPIAgentTool[]> BuildToolsForAgentAsync(
        CoreAgentDefinition agentDef,
        List<CoreToolDefinition> toolDefinitions)
    {
        var agentTools = new List<OpenAPIAgentTool>();

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
            // Note: Agent-to-agent connections might use different approach in V2
            // This can be extended in the future if the V2 SDK supports it
        }

        return agentTools.ToArray();
    }

    private OpenAPIAgentTool? CreateOpenApiTool(CoreToolDefinition toolDef)
    {
        if (!_openApiSpecs.TryGetValue(toolDef.Name, out var spec))
        {
            Console.WriteLine($"  ⚠ Warning: OpenAPI spec not found for '{toolDef.Name}'");
            return null;
        }

        OpenAPIFunctionDefinition toolDefinition = new(
            name: toolDef.Name,
            spec: BinaryData.FromBytes(spec),
            auth: new OpenAPIAnonymousAuthenticationDetails()
        );
        toolDefinition.Description = toolDef.Description;
        
        OpenAPIAgentTool openapiTool = new(toolDefinition);
        return openapiTool;
    }

    private async Task<AgentReference> GetOrCreateAgentAsync(
        string agentName,
        string model,
        string instructions,
        OpenAPIAgentTool[] openAPIAgentTools)
    {
        Console.WriteLine($"Checking if agent '{agentName}' already exists...");

        try
        {
            AgentRecord agentRecord = await _projectClient.Agents.GetAgentAsync(agentName);

            if (agentRecord != null)
            {
                Console.WriteLine($"Agent retrieved (name: {agentRecord.Name}, id: {agentRecord.Id})");
                return agentRecord;
            }
        }
        catch
        {
            Console.WriteLine($"No agent found");
        }

        Console.WriteLine($"Creating agent '{agentName}'...");

        try
        {
            PromptAgentDefinition agentDefinition = new PromptAgentDefinition(model)
            {
                Instructions = instructions,
            };
            
            if (openAPIAgentTools != null && openAPIAgentTools.Length > 0)
            {
                foreach (var tool in openAPIAgentTools)
                {
                    agentDefinition.Tools.Add(tool);
                }
            }

            var agentVersionOptions = new AgentVersionCreationOptions(agentDefinition);

            var agentVersion = _projectClient.Agents.CreateAgentVersion(
                agentName: agentName,
                options: agentVersionOptions
            );

            Console.WriteLine($"Agent created (name: {agentVersion.Value.Name}, id: {agentVersion.Value.Id})");
            return agentVersion.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating agent: {ex.Message}");
            throw;
        }
    }
}
