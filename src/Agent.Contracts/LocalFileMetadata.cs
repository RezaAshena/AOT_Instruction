namespace Agent.Contracts;

public sealed record LocalFileMetadata(
    string FileName,
    string FullPath,
    string FolderPath,
    string Extension,
    long FileSizeBytes,
    DateTimeOffset CreatedUtc,
    DateTimeOffset ModifiedUtc
);