using Agent.Contracts;

namespace Mcp.LocalFileSearch;

public sealed class LocalFileSearchToolHandler(ILocalFileSearchEngine engine)
{
    public const string ToolName = "local_file_search";

    public async Task<LocalFileSearchResponse> ExecuteAsync(LocalFileSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var files = await engine.SearchAsync(request, cancellationToken);
        return new LocalFileSearchResponse(files, files.Count);
    }
}
