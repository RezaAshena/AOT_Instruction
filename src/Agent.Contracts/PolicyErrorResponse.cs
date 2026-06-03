namespace Agent.Contracts;

public sealed record PolicyErrorResponse(
    string Code,
    string Message,
    IReadOnlyList<string> AllowedDomains
);