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
**Purpose**: MCP protocol server exposing todo management tools and resources.

**Architecture**:
- **Official .NET MCP SDK** (`ModelContextProtocol` and `ModelContextProtocol.AspNetCore` packages)
- **Attribute-based tool/resource discovery** using `[McpServerTool]` and `[McpServerResource]`
- **Extension method pattern** for clean service registration
- **Two transport modes**:
  - `stdio`: VS Code/Claude Desktop integration via standard I/O
  - `http`: HTTP-based integration via ASP.NET Core
- **Clean Program.cs**: ~90 lines with separated concerns using local functions

**Key Components**:
```
Configuration/
  McpConfiguration.cs  - Constants for default port and backend URL
  ServerOptions.cs     - Server configuration options class

Extensions/
  ServiceCollectionExtensions.cs - DI registration for MCP services

Models/
  TaskDto.cs           - Backend API task DTO
  ApiResults.cs        - Result types (AddTaskResult, CompleteTaskResult, ListTasksResult)

Tools/
  TodoTools.cs         - MCP tool implementations using [McpServerTool] attributes

Resources/
  TodoResources.cs     - MCP resource implementations using [McpServerResource] attributes
  project-context.json - Project metadata resource content

Services/
  TodoApiClient.cs     - HTTP client for backend integration
```

**SDK Patterns**:
1. **Attribute-based discovery**: Tools and resources use `[McpServerToolType]`/`[McpServerResourceType]` class markers and `[McpServerTool]`/`[McpServerResource]` method markers
2. **Dependency injection**: `TodoApiClient` injected via constructor into `TodoTools`
3. **Host builder pattern**: Use `Host.CreateApplicationBuilder` for stdio, `WebApplication.CreateBuilder` for HTTP
4. **SDK registration**: Chain `.AddMcpServer().WithStdioServerTransport().WithTools<T>().WithResources<T>()`
5. **Transport selection**: Simple args parsing switches between stdio and HTTP modes

**Tool Implementations**:
- `AddTask(string description)` - Adds a new todo item
- `CompleteTask(int id)` - Marks a task as complete
- `ListTasks()` - Returns all open tasks

**Resource Implementations**:
- `project://context` - Project metadata (team, priorities, sprint, conventions)

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

## Code Quality Standards

### For Talk/Demo Code
1. **SDK over custom protocol**: Use official MCP SDK to demonstrate best practices, not low-level protocol implementation
2. **Attribute-driven patterns**: Leverage `[McpServerTool]` and `[McpServerResource]` for clear, declarative API surface
3. **Dependency injection**: Use standard .NET DI patterns for services like `TodoApiClient`
4. **Host builder pattern**: Follow .NET hosting conventions for both stdio and HTTP modes
5. **Demo-friendly entry point**: Keep `Program.cs` short and clear with obvious branching for different modes
6. **Descriptive attributes**: Use `[Description]` on tools, resources, and parameters for self-documenting APIs

### Architecture Notes
- **Custom protocol branch preserved**: `custom-protocol-implementation` branch contains low-level JSON-RPC implementation for reference
- **SDK advantages for talks**: Attendees can replicate the patterns, focus shifts from protocol to business logic
- **Tool results as strings**: Simple string returns from tool methods make demo output clear and readable

## Common Pitfalls To Avoid
- Skipping `[Description]` attributes on tools and parameters (reduces LLM tool-use effectiveness)
- Mixing SDK registration methods (use `.WithTools<T>()` not manual registration)
- Forgetting to mark tool/resource classes with `[McpServerToolType]`/`[McpServerResourceType]`
- Using raw `ILogger` instead of `ILogger<T>` in .NET 10
- Missing `[FromServices]` attributes in TodoApi causing parameter binding ambiguity
- Trying to implement custom protocol handling when SDK provides it

## Integration Points To Track As Code Is Added
- MCP protocol boundary between server and consumer.
- External systems/tools exposed by the MCP server (if any) and where credentials/config are loaded.
- Transport/runtime assumptions (local process, stdio, HTTP, etc.) should be explicit in code and docs.
