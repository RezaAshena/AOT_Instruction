using Agent.Contracts;
using Agent.Orchestration;
using Mcp.LocalFileSearch;
using Microsoft.Extensions.Options;
using Xunit;

namespace Orchestration.Tests;

public sealed class AgentOrchestratorTests
{
    [Fact]
    public async Task ExecuteAsync_ForMicrosoftQuery_TruncatesResponseTo2000Characters()
    {
        var orchestrator = CreateOrchestrator(new FakeLearnClient(new string('a', 2500)), new FakeFileEngine());

        var response = await orchestrator.ExecuteAsync(new AgentQueryRequest("What is Azure Blob Storage?"));

        Assert.Equal("microsoft_azure", response.Domain);
        var payload = Assert.IsType<MicrosoftLearnResponse>(response.Payload);
        Assert.Equal(2000, payload.Answer.Length);
    }

    [Fact]
    public async Task ExecuteAsync_ForLocalFileQuery_ReturnsTypedLocalSearchPayload()
    {
        var orchestrator = CreateOrchestrator(new FakeLearnClient("unused"), new FakeFileEngine());

        var response = await orchestrator.ExecuteAsync(new AgentQueryRequest("What PDF files are available in our system?"));

        Assert.Equal("local_file_search", response.Domain);
        var payload = Assert.IsType<LocalFileSearchResponse>(response.Payload);
        Assert.NotEmpty(payload.Files);
        Assert.All(payload.Files, file => Assert.Equal(".pdf", file.Extension, ignoreCase: true));
    }

    [Fact]
    public async Task ExecuteAsync_ForUnsupportedQuery_ReturnsPolicyErrorPayload()
    {
        var orchestrator = CreateOrchestrator(new FakeLearnClient("unused"), new FakeFileEngine());

        var response = await orchestrator.ExecuteAsync(new AgentQueryRequest("Who won the football match today?"));

        Assert.Equal("unsupported", response.Domain);
        var payload = Assert.IsType<PolicyErrorResponse>(response.Payload);
        Assert.Equal("unsupported_query_domain", payload.Code);
    }

    private static AgentOrchestrator CreateOrchestrator(ILearnMcpClient learnClient, ILocalFileSearchEngine fileEngine)
    {
        var localServer = new LocalFileSearchMcpServer(new LocalFileSearchToolHandler(fileEngine));

        return new AgentOrchestrator(
            new QueryIntentClassifier(),
            localServer,
            learnClient,
            Options.Create(new AgentOrchestrationOptions { DefaultLocalRootPath = "." }));
    }

    private sealed class FakeLearnClient(string answer) : ILearnMcpClient
    {
        public Task<MicrosoftLearnResponse> AskAsync(string query, CancellationToken cancellationToken = default)
            => Task.FromResult(new MicrosoftLearnResponse(answer));
    }

    private sealed class FakeFileEngine : ILocalFileSearchEngine
    {
        public Task<IReadOnlyList<LocalFileMetadata>> SearchAsync(LocalFileSearchRequest request, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<LocalFileMetadata> files =
            [
                new LocalFileMetadata(
                    "freshwater-ecosystem-summary.pdf",
                    Path.Combine("sample-data", "biology", "freshwater-ecosystem-summary.pdf"),
                    Path.Combine("sample-data", "biology"),
                    ".pdf",
                    512,
                    DateTimeOffset.UtcNow.AddDays(-3),
                    DateTimeOffset.UtcNow.AddDays(-1))
            ];

            return Task.FromResult(files);
        }
    }
}
