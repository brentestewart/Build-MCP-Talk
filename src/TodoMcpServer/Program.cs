using Microsoft.Extensions.Logging;
using TodoMcpServer.Configuration;
using TodoMcpServer.Extensions;

// Parse command-line arguments into configuration
var options = ParseArguments(args);

// Run in appropriate mode
if (options.Mode == TransportMode.Http)
{
    await RunHttpModeAsync(options);
}
else
{
    await RunStdioModeAsync(options);
}

return 0;

// --- Local Functions ---

static ServerOptions ParseArguments(string[] args)
{
    var options = new ServerOptions();
    
    // Parse mode
    var modeArg = args.FirstOrDefault()?.ToLowerInvariant();
    if (modeArg == "http")
    {
        options.Mode = TransportMode.Http;
    }
    else if (modeArg == "stdio" || modeArg == null)
    {
        options.Mode = TransportMode.Stdio;
    }
    else
    {
        Console.Error.WriteLine($"Invalid mode '{modeArg}'. Use 'stdio' or 'http'.");
        Environment.Exit(1);
    }
    
    // Parse port (optional, second argument)
    if (args.Length > 1)
    {
        if (!int.TryParse(args[1], out var port))
        {
            Console.Error.WriteLine($"Invalid port '{args[1]}'. Must be a valid integer.");
            Environment.Exit(1);
        }
        options.HttpPort = port;
    }
    
    // Parse backend URL (optional, third argument)
    if (args.Length > 2)
    {
        if (!Uri.TryCreate(args[2], UriKind.Absolute, out _))
        {
            Console.Error.WriteLine($"Invalid backend URL '{args[2]}'.");
            Environment.Exit(1);
        }
        options.BackendApiUrl = args[2];
    }
    
    return options;
}

static async Task RunHttpModeAsync(ServerOptions options)
{
    Console.WriteLine($"Starting MCP server in HTTP mode on port {options.HttpPort}");
    Console.WriteLine($"Backend API: {options.BackendApiUrl}");
    
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddTodoMcpServer(options, TransportMode.Http);
    
    var app = builder.Build();
    app.MapMcp();
    
    await app.RunAsync($"http://localhost:{options.HttpPort}");
}

static async Task RunStdioModeAsync(ServerOptions options)
{
    Console.Error.WriteLine($"Starting MCP server in stdio mode");
    Console.Error.WriteLine($"Backend API: {options.BackendApiUrl}");
    
    var builder = Host.CreateApplicationBuilder();
    
    // Configure logging to stderr for stdio transport
    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });
    
    builder.Services.AddTodoMcpServer(options, TransportMode.Stdio);
    
    await builder.Build().RunAsync();
}

