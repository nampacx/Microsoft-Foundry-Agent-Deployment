namespace DeployAgent.Models;

public class AgentDefinition : DefinitionBase
{
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<string> Tools { get; set; } = new List<string>();
}