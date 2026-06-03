using System.Text;
using System.Text.Json;
using Agent.Contracts;
using Microsoft.Extensions.Options;

namespace Agent.Orchestration;

public sealed class LearnMcpClient(HttpClient httpClient, IOptions<LearnMcpOptions> options) : ILearnMcpClient
{
    private readonly LearnMcpOptions _options = options.Value;

    public async Task<MicrosoftLearnResponse> AskAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        var payload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString("N"),
            method = "tools/call",
            @params = new
            {
                name = _options.ToolName,
                arguments = new
                {
                    query
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, AgentJson.Default), Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return new MicrosoftLearnResponse(string.Empty);
        }

        using var document = JsonDocument.Parse(body);
        var answer = TryExtractAnswer(document.RootElement);

        return new MicrosoftLearnResponse(answer);
    }

    private static string TryExtractAnswer(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object)
            {
                if (result.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                {
                    var texts = content
                        .EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.Object && item.TryGetProperty("text", out _))
                        .Select(item => item.GetProperty("text").GetString())
                        .Where(text => !string.IsNullOrWhiteSpace(text));

                    var joined = string.Join("\n", texts!);
                    if (!string.IsNullOrWhiteSpace(joined))
                    {
                        return joined;
                    }
                }
            }
        }

        return root.ToString();
    }
}
