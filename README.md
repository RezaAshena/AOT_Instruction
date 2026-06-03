# AOT Instruction - Dual MCP CLI Agent

## Overview
This solution implements a code-first .NET agent with two tool backends:
- **Local File Search MCP (in-process)** for local metadata-based file discovery.
- **Microsoft Learn MCP (remote)** at `https://learn.microsoft.com/api/mcp` for Microsoft/Azure queries.

The CLI supports only:
1. Local file search queries.
2. Microsoft/Azure queries.

Unsupported domains return structured policy JSON.

## Solution Structure
- `src/Agent.Contracts` - shared DTOs and JSON settings.
- `src/Mcp.LocalFileSearch` - local in-process MCP search server and filesystem search engine.
- `src/Agent.Orchestration` - intent classifier, routing, and response policy enforcement.
- `src/Agent.Host.Cli` - CLI host and runtime wiring.
- `tests/LocalFileSearch.Tests` - local search unit tests.
- `tests/Orchestration.Tests` - orchestration and policy tests.
- `docs/SKILLS.md` - tool selection and output behavior instructions.
- `sample-data` - non-technology zoology/biology files in multiple extensions.

## Requirements Coverage
- Metadata filters: file name, folder path, extension, created date, modified date, file size.
- Local queries return typed JSON payloads only.
- Microsoft/Azure answers route to Learn MCP and are capped to 2,000 characters.
- Minimum sample extensions included: `.pdf`, `.docx`, `.xlsx`, `.xls`, `.jpg`, `.txt`.

## Build
```powershell
dotnet build AOT_Instruction.slnx
```

## Test
```powershell
dotnet test AOT_Instruction.slnx
```

## Run CLI
```powershell
dotnet run --project src/Agent.Host.Cli/Agent.Host.Cli.csproj
```

Enter a query and press Enter. Type `exit` to quit.

## Configuration
`src/Agent.Host.Cli/appsettings.json`
- `Orchestration:DefaultLocalRootPath` (defaults to `sample-data`)
- `LearnMcp:Endpoint` (defaults to `https://learn.microsoft.com/api/mcp`)
- `LearnMcp:ToolName` (defaults to `microsoft_docs_search`)

## API Key Note
OpenAI API integration can be added for LLM orchestration. When you are ready for end-to-end OpenAI runtime validation, share the API key and preferred model deployment details.

## GitHub Check-in
The repository is structured for direct push to GitHub. You can publish as:
- Public repository, or
- Private repository shared with `abin-aot`.
