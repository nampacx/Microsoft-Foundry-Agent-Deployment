namespace DeployAgent.Services;

public class ConfigurationService
{
    public string? YamlFilePath { get; }
    public string? ProjectEndpoint { get; }
    public string? TenantId { get; }

    public ConfigurationService(string[] args)
    {
        // First positional argument is the YAML file path
        if (args.Length > 0 && !args[0].StartsWith('-'))
        {
            YamlFilePath = args[0];
        }

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
            }
        }
    }

    public bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(YamlFilePath))
        {
            Console.WriteLine("Usage: DeployAgent <path-to-yaml-file> --project-endpoint <endpoint> [--tenant-id <tenantId>]");
            Console.WriteLine("Example: DeployAgent agents.yaml -p https://example.services.ai.azure.com/api/projects/myproject -t <guid>");
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

        return true;
    }
}