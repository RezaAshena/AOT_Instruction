using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mcp.LocalFileSearch;

namespace Agent.Orchestration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentOrchestration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AgentOrchestrationOptions>(configuration.GetSection(AgentOrchestrationOptions.SectionName));
        services.Configure<LearnMcpOptions>(configuration.GetSection(LearnMcpOptions.SectionName));
        services.AddHttpClient<ILearnMcpClient, LearnMcpClient>();
        services.AddLocalFileSearchMcp();
        services.AddSingleton<QueryIntentClassifier>();
        services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();
        return services;
    }
}
