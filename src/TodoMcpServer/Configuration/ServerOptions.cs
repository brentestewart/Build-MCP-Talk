namespace TodoMcpServer.Configuration;

public class ServerOptions
{
    public const string SectionName = "Server";
    
    public TransportMode Mode { get; set; } = TransportMode.Stdio;
    public int HttpPort { get; set; } = McpConfiguration.DefaultHttpPort;
    public string BackendApiUrl { get; set; } = McpConfiguration.DefaultBackendApiUrl;
}

public enum TransportMode
{
    Stdio,
    Http
}
