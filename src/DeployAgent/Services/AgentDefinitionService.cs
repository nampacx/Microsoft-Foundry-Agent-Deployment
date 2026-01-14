using DeployAgent.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DeployAgent.Services;

public class AgentDefinitionService
{
    private readonly string _agentDefinitionsPath;

    public AgentDefinitionService(string agentDefinitionsPath)
    {
        if (string.IsNullOrWhiteSpace(agentDefinitionsPath))
        {
            throw new ArgumentException("Agent definitions path cannot be null or empty", nameof(agentDefinitionsPath));
        }

        _agentDefinitionsPath = agentDefinitionsPath;
    }

    public async Task<(List<AgentDefinition> Agents, List<ToolDefinition> Tools)> ParseDefinitionsAsync()
    {
        if (!File.Exists(_agentDefinitionsPath))
        {
            throw new FileNotFoundException($"Agent definitions file not found at: {_agentDefinitionsPath}");
        }

        var yamlContent = await File.ReadAllTextAsync(_agentDefinitionsPath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var yamlData = deserializer.Deserialize<YamlDefinitions>(yamlContent);

        var agentDefinitions = yamlData.Agents?.Select(a => new AgentDefinition
        {
            Type = a.Type,
            Name = a.Name,
            Model = a.Model,
            Instructions = a.Instructions,
            Tools = a.Tools ?? new List<string>()
        }).ToList() ?? new List<AgentDefinition>();

        var toolDefinitions = yamlData.Tools?.Select(t => new ToolDefinition
        {
            Type = t.Type,
            Name = t.Name,
            Kind = t.Kind,
            Description = t.Description,
            SpecUrl = t.SpecUrl
        }).ToList() ?? new List<ToolDefinition>();

        return (agentDefinitions, toolDefinitions);
    }

    public (bool IsValid, List<string> Errors) ValidateDefinitions(
        List<AgentDefinition> agents,
        List<ToolDefinition> tools)
    {
        var errors = new List<string>();

        var agentLookup = agents.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
        var toolLookup = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        ValidateAgentTools(tools, agentLookup, errors);
        ValidateToolReferences(agents, toolLookup, errors);

        var cyclicErrors = DetectCyclicDependencies(agents, tools);
        errors.AddRange(cyclicErrors);

        return (errors.Count == 0, errors);
    }

    private static void ValidateAgentTools(
        List<ToolDefinition> tools,
        Dictionary<string, AgentDefinition> agentLookup,
        List<string> errors)
    {
        foreach (var tool in tools.Where(t => t.Kind.Equals("agent", StringComparison.OrdinalIgnoreCase)))
        {
            if (!agentLookup.ContainsKey(tool.Name))
            {
                errors.Add($"Tool '{tool.Name}' of kind 'agent' references non-existent agent '{tool.Name}'");
            }
        }
    }

    private static void ValidateToolReferences(
        List<AgentDefinition> agents,
        Dictionary<string, ToolDefinition> toolLookup,
        List<string> errors)
    {
        foreach (var agent in agents)
        {
            foreach (var toolName in agent.Tools)
            {
                if (!toolLookup.ContainsKey(toolName))
                {
                    errors.Add($"Agent '{agent.Name}' references non-existent tool '{toolName}'");
                }
            }
        }
    }

    private List<string> DetectCyclicDependencies(
        List<AgentDefinition> agents,
        List<ToolDefinition> tools)
    {
        var errors = new List<string>();
        var agentToolMap = tools
            .Where(t => t.Kind.Equals("agent", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var agent in agents)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var path = new List<string>();

            if (HasCyclicDependency(agent.Name, agents, agentToolMap, visited, path))
            {
                errors.Add($"Cyclic dependency detected: {string.Join(" -> ", path)} -> {agent.Name}");
            }
        }

        return errors;
    }

    private bool HasCyclicDependency(
        string agentName,
        List<AgentDefinition> agents,
        Dictionary<string, ToolDefinition> agentToolMap,
        HashSet<string> visited,
        List<string> path)
    {
        if (path.Contains(agentName, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (visited.Contains(agentName))
        {
            return false;
        }

        visited.Add(agentName);
        path.Add(agentName);

        var agent = agents.FirstOrDefault(a => a.Name.Equals(agentName, StringComparison.OrdinalIgnoreCase));
        if (agent != null)
        {
            foreach (var toolName in agent.Tools)
            {
                if (agentToolMap.TryGetValue(toolName, out var agentTool))
                {
                    if (HasCyclicDependency(agentTool.Name, agents, agentToolMap, visited, path))
                    {
                        return true;
                    }
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    public List<AgentDefinition> GetAgentsInDependencyOrder(
        List<AgentDefinition> agents,
        List<ToolDefinition> tools)
    {
        var agentToolMap = tools
            .Where(t => t.Kind.Equals("agent", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var ordered = new List<AgentDefinition>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Visit(AgentDefinition agent)
        {
            if (visited.Contains(agent.Name))
                return;

            visited.Add(agent.Name);

            foreach (var toolName in agent.Tools)
            {
                if (agentToolMap.TryGetValue(toolName, out _))
                {
                    var dependentAgent = agents.FirstOrDefault(a =>
                        a.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));

                    if (dependentAgent != null)
                    {
                        Visit(dependentAgent);
                    }
                }
            }

            ordered.Add(agent);
        }

        foreach (var agent in agents)
        {
            Visit(agent);
        }

        return ordered;
    }

    public AgentDefinition? GetAgentDefinitionByName(List<AgentDefinition> definitions, string name)
    {
        return definitions.FirstOrDefault(d =>
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public ToolDefinition? GetToolDefinitionByName(List<ToolDefinition> definitions, string name)
    {
        return definitions.FirstOrDefault(d =>
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public List<ToolDefinition> GetToolDefinitionsByKind(List<ToolDefinition> definitions, string kind)
    {
        return definitions.Where(d =>
            d.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

internal class YamlDefinitions
{
    public List<YamlAgent>? Agents { get; set; }
    public List<YamlTool>? Tools { get; set; }
}

internal class YamlAgent
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<string>? Tools { get; set; }
}

internal class YamlTool
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SpecUrl { get; set; }
}