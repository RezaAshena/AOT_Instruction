using Microsoft.Extensions.DependencyInjection;

namespace Mcp.LocalFileSearch;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalFileSearchMcp(this IServiceCollection services)
    {
        services.AddSingleton<ILocalFileSearchEngine, LocalFileSearchEngine>();
        services.AddSingleton<LocalFileSearchToolHandler>();
        services.AddSingleton<LocalFileSearchMcpServer>();
        return services;
    }
}
