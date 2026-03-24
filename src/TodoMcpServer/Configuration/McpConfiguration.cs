namespace TodoMcpServer.Configuration;

public class McpConfiguration
{
    public const int DefaultPort = 3000;
    public const string DefaultBackendUrl = "http://localhost:5000";

    public ServerMode Mode { get; set; } = ServerMode.Stdio;
    public int Port { get; set; } = DefaultPort;
    public string BackendApiUrl { get; set; } = DefaultBackendUrl;
}
