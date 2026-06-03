using Agent.Contracts;
using Mcp.LocalFileSearch;
using Xunit;

namespace LocalFileSearch.Tests;

public sealed class LocalFileSearchEngineTests
{
    [Fact]
    public async Task SearchAsync_FiltersByExtensionAndReturnsSortedResults()
    {
        var root = CreateTempDirectory();
        try
        {
            var zooDir = Directory.CreateDirectory(Path.Combine(root, "zoology"));
            await File.WriteAllTextAsync(Path.Combine(zooDir.FullName, "alpha.pdf"), "A");
            await File.WriteAllTextAsync(Path.Combine(zooDir.FullName, "beta.txt"), "B");
            await File.WriteAllTextAsync(Path.Combine(zooDir.FullName, "gamma.pdf"), "C");

            var engine = new LocalFileSearchEngine();
            var response = await engine.SearchAsync(new LocalFileSearchRequest(
                RootFolderPath: root,
                Extension: ".pdf",
                IncludeSubdirectories: true));

            Assert.Equal(2, response.Count);
            Assert.All(response, file => Assert.Equal(".pdf", file.Extension, ignoreCase: true));
            Assert.True(string.Compare(response[0].FullPath, response[1].FullPath, StringComparison.OrdinalIgnoreCase) < 0);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SearchAsync_FiltersBySizeRange()
    {
        var root = CreateTempDirectory();
        try
        {
            var fileA = Path.Combine(root, "small.pdf");
            var fileB = Path.Combine(root, "large.pdf");

            await File.WriteAllTextAsync(fileA, "12345");
            await File.WriteAllTextAsync(fileB, new string('x', 500));

            var engine = new LocalFileSearchEngine();
            var response = await engine.SearchAsync(new LocalFileSearchRequest(
                RootFolderPath: root,
                Extension: ".pdf",
                MinimumFileSizeBytes: 100,
                MaximumFileSizeBytes: 1000));

            Assert.Single(response);
            Assert.Equal("large.pdf", response[0].FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"local-search-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
