namespace DeployAgent.Core.Models;

public class ToolDefinition : DefinitionBase
{
    public string Kind { get; set; } = string.Empty;
    public string? SpecUrl { get; set; }
    public string Description { get; set; } = string.Empty;
}
