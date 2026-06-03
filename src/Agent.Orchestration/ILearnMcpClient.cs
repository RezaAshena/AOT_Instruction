using Agent.Contracts;

namespace Agent.Orchestration;

public interface ILearnMcpClient
{
    Task<MicrosoftLearnResponse> AskAsync(string query, CancellationToken cancellationToken = default);
}
