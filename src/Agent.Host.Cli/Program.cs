using System.Text.Json;
using Agent.Contracts;
using Agent.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.Title = "AOT Instruction - Dual MCP CLI Agent";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddAgentOrchestration(configuration);

using var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<IAgentOrchestrator>();

while (true)
{
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var response = await orchestrator.ExecuteAsync(new AgentQueryRequest(input));

    if (string.Equals(response.Domain, "microsoft_azure", StringComparison.OrdinalIgnoreCase))
    {
        if (response.Payload is MicrosoftLearnResponse learn)
        {
            Console.WriteLine(learn.Answer);
            continue;
        }
    }

    Console.WriteLine(JsonSerializer.Serialize(response.Payload, AgentJson.Default));
}
