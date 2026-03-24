using TodoMcpServer.Configuration;
using TodoMcpServer.Models;
using TodoMcpServer.Services;

namespace TodoMcpServer.Servers;

public static class HttpServer
{
    public static async Task<int> RunAsync(McpConfiguration config, string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Register services
        builder.Services.AddSingleton(config);
        
        builder.Services.AddHttpClient<TodoApiClient>(client =>
        {
            client.BaseAddress = new Uri(config.BackendApiUrl);
        });

        builder.Services.AddScoped<McpProtocolHandler>();

        // Configure JSON options
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("HttpServer");

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
           .WithName("HealthCheck");

        // POST /mcp endpoint for JSON-RPC 2.0
        app.MapPost("/mcp", async (HttpContext context, McpProtocolHandler protocolHandler) =>
        {
            var request = await context.Request.ReadFromJsonAsync<McpRequest>();
            var response = await protocolHandler.ProcessRequestAsync(request);
            return Results.Json(response);
        });

        // Configure port
        app.Urls.Clear();
        app.Urls.Add($"http://localhost:{config.Port}");

        logger.LogInformation("Starting MCP Server in HTTP mode on http://localhost:{Port} (Backend API: {BackendUrl})", 
            config.Port, config.BackendApiUrl);

        await app.RunAsync();
        return 0;
    }
}