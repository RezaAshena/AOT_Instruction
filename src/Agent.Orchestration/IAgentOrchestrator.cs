using Agent.Contracts;

namespace Agent.Orchestration;

public interface IAgentOrchestrator
{
    Task<AgentQueryResponse> ExecuteAsync(AgentQueryRequest request, CancellationToken cancellationToken = default);
}
