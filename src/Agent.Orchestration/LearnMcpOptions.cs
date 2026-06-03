namespace Agent.Orchestration;

public sealed class LearnMcpOptions
{
    public const string SectionName = "LearnMcp";

    public string Endpoint { get; set; } = "https://learn.microsoft.com/api/mcp";
    public string ToolName { get; set; } = "microsoft_docs_search";
}
