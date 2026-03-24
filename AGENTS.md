# AGENTS.md

## Purpose
- This repository is a **working demo/talk project** showcasing Model Context Protocol (MCP) implementation in .NET.
- Primary intent: demonstrate MCP server architecture for conference presentation.
- Code prioritizes clarity and demo-friendliness over production patterns.

## What Exists Today
- `README.md`: talk framing and MCP motivation.
- `docs/overview.md`: architectural overview and implementation details.
- `src/TodoMcpServer/`: MCP server implementation (stdio + HTTP transports).
- `src/TodoApi/`: Backend API that the MCP server integrates with.
- `Build-MCP-Talk.sln`: .NET 10 solution file.
- `slides/`: Presentation assets (contents not tracked).

## Architecture Signals (From Docs)
- Expected major components:
  - MCP server component (tool/skill host).
  - Consumer application that calls/uses the MCP server.
- Expected high-level flow:
  1. Consumer sends structured requests to MCP server.
  2. MCP server executes controlled logic/tooling.
  3. Results are returned to the consumer/LLM workflow.
- Keep server and consumer as separate projects/modules once code is introduced.

## Implementation Overview (Current State)

### TodoMcpServer Project
**Purpose**: MCP protocol server exposing todo management tools.

**Architecture**:
- **Fully static server classes** (`StdioServer`, `HttpServer`) - no fake instance abstractions
- **System.CommandLine 3.0** for CLI parsing with typed options
- **Two transport modes**:
  - `stdio`: VS Code integration via standard I/O
  - `http`: Claude Desktop integration via HTTP endpoints
- **Clean Program.cs**: 23 lines showing demo-friendly high-level flow

**Key Components**:
```
Configuration/
  CliOptions.cs        - CLI option definitions (static readonly Option<T> properties)
  McpConfiguration.cs  - Configuration model with defaults
  ServerMode.cs        - Enum for Stdio/Http mode selection

Models/
  McpRequest.cs        - MCP protocol request structure
  McpResponse.cs       - MCP protocol response structure
  TaskDto.cs           - Backend API task DTO
  ApiResults.cs        - Result types (AddTaskResult, CompleteTaskResult, ListTasksResult)
  ToolDefinition.cs    - MCP tool metadata

Servers/
  StdioServer.cs       - Static stdio transport server
  HttpServer.cs        - Static HTTP transport server

Services/
  McpProtocolHandler.cs - MCP protocol message routing and handling
  TodoApiClient.cs      - HTTP client for backend integration
  ToolDefinitions.cs    - MCP tool definitions registry
```

**Established Patterns**:
1. **Distributed constants**: Constants live near their usage (no monolithic Constants.cs)
   - Protocol constants → McpProtocolHandler
   - Tool names → ToolDefinitions
   - Config defaults → McpConfiguration
   - CLI option names → CliOptions
2. **Static server architecture**: Both servers are static classes with `RunAsync()` entry points
3. **ILoggerFactory.CreateLogger(string)**: Pattern for logging in static classes
4. **ServerMode enum**: Type-safe mode selection instead of magic strings
5. **Direct object construction**: No unnecessary factory methods or abstraction layers

### TodoApi Project
**Purpose**: Simple backend API for task management (demonstrates external system integration).

**Architecture**:
- ASP.NET Core Minimal APIs
- In-memory task storage (demo simplicity)
- CORS enabled for all origins (development/demo only)
- Runs on `http://localhost:5000`

**Key Components**:
```
Endpoints/
  TodoEndpoints.cs  - HTTP endpoints (GET /tasks, POST /tasks, POST /tasks/{id}/complete)

Services/
  TaskService.cs    - In-memory task management

Models/
  TaskItem.cs       - Task domain model
  TaskCreateRequest.cs - API request model
```

**Important Notes**:
- .NET 10 requires `[FromServices]` on DI parameters when there's body parameter ambiguity
- Use `ILogger<Program>` not raw `ILogger` (raw ILogger not registered in DI)

## Agent Workflow In This Repo
- First read `README.md` and `docs/overview.md` before proposing implementation details.
- Do not claim build/test commands exist unless you can point to committed files (e.g., `.sln`, `.csproj`, CI config).
- When adding initial code, create explicit runnable entry points and document exact commands in `README.md`.
- Prefer .NET conventions for initial scaffolding because `.gitignore` is tailored for .NET artifacts.

## Conventions To Preserve
- Keep docs aligned with code reality; update `docs/overview.md` if architecture scope changes.
- Avoid introducing extra top-level folders unless they map to server, consumer, docs, or slides.
- Document integration boundaries clearly (server API surface, consumer call path) as soon as code appears.

## Code Quality Standards (Established Through Refactoring)
1. **Simplicity over abstraction**: Prefer direct, visible code flow over "clean architecture" ceremony
2. **No unnecessary indirection**: Eliminate single-method wrapper classes and factory methods that just wrap object initialization
3. **Constants near usage**: Distribute constants to relevant classes, not monolithic constant files
4. **Consistent architecture**: If one server is static, make them all static (no mixing patterns)
5. **Namespace organization**: Group related files (Configuration/, Models/, Services/, Servers/)
6. **Demo-friendly Program.cs**: Keep entry point short and clear for presentation purposes
7. **Type safety**: Use enums over string constants for type-safe selection (e.g., ServerMode)
8. **Honest abstractions**: Static classes that don't need instances shouldn't pretend to be instance-based

## Common Pitfalls To Avoid
- Creating Constants.cs junk drawer files
- Mixing static and instance patterns in related classes
- Factory methods that only wrap `new { }`
- Single-method "runner" classes that just delegate
- Tuple returns when static properties would be clearer
- Using raw `ILogger` instead of `ILogger<T>` in .NET 10
- Missing `[FromServices]` attributes causing parameter binding ambiguity

## Integration Points To Track As Code Is Added
- MCP protocol boundary between server and consumer.
- External systems/tools exposed by the MCP server (if any) and where credentials/config are loaded.
- Transport/runtime assumptions (local process, stdio, HTTP, etc.) should be explicit in code and docs.
