using DeployAgent.Core.Abstractions;
using DeployAgent.Core.Services;

// Initialize services
var configService = new ConfigurationService(args);
if (!configService.ValidateConfiguration())
{
    return 1;
}

Console.WriteLine("=== Agent Deployment System ===");
Console.WriteLine($"SDK Version: {configService.SdkVersion.ToUpperInvariant()}");
Console.WriteLine($"Using YAML file: {configService.YamlFilePath}\n");

// Initialize agent definition service
var agentDefinitionService = new AgentDefinitionService(configService.YamlFilePath!);

// Initialize orchestration service based on SDK version
using var openApiService = new OpenApiService();
IAgentDeploymentService deploymentService;

switch (configService.SdkVersion.ToLowerInvariant())
{
    case "v1":
        deploymentService = new DeployAgent.V1.Services.PersistentAgentDeploymentService(
            configService.ProjectEndpoint!,
            configService.TenantId,
            agentDefinitionService,
            openApiService
        );
        break;
    
    case "v2":
        deploymentService = new DeployAgent.V2.Services.AIProjectAgentDeploymentService(
            configService.ProjectEndpoint!,
            configService.TenantId,
            agentDefinitionService,
            openApiService
        );
        break;
    
    default:
        Console.WriteLine($"Error: Unsupported SDK version '{configService.SdkVersion}'");
        return 1;
}

// Create all agents from definitions
try
{
    var agentCount = await deploymentService.CreateAllAgentsAsync();
    Console.WriteLine($"\n=== Successfully deployed {agentCount} agent(s) ===");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}
