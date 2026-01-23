using DeployAgent.Core.Models;

namespace DeployAgent.Core.Abstractions;

/// <summary>
/// Defines the contract for agent deployment services across different SDK versions.
/// </summary>
public interface IAgentDeploymentService
{
    /// <summary>
    /// Creates all agents defined in the configuration with optional placeholder replacements.
    /// </summary>
    /// <param name="placeholders">Optional dictionary of placeholder values for instruction replacement</param>
    /// <returns>Number of agents successfully created</returns>
    Task<int> CreateAllAgentsAsync(Dictionary<string, string>? placeholders = null);

    /// <summary>
    /// Gets an agent by name after it has been created.
    /// </summary>
    /// <param name="agentName">The name of the agent to retrieve</param>
    /// <returns>True if the agent exists, false otherwise</returns>
    bool HasAgent(string agentName);
}
