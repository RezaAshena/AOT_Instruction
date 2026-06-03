namespace Agent.Contracts;

public sealed record AgentQueryResponse(
    string Domain,
    object Payload
);