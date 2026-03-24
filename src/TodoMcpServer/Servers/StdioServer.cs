using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoMcpServer.Configuration;
using TodoMcpServer.Models;
using TodoMcpServer.Services;

namespace TodoMcpServer.Servers;

public static class StdioServer
{
    private const int ErrorCodeInternalError = -32603;

    public static async Task<int> RunAsync(McpConfiguration config, CancellationToken cancellationToken = default)
    {
        // Create host with dependency injection
        using var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                });
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(config);

                services.AddHttpClient<TodoApiClient>(client =>
                {
                    client.BaseAddress = new Uri(config.BackendApiUrl);
                });

                services.AddScoped<McpProtocolHandler>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("StdioServer");
        logger.LogInformation("Starting MCP Server in stdio mode (Backend API: {BackendUrl})", config.BackendApiUrl);

        // Set up cancellation
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            logger.LogInformation("Shutdown signal received");
        };

        // JSON serialization options
        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Process stdio messages
        using var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin);

        string? line;
        while ((line = await reader.ReadLineAsync(cts.Token)) != null)
        {
            if (cts.Token.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested, shutting down");
                break;
            }

            if (string.IsNullOrEmpty(line)) continue;

            logger.LogDebug("Received stdio message: {Message}", line);

            try
            {
                using var scope = host.Services.CreateScope();
                var protocolHandler = scope.ServiceProvider.GetRequiredService<McpProtocolHandler>();

                var request = JsonSerializer.Deserialize<McpRequest>(line);
                var response = await protocolHandler.ProcessRequestAsync(request);
                var responseJson = JsonSerializer.Serialize(response, jsonOptions);

                Console.WriteLine(responseJson);
                logger.LogDebug("Sent stdio response: {Response}", responseJson);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing stdio request");
                var errorResponse = new McpResponse
                {
                    Id = null,
                    Error = new McpErrorObject
                    {
                        Code = ErrorCodeInternalError,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                };
                Console.WriteLine(JsonSerializer.Serialize(errorResponse, jsonOptions));
            }
        }

        logger.LogInformation("Stdio server stopped");
        return 0;
    }
}
