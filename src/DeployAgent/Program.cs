using DeployAgent.Services;

// Initialize services
var configService = new ConfigurationService(args);
if (!configService.ValidateConfiguration())
{
    return 1;
}

Console.WriteLine("=== Agent Deployment System ===\n");
Console.WriteLine($"Using YAML file: {configService.YamlFilePath}\n");

// Initialize agent definition service
var agentDefinitionService = new AgentDefinitionService(configService.YamlFilePath!);

// Initialize orchestration service with proper disposal
using var openApiService = new OpenApiService();
var orchestrationService = new AgentDeploymentService(
    configService.ProjectEndpoint!,
    configService.TenantId,
    agentDefinitionService,
    openApiService
);

// Create all agents from definitions
try
{
    var createdAgents = await orchestrationService.CreateAllAgentsAsync();
    Console.WriteLine($"\n=== Successfully deployed {createdAgents.Count} agent(s) ===");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}