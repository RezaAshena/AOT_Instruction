using System.Net.Http.Headers;
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

        using var request = CreateRequest(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = string.IsNullOrWhiteSpace(body)
                ? "<empty>"
                : body.Length <= 500 ? body : $"{body[..500]}...";

            throw new HttpRequestException(
                $"Request to '{_options.Endpoint}' failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}). Response body: {errorBody}",
                null,
                response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return new MicrosoftLearnResponse(string.Empty);
        }

        body = NormalizeResponseBody(body, response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(body);
        var answer = TryExtractAnswer(document.RootElement);

        return new MicrosoftLearnResponse(answer);
    }

    private HttpRequestMessage CreateRequest(object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, AgentJson.Default), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        return request;
    }

    private static string NormalizeResponseBody(string body, string? mediaType)
    {
        if (string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase)
            || body.StartsWith("event:", StringComparison.OrdinalIgnoreCase)
            || body.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            body = ExtractJsonFromSse(body);
        }

        if (body.StartsWith("{", StringComparison.Ordinal) || body.StartsWith("[", StringComparison.Ordinal))
        {
            return body;
        }

        var preview = body.Length <= 500 ? body : $"{body[..500]}...";
        throw new JsonException($"Expected JSON response from '{nameof(LearnMcpClient)}' but received non-JSON content. Body: {preview}");
    }

    private static string ExtractJsonFromSse(string body)
    {
        using var reader = new StringReader(body);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var payload = line["data:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(payload) || string.Equals(payload, "[DONE]", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (payload.StartsWith("{", StringComparison.Ordinal) || payload.StartsWith("[", StringComparison.Ordinal))
            {
                return payload;
            }
        }

        return body;
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
