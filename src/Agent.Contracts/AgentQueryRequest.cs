namespace Agent.Contracts;

public sealed record AgentQueryRequest(
    string Query,
    LocalFileSearchRequest? LocalFileSearch = null
);