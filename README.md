# Build-MCP-Talk
Teaching LLMs New Tricks: Build Your Own MCP Skills

Large language models are powerful, but out of the box they only know what they’ve been trained on. What if you could give an LLM new skills, access to external systems, or specialized domain knowledge, all while retaining control over the logic? That’s where Model Context Protocol (MCP) servers come in. MCP servers let you expose functionality to an LLM, enabling it to perform tasks it couldn’t do natively.

Come join us as we explore how to design and build a custom MCP server and integrate it with an LLM. You’ll see how MCP servers can augment AI capabilities, provide structured access to data and tools, and enable developers to safely expand the scope of what LLMs can do. We’ll also discuss best practices for building reusable skills and integrating MCP servers in various scenarios.

## Architecture

This repository demonstrates a production-style MCP implementation with three components:

1. **TodoApi** (`src/TodoApi/`) - Backend API with in-memory storage
   - REST endpoints: `GET /tasks`, `POST /tasks`, `POST /tasks/{id}/complete`
   - Runs on `http://localhost:5000`

2. **TodoMcpServer** (`src/TodoMcpServer/`) - MCP Protocol Server
   - Translates MCP protocol to backend API calls
   - Supports stdio mode (VS Code) or HTTP mode (Claude Desktop)
   - Pure protocol translation - no business logic

3. **Shared State** - Multiple MCP instances can run simultaneously calling the same backend

## Getting Started

### Prerequisites

- .NET 10 SDK

### Building

```bash
dotnet build
```

### Running the Backend API

Start the backend API first (required):

```bash
dotnet run --project src/TodoApi
```

API runs on `http://localhost:5000`.

### Running the MCP Server

**For VS Code (stdio mode - default):**
```bash
dotnet run --project src/TodoMcpServer
```

**For Claude Desktop (HTTP mode):**
```bash
dotnet run --project src/TodoMcpServer -- --mode http --port 3000
```

**Options:**
- `--mode stdio|http` - Server mode (default: stdio)
- `--port <number>` - HTTP port (default: 3000)

### Testing via HTTP

When running in HTTP mode, you can test the MCP server directly:

**Initialize the connection:**
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0.0"}}}'
```

**List available tools:**
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
```

**Call a tool - add_task:**
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"add_task","arguments":{"description":"Prepare conference abstract"}}}'
```

**Call a tool - complete_task:**
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"complete_task","arguments":{"id":1}}}'
```

**Call a tool - list_tasks:**
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"list_tasks","arguments":{}}}'
```

#### Available Tools

The server exposes three tools:
- **add_task**: Add a new task with a description
- **complete_task**: Mark a task as completed by ID
- **list_tasks**: List all open (incomplete) tasks

### Connecting to VS Code

**For VS Code Insiders on macOS**, MCP servers must be configured in your User settings directory, not in workspace `.vscode` folders.

Edit or create: `~/Library/Application Support/Code - Insiders/User/mcp.json`

Add the todo-server configuration:

```json
{
  "servers": {
    "todo-server": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/Users/YOUR_USERNAME/Developer/Build-MCP-Talk/src/TodoMcpServer"
      ],
      "env": {},
      "type": "stdio"
    }
  },
  "inputs": []
}
```

**Important:** Update the path to match your actual project location.

After editing the config file, restart VS Code (or reload the window with Cmd+Shift+P → "Developer: Reload Window"). The three to-do tools should appear in GitHub Copilot's tool list.

**Important:**  Make sure the backend API (`TodoApi`) is running on port 5000 before starting VS Code's MCP server instance.

### Connecting to Claude Desktop

For Claude Desktop, the MCP server needs to run in HTTP mode. Start it separately:

```bash
# First, ensure the backend API is running:
dotnet run --project src/TodoApi

# Then start the MCP server in HTTP mode:
dotnet run --project src/TodoMcpServer -- --mode http --port 3000
```

Add this configuration to your Claude Desktop config file:

**macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "todo-server": {
      "command": "npx",
      "args": ["-y", "mcp-remote", "http://localhost:3000/mcp"]
    }
  }
}
```

After restarting Claude Desktop, the three to-do tools should be available.

**Shared State Demo:** With the backend API running, you can have both VS Code (stdio) and Claude Desktop (HTTP) connected simultaneously - both will share the same task list!

### Project Structure

- `src/TodoApi/` - Backend REST API
  - `Program.cs` - API endpoints and configuration
  - `Models/TaskItem.cs` - Task data model
  - `Services/TaskService.cs` - In-memory storage
- `src/TodoMcpServer/` - MCP Protocol Server
  - `Program.cs` - Mode selection and MCP protocol handling
  - `Models/` - MCP protocol models (McpRequest, McpResponse, ToolDefinition)
  - `Services/TodoApiClient.cs` - HTTP client for backend API

### Available Tools  

The MCP server exposes three tools:
- **add_task**: Add a new task with a description
- **complete_task**: Mark a task as completed by ID  
- **list_tasks**: List all open (incomplete) tasks

### More Information

See [docs/overview.md](docs/overview.md) for detailed architecture and design decisions.