using System.CommandLine;
using TodoMcpServer.Configuration;
using TodoMcpServer.Servers;

var rootCommand = new RootCommand("Todo MCP Server - Model Context Protocol integration");

// Configure command-line interface
rootCommand.Options.Add(CliOptions.ModeOption);
rootCommand.Options.Add(CliOptions.PortOption);
rootCommand.Options.Add(CliOptions.BackendUrlOption);

// Define application behavior
rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var config = new McpConfiguration
    {
        Mode = parseResult.GetValue(CliOptions.ModeOption),
        Port = parseResult.GetValue(CliOptions.PortOption),
        BackendApiUrl = parseResult.GetValue(CliOptions.BackendUrlOption)!
    };
    
    return config.Mode switch
    {
        ServerMode.Stdio => await StdioServer.RunAsync(config, cancellationToken),
        ServerMode.Http => await HttpServer.RunAsync(config, args),
        _ => throw new InvalidOperationException($"Unknown mode: {config.Mode}")
    };
});

return rootCommand.Parse(args).Invoke();

