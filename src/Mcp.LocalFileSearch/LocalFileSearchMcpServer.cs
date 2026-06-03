using Agent.Contracts;

namespace Mcp.LocalFileSearch;

public sealed class LocalFileSearchMcpServer(LocalFileSearchToolHandler toolHandler)
{
    public string Name => "LocalFileSearchMcpServer";

    public Task<LocalFileSearchResponse> SearchFilesAsync(LocalFileSearchRequest request, CancellationToken cancellationToken = default)
        => toolHandler.ExecuteAsync(request, cancellationToken);
}
