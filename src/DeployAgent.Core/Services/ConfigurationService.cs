namespace DeployAgent.Core.Services;

public class ConfigurationService
{
    public string? YamlFilePath { get; }
    public string? ProjectEndpoint { get; }
    public string? TenantId { get; }
    public string SdkVersion { get; }

    public ConfigurationService(string[] args)
    {
        // First positional argument is the YAML file path
        if (args.Length > 0 && !args[0].StartsWith('-'))
        {
            YamlFilePath = args[0];
        }

        // Default to V1 if not specified
        SdkVersion = "v1";

        // Parse named command-line arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--project-endpoint":
                case "-p":
                    if (i + 1 < args.Length)
                        ProjectEndpoint = args[++i];
                    break;
                case "--tenant-id":
                case "-t":
                    if (i + 1 < args.Length)
                        TenantId = args[++i];
                    break;
                case "--sdk-version":
                case "-v":
                    if (i + 1 < args.Length)
                        SdkVersion = args[++i].ToLowerInvariant();
                    break;
            }
        }
    }

    public bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(YamlFilePath))
        {
            Console.WriteLine("Usage: DeployAgent <path-to-yaml-file> --project-endpoint <endpoint> [--sdk-version <v1|v2>] [--tenant-id <tenantId>]");
            Console.WriteLine("Example: DeployAgent agents.yaml -p https://example.services.ai.azure.com/api/projects/myproject --sdk-version v1 -t <guid>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --sdk-version, -v    SDK version to use (v1 or v2). Default: v1");
            Console.WriteLine("  --project-endpoint, -p   Azure AI Foundry project endpoint (required)");
            Console.WriteLine("  --tenant-id, -t      Azure tenant ID (optional)");
            return false;
        }

        if (!File.Exists(YamlFilePath))
        {
            Console.WriteLine($"Error: YAML file not found: {YamlFilePath}");
            return false;
        }

        if (string.IsNullOrEmpty(ProjectEndpoint))
        {
            Console.WriteLine("Error: --project-endpoint is required.");
            return false;
        }

        if (SdkVersion != "v1" && SdkVersion != "v2")
        {
            Console.WriteLine($"Error: Invalid SDK version '{SdkVersion}'. Must be 'v1' or 'v2'.");
            return false;
        }

        return true;
    }
}
