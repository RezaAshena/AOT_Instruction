namespace Agent.Contracts;

public sealed record LocalFileSearchResponse(
    IReadOnlyList<LocalFileMetadata> Files,
    int Count
);