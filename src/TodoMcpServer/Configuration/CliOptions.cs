using System.CommandLine;

namespace TodoMcpServer.Configuration;

internal static class CliOptions
{
    public static readonly Option<ServerMode> ModeOption = new("--mode")
    {
        Description = "stdio (VS Code) or http (Claude Desktop)",
        DefaultValueFactory = _ => ServerMode.Stdio
    };

    public static readonly Option<int> PortOption = new("--port")
    {
        Description = "HTTP server port",
        DefaultValueFactory = _ => McpConfiguration.DefaultPort
    };

    public static readonly Option<string> BackendUrlOption = new("--backend-url")
    {
        Description = "Backend API URL",
        DefaultValueFactory = _ => McpConfiguration.DefaultBackendUrl
    };
}
