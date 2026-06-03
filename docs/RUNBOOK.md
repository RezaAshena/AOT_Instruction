# Runbook

## Quick Start
1. Build: `dotnet build AOT_Instruction.slnx`
2. Test: `dotnet test AOT_Instruction.slnx`
3. Run: `dotnet run --project src/Agent.Host.Cli/Agent.Host.Cli.csproj`

## Validation Scenarios

### Scenario 1: Local file query
Input:
`What PDF files are available in our system?`

Expected:
- Routed to local file search tool.
- JSON object output only.
- Contains `.pdf` file metadata fields.

### Scenario 2: Microsoft/Azure query
Input:
`What is Azure Blob Storage?`

Expected:
- Routed to Learn MCP endpoint.
- Concise answer <= 2,000 chars.

### Scenario 3: Unsupported topic
Input:
`Who won the football match today?`

Expected:
- Policy error JSON indicating allowed domains.

## Troubleshooting
- If local search returns empty results, verify `Orchestration:DefaultLocalRootPath`.
- If Learn MCP fails, verify network access to `https://learn.microsoft.com/api/mcp`.
- If package restore warnings appear, run `dotnet restore` and then `dotnet build`.
