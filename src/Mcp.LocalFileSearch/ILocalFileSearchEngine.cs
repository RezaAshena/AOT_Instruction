using Agent.Contracts;

namespace Mcp.LocalFileSearch;

public interface ILocalFileSearchEngine
{
    Task<IReadOnlyList<LocalFileMetadata>> SearchAsync(LocalFileSearchRequest request, CancellationToken cancellationToken = default);
}
