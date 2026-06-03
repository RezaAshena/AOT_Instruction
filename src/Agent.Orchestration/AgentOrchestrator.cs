using Agent.Contracts;
using Mcp.LocalFileSearch;
using Microsoft.Extensions.Options;

namespace Agent.Orchestration;

public sealed class AgentOrchestrator(
    QueryIntentClassifier classifier,
    LocalFileSearchMcpServer localFileSearchMcpServer,
    ILearnMcpClient learnMcpClient,
    IOptions<AgentOrchestrationOptions> orchestrationOptions) : IAgentOrchestrator
{
    private readonly AgentOrchestrationOptions _orchestrationOptions = orchestrationOptions.Value;

    public async Task<AgentQueryResponse> ExecuteAsync(AgentQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var intent = classifier.Classify(request.Query);
        return intent switch
        {
            QueryIntent.LocalFileSearch => await HandleLocalFileSearchAsync(request, cancellationToken),
            QueryIntent.MicrosoftOrAzure => await HandleMicrosoftOrAzureAsync(request, cancellationToken),
            _ => new AgentQueryResponse("unsupported", new PolicyErrorResponse(
                "unsupported_query_domain",
                "Only local file search and Microsoft/Azure queries are supported.",
                ["local_file_search", "microsoft_azure"]))
        };
    }

    private async Task<AgentQueryResponse> HandleLocalFileSearchAsync(AgentQueryRequest request, CancellationToken cancellationToken)
    {
        var localRequest = request.LocalFileSearch ?? BuildLocalSearchRequestFromQuery(request.Query);
        var result = await localFileSearchMcpServer.SearchFilesAsync(localRequest, cancellationToken);

        // Local file query policy: payload must remain a strongly-typed object suitable for direct JSON serialization.
        return new AgentQueryResponse("local_file_search", result);
    }

    private async Task<AgentQueryResponse> HandleMicrosoftOrAzureAsync(AgentQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await learnMcpClient.AskAsync(request.Query, cancellationToken);
        var preciseAnswer = TruncateToMaxLength(result.Answer, 2000);
        return new AgentQueryResponse("microsoft_azure", new MicrosoftLearnResponse(preciseAnswer));
    }

    private LocalFileSearchRequest BuildLocalSearchRequestFromQuery(string query)
    {
        var extension = query.Contains("pdf", StringComparison.OrdinalIgnoreCase)
            ? ".pdf"
            : TryExtractExtension(query);

        return new LocalFileSearchRequest(
            RootFolderPath: _orchestrationOptions.DefaultLocalRootPath,
            Extension: extension,
            IncludeSubdirectories: true);
    }

    private static string? TryExtractExtension(string query)
    {
        var known = new[] { ".pdf", ".docx", ".xlsx", ".xls", ".jpg", ".txt" };
        return known.FirstOrDefault(ext => query.Contains(ext, StringComparison.OrdinalIgnoreCase)
            || query.Contains(ext.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
    }

    private static string TruncateToMaxLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
