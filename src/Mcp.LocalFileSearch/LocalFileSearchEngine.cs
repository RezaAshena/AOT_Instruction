using Agent.Contracts;

namespace Mcp.LocalFileSearch;

public sealed class LocalFileSearchEngine : ILocalFileSearchEngine
{
    public Task<IReadOnlyList<LocalFileMetadata>> SearchAsync(LocalFileSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RootFolderPath))
        {
            throw new ArgumentException("RootFolderPath is required.", nameof(request));
        }

        var rootPath = Path.GetFullPath(request.RootFolderPath);
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        var extensionFilter = NormalizeExtension(request.Extension);
        var fileNameFilter = request.FileNameContains?.Trim();
        var folderFilter = request.FolderPathContains?.Trim();
        var maxResults = request.MaxResults is null or <= 0 ? 200 : Math.Min(request.MaxResults.Value, 1000);

        var results = new List<LocalFileMetadata>();
        var searchOption = request.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*", searchOption))
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(filePath);
            }
            catch
            {
                continue;
            }

            var extension = fileInfo.Extension;
            if (!string.IsNullOrWhiteSpace(extensionFilter) && !string.Equals(extension, extensionFilter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(fileNameFilter) && fileInfo.Name.IndexOf(fileNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var folderPath = fileInfo.DirectoryName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(folderFilter) && folderPath.IndexOf(folderFilter, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var createdUtc = new DateTimeOffset(fileInfo.CreationTimeUtc);
            if (request.CreatedFromUtc.HasValue && createdUtc < request.CreatedFromUtc.Value)
            {
                continue;
            }

            if (request.CreatedToUtc.HasValue && createdUtc > request.CreatedToUtc.Value)
            {
                continue;
            }

            var modifiedUtc = new DateTimeOffset(fileInfo.LastWriteTimeUtc);
            if (request.ModifiedFromUtc.HasValue && modifiedUtc < request.ModifiedFromUtc.Value)
            {
                continue;
            }

            if (request.ModifiedToUtc.HasValue && modifiedUtc > request.ModifiedToUtc.Value)
            {
                continue;
            }

            var size = fileInfo.Length;
            if (request.MinimumFileSizeBytes.HasValue && size < request.MinimumFileSizeBytes.Value)
            {
                continue;
            }

            if (request.MaximumFileSizeBytes.HasValue && size > request.MaximumFileSizeBytes.Value)
            {
                continue;
            }

            results.Add(new LocalFileMetadata(
                fileInfo.Name,
                fileInfo.FullName,
                folderPath,
                extension,
                size,
                createdUtc,
                modifiedUtc));

            if (results.Count >= maxResults)
            {
                break;
            }
        }

        var ordered = results
            .OrderBy(file => file.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<LocalFileMetadata>>(ordered);
    }

    private static string? NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return null;
        }

        return extension.StartsWith('.') ? extension.Trim() : $".{extension.Trim()}";
    }
}
