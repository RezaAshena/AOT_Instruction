namespace Agent.Orchestration;

public sealed class QueryIntentClassifier
{
    private static readonly string[] MicrosoftTerms =
    [
        "microsoft", "azure", "asp.net", "dotnet", ".net", "blazor", "c#", "csharp", "sql server", "entra"
    ];

    private static readonly string[] LocalFileTerms =
    [
        "file", "files", "folder", "pdf", "docx", "xlsx", "xls", "jpg", "txt", "local"
    ];

    public QueryIntent Classify(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return QueryIntent.Unsupported;
        }

        if (ContainsAny(query, MicrosoftTerms))
        {
            return QueryIntent.MicrosoftOrAzure;
        }

        if (ContainsAny(query, LocalFileTerms))
        {
            return QueryIntent.LocalFileSearch;
        }

        return QueryIntent.Unsupported;
    }

    private static bool ContainsAny(string query, IReadOnlyList<string> terms) =>
        terms.Any(term => query.Contains(term, StringComparison.OrdinalIgnoreCase));
}
