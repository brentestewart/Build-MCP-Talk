using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using TodoMcpServer.Configuration;
using TodoMcpServer.Resources;
using TodoMcpServer.Services;
using TodoMcpServer.Tools;

namespace TodoMcpServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodoMcpServer(
        this IServiceCollection services,
        ServerOptions options,
        TransportMode transportMode)
    {
        // Register HTTP client for TodoApi backend
        services.AddHttpClient<TodoApiClient>(client =>
        {
            client.BaseAddress = new Uri(options.BackendApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register MCP server with appropriate transport
        var mcpBuilder = services.AddMcpServer();
        
        if (transportMode == TransportMode.Http)
        {
            mcpBuilder.WithHttpTransport();
        }
        else
        {
            mcpBuilder.WithStdioServerTransport();
        }
        
        mcpBuilder
            .WithTools<TodoTools>()
            .WithResources<TodoResources>();

        return services;
    }
}
