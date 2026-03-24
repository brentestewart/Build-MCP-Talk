## Overview

This demo implements a production-style MCP (Model Context Protocol) architecture with clear separation of concerns:

### Architecture Components

1. **Backend API** (`TodoApi`)
   - Simple REST API with in-memory storage
   - Business logic layer
   - Runs independently on port 5000
   - Endpoints:
     - `GET /tasks` - List all open tasks
     - `POST /tasks` - Add a new task
     - `POST /tasks/{id}/complete` - Mark task as completed

2. **MCP Protocol Server** (`TodoMcpServer`)
   - Protocol translation layer only - no business logic
   - Two operating modes:
     - **stdio mode** (default) - For VS Code integration via stdin/stdout
     - **HTTP mode** - For Claude Desktop and manual testing
   - Translates MCP JSON-RPC 2.0 calls to backend API HTTP requests

3. **Shared State**
   - Multiple MCP server instances can run simultaneously
   - All instances call the same backend API
   - Demonstrates realistic multi-client architecture

### Original Requirements (Modified)

The original requirements called for a simple HTTP-based MCP server. The implementation has evolved to demonstrate production patterns:

**Original Concept:**
You are building a small proof-of-concept HTTP-based MCP server for a demo. The server should implement a simple To-Do list with three actions:

“add_task”: Adds a task with a description.
“complete_task”: Marks a task as completed by ID.
“list_tasks”: Returns all open tasks.
Requirements:

The server should accept HTTP POST requests with JSON payloads in the MCP standard format: { “method”: “”, “params”: { … } }

The server should return structured JSON responses: { “result”: , “error”: null }

Example calls: POST /mcp Body: {“method”: “add_task”, “params”: {“description”: “Prepare conference abstract”}} Response: {“result”: {“id”: 1, “description”: “Prepare conference abstract”, “completed”: false}, “error”: null}

POST /mcp Body: {“method”: “complete_task”, “params”: {“id”: 1}} Response: {“result”: {“id”: 1, “description”: “Prepare conference abstract”, “completed”: true}, “error”: null}

POST /mcp Body: {“method”: “list_tasks”, “params”: {}} Response: {“result”: [{“id”: 1, “description”: “Prepare conference abstract”, “completed”: false}], “error”: null}

Keep the server minimal and self-contained so it can run locally on a laptop for a live demo.

Use in-memory storage (no database needed) for simplicity.

Include clear logging so it’s obvious when a method is called.

Generate the complete code for this HTTP MCP server ready to run as a local demo.

The code should use .NET as this is a demo for a .NET user group.

## VS Code Integration

### MCP Server Configuration on macOS

For VS Code Insiders on macOS, MCP servers must be configured in the **User settings directory**, not in workspace `.vscode` folders.

**Configuration file location:**
```
~/Library/Application Support/Code - Insiders/User/mcp.json
```

**Format:**
```json
{
  "servers": {
    "todo-server": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/TodoMcpServer"],
      "env": {},
      "type": "stdio"
    }
  },
  "inputs": []
}
```

**Key Implementation Details:**

1. **Separate Backend**: Backend API (`TodoApi`) handles all business logic and runs independently on port 5000
2. **MCP Server Modes**: MCP server can run in either stdio OR HTTP mode (not simultaneously):
   - **stdio mode** (default): For VS Code - reads from stdin, writes to stdout, logs to stderr
   - **HTTP mode**: For Claude Desktop - runs HTTP server on port 3000
3. **Protocol Translation Only**: MCP server contains no business logic - pure protocol adapter
4. **Shared State**: Multiple MCP instances (one stdio, one HTTP) can connect to the same backend API simultaneously
5. **JSON-RPC Compliance**: Responses must have EITHER `result` OR `error`, never both (use `DefaultIgnoreCondition.WhenWritingNull`)
6. **Command Line Control**: Operating mode selected via `--mode stdio|http` argument

### Demo Flow

1. Start backend API: `dotnet run --project src/TodoApi`
2. For VS Code: Configured in `mcp.json`, starts automatically in stdio mode
3. For Claude Desktop: Manually start in HTTP mode: `dotnet run --project src/TodoMcpServer -- --mode http --port 3000`
4. Both clients share state through the backend API