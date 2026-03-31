using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace TodoConsumer;

/// <summary>
/// Demonstrates how MCP tools integrate with LLMs via Microsoft.Extensions.AI.
/// These methods are not called by the console app - they show the integration pattern.
/// </summary>
internal static class LlmIntegrationExample
{
    /// <summary>
    /// Shows how to pass MCP tools to an LLM and handle tool call requests.
    /// The LLM decides which tools to call based on the user's message.
    /// </summary>
    public static async Task DemoManualToolInvocationAsync(IChatClient chatClient)
    {
        // Connect to the MCP server
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:3000")
        });
        
        await using var mcpClient = await McpClient.CreateAsync(transport);
        
        // Get MCP tools - McpClientTool inherits from AIFunction
        IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
        
        // Create a conversation
        List<ChatMessage> conversation = 
        [
            new ChatMessage(ChatRole.System, 
                "You are a helpful assistant. Use the available tools when needed."),
            new ChatMessage(ChatRole.User, 
                "Add a task to review PR #42")
        ];
        
        // Call the LLM with MCP tools available
        ChatResponse response = await chatClient.GetResponseAsync(
            conversation,
            new ChatOptions 
            { 
                Tools = [.. mcpTools],
                ToolMode = ChatToolMode.Auto
            });
        
        // Check if the LLM wants to call tools
        foreach (var message in response.Messages)
        {
            foreach (var item in message.Contents)
            {
                if (item is FunctionCallContent functionCall)
                {
                    // Execute the tool via MCP
                    var toolResult = await mcpClient.CallToolAsync(
                        functionCall.Name, 
                        functionCall.Arguments as IReadOnlyDictionary<string, object?>);
                    
                    // Add results back to conversation
                    conversation.Add(message);
                    conversation.Add(new ChatMessage(ChatRole.Tool, 
                        [new FunctionResultContent(functionCall.CallId, toolResult)]));
                    
                    // Get final response from LLM
                    response = await chatClient.GetResponseAsync(conversation);
                }
            }
        }
    }
    
    /// <summary>
    /// Shows automatic tool invocation using FunctionInvokingChatClient.
    /// Tool calls are automatically executed and fed back to the LLM.
    /// </summary>
    public static async Task DemoAutomaticToolInvocationAsync(IChatClient baseChatClient)
    {
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:3000")
        });
        
        await using var mcpClient = await McpClient.CreateAsync(transport);
        IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
        
        // Wrap the chat client to auto-invoke tools
        var autoClient = new FunctionInvokingChatClient(baseChatClient);
        
        // Tool calls are now automatic
        var response = await autoClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.User, 
                    "Add tasks for reviewing PR #42 and updating the docs")
            ],
            new ChatOptions { Tools = [.. mcpTools] });
    }
}
