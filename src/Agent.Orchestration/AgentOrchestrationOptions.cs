namespace Agent.Orchestration;

public sealed class AgentOrchestrationOptions
{
    public const string SectionName = "Orchestration";

    public string DefaultLocalRootPath { get; set; } = "sample-data";
}
