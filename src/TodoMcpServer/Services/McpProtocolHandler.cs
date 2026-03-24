using System.Text.Json;
using TodoMcpServer.Models;

namespace TodoMcpServer.Services;

public class McpProtocolHandler
{
    // MCP Protocol constants
    private const string ProtocolVersion = "2024-11-05";
    private const string MethodInitialize = "initialize";
    private const string MethodToolsList = "tools/list";
    private const string MethodToolsCall = "tools/call";

    // JSON-RPC Error codes
    private const int ErrorCodeInvalidRequest = -32600;
    private const int ErrorCodeMethodNotFound = -32601;
    private const int ErrorCodeInvalidParams = -32602;
    private const int ErrorCodeInternalError = -32603;
    private const int ErrorCodeParseError = -32700;

    // Server metadata
    private const string ServerName = "todo-mcp-server";
    private const string ServerVersion = "1.0.0";
    private const int MaxDescriptionLength = 500;

    private readonly TodoApiClient _apiClient;
    private readonly ILogger<McpProtocolHandler> _logger;

    public McpProtocolHandler(TodoApiClient apiClient, ILogger<McpProtocolHandler> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<McpResponse> ProcessRequestAsync(McpRequest? request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Method))
            {
                _logger.LogWarning("Received null or invalid request");
                return CreateErrorResponse(null, ErrorCodeInvalidRequest, "Invalid Request");
            }

            _logger.LogInformation("Received method call: {Method} (id: {Id})", request.Method, request.Id);

            McpResponse response = request.Method switch
            {
                MethodInitialize => HandleInitialize(request),
                MethodToolsList => HandleToolsList(request),
                MethodToolsCall => await HandleToolsCallAsync(request),
                _ => CreateErrorResponse(request.Id, ErrorCodeMethodNotFound, $"Method not found: {request.Method}")
            };

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error");
            return CreateErrorResponse(null, ErrorCodeParseError, "Parse error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MCP request");
            return CreateErrorResponse(request?.Id, ErrorCodeInternalError, "Internal error", ex.Message);
        }
    }

    private McpResponse HandleInitialize(McpRequest request)
    {
        _logger.LogInformation("Client initializing connection");

        var result = new
        {
            protocolVersion = ProtocolVersion,
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = ServerName,
                version = ServerVersion
            }
        };

        return new McpResponse { Id = request.Id, Result = result };
    }

    private McpResponse HandleToolsList(McpRequest request)
    {
        _logger.LogInformation("Client requesting tools list");

        var result = new { tools = ToolDefinitions.Tools };

        return new McpResponse { Id = request.Id, Result = result };
    }

    private async Task<McpResponse> HandleToolsCallAsync(McpRequest request)
    {
        try
        {
            if (request.Params == null)
            {
                return CreateErrorResponse(request.Id, ErrorCodeInvalidParams, "Invalid params: params required");
            }

            var paramsJson = request.Params.Value;

            if (!paramsJson.TryGetProperty("name", out var nameElement))
            {
                return CreateErrorResponse(request.Id, ErrorCodeInvalidParams, "Invalid params: 'name' required");
            }

            var toolName = nameElement.GetString();
            _logger.LogInformation("Calling tool: {ToolName}", toolName);

            var arguments = paramsJson.TryGetProperty("arguments", out var argsElement)
                ? argsElement
                : new JsonElement();

            var toolResult = toolName switch
            {
                ToolDefinitions.ToolAddTask => await ExecuteAddTaskAsync(arguments),
                ToolDefinitions.ToolCompleteTask => await ExecuteCompleteTaskAsync(arguments),
                ToolDefinitions.ToolListTasks => await ExecuteListTasksAsync(),
                _ => throw new Exception($"Unknown tool: {toolName}")
            };

            var result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = JsonSerializer.Serialize(toolResult)
                    }
                }
            };

            return new McpResponse { Id = request.Id, Result = result };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool");
            return CreateErrorResponse(request.Id, ErrorCodeInternalError, "Tool execution error", ex.Message);
        }
    }

    private async Task<object> ExecuteAddTaskAsync(JsonElement arguments)
    {
        try
        {
            var description = arguments.GetProperty(ToolDefinitions.PropertyDescription).GetString();
            
            // Validation matching backend rules
            if (string.IsNullOrWhiteSpace(description))
            {
                return new AddTaskResult 
                { 
                    Success = false, 
                    Error = "Description is required" 
                };
            }

            if (description.Length > MaxDescriptionLength)
            {
                return new AddTaskResult 
                { 
                    Success = false, 
                    Error = $"Description must be {MaxDescriptionLength} characters or less" 
                };
            }

            var result = await _apiClient.AddTaskAsync(description);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in add_task");
            return new AddTaskResult { Success = false, Error = ex.Message };
        }
    }

    private async Task<object> ExecuteCompleteTaskAsync(JsonElement arguments)
    {
        try
        {
            var id = arguments.GetProperty(ToolDefinitions.PropertyId).GetInt32();
            var result = await _apiClient.CompleteTaskAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in complete_task");
            return new CompleteTaskResult { Success = false, Error = ex.Message };
        }
    }

    private async Task<object> ExecuteListTasksAsync()
    {
        try
        {
            var result = await _apiClient.ListTasksAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in list_tasks");
            return new ListTasksResult { Success = false, Error = ex.Message, Tasks = new(), Count = 0 };
        }
    }

    private static McpResponse CreateErrorResponse(object? id, int code, string message, string? data = null)
    {
        return new McpResponse
        {
            Id = id,
            Error = new McpErrorObject
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
    }
}
