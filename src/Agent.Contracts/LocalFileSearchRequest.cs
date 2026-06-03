namespace Agent.Contracts;

public sealed record LocalFileSearchRequest(
    string RootFolderPath,
    string? FileNameContains = null,
    string? FolderPathContains = null,
    string? Extension = null,
    DateTimeOffset? CreatedFromUtc = null,
    DateTimeOffset? CreatedToUtc = null,
    DateTimeOffset? ModifiedFromUtc = null,
    DateTimeOffset? ModifiedToUtc = null,
    long? MinimumFileSizeBytes = null,
    long? MaximumFileSizeBytes = null,
    bool IncludeSubdirectories = true,
    int? MaxResults = 200
);