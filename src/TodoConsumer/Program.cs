using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure;

// Load configuration from user secrets
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

Console.WriteLine("MCP Todo Consumer - Connecting to server...\n");

// Create HTTP transport to connect to the MCP server
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri("http://localhost:3000")
});

try
{
    // Create and initialize the MCP client
    await using var client = await McpClient.CreateAsync(transport);
    Console.WriteLine("✓ Connected to MCP server\n");

    // List available tools
    var tools = await client.ListToolsAsync();
    Console.WriteLine($"Available tools ({tools.Count}):");
    foreach (var tool in tools)
    {
        Console.WriteLine($"  - {tool.Name}: {tool.Description}");
    }
    Console.WriteLine();

    // Check if Azure OpenAI is configured
    var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
    var azureApiKey = configuration["AzureOpenAI:ApiKey"];
    var azureDeployment = configuration["AzureOpenAI:Deployment"] ?? "gpt-4o-mini";
    bool aiEnabled = !string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey);

    // Interactive menu loop
    while (true)
    {
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("  1. Add a task");
        Console.WriteLine("  2. List all tasks");
        Console.WriteLine("  3. Complete a task");
        if (aiEnabled)
        {
            Console.WriteLine("  4. AI Assistant (chat with AI to manage tasks)");
        }
        Console.WriteLine($"  {(aiEnabled ? "5" : "4")}. Exit");
        Console.Write($"\nEnter your choice (1-{(aiEnabled ? "5" : "4")}): ");
        
        var choice = Console.ReadLine()?.Trim();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                await AddTask(client);
                break;
            case "2":
                await ListTasks(client);
                break;
            case "3":
                await CompleteTask(client);
                break;
            case "4" when aiEnabled:
                await AiAssistant(client, azureEndpoint!, azureApiKey!, azureDeployment);
                break;
            case "4" when !aiEnabled:
            case "5" when aiEnabled:
                Console.WriteLine("Goodbye!");
                return 0;
            default:
                Console.WriteLine($"Invalid choice. Please enter 1-{(aiEnabled ? "5" : "4")}.\n");
                break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}

static async Task AddTask(McpClient client)
{
    Console.Write("Enter task description: ");
    var description = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrEmpty(description))
    {
        Console.WriteLine("✗ Task description cannot be empty.\n");
        return;
    }

    var result = await client.CallToolAsync("add_task", new Dictionary<string, object?>
    {
        ["description"] = description
    });
    
    foreach (var content in result.Content)
    {
        if (content is TextContentBlock textContent)
        {
            Console.WriteLine($"✓ {textContent.Text}");
        }
    }
    Console.WriteLine();
}

static async Task ListTasks(McpClient client)
{
    var result = await client.CallToolAsync("list_tasks", new Dictionary<string, object?>());
    
    foreach (var content in result.Content)
    {
        if (content is TextContentBlock textContent)
        {
            Console.WriteLine(textContent.Text);
        }
    }
    Console.WriteLine();
}

static async Task CompleteTask(McpClient client)
{
    Console.Write("Enter task ID to complete: ");
    var input = Console.ReadLine()?.Trim();
    
    if (!int.TryParse(input, out int taskId))
    {
        Console.WriteLine("✗ Invalid task ID. Please enter a number.\n");
        return;
    }

    var result = await client.CallToolAsync("complete_task", new Dictionary<string, object?>
    {
        ["id"] = taskId
    });
    
    foreach (var content in result.Content)
    {
        if (content is TextContentBlock textContent)
        {
            Console.WriteLine($"✓ {textContent.Text}");
        }
    }
    Console.WriteLine();
}

static async Task AiAssistant(McpClient mcpClient, string endpoint, string apiKey, string deployment)
{
    Console.WriteLine("🤖 AI Assistant Mode");
    Console.WriteLine("Type your request in natural language (or 'back' to return to menu)\n");
    
    // Get MCP tools
    var mcpTools = await mcpClient.ListToolsAsync();
    
    // Create Azure OpenAI chat client
    var azureClient = new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureKeyCredential(apiKey));
    var chatClient = azureClient.GetChatClient(deployment).AsIChatClient();
    
    // Wrap with automatic function invocation
    var aiClient = new FunctionInvokingChatClient(chatClient);
    
    // Conversation history
    List<ChatMessage> conversation = 
    [
        new ChatMessage(ChatRole.System, 
            "You are a helpful task management assistant. " +
            "Use the available tools to help users manage their tasks. " +
            "Be conversational and friendly.")
    ];
    
    while (true)
    {
        Console.Write("You: ");
        var userInput = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(userInput))
        {
            continue;
        }
        
        if (userInput.Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine();
            return;
        }
        
        // Add user message to conversation
        conversation.Add(new ChatMessage(ChatRole.User, userInput));
        
        try
        {
            // Get AI response with automatic tool invocation
            var response = await aiClient.GetResponseAsync(
                conversation,
                new ChatOptions { Tools = [.. mcpTools] });
            
            // Add AI response to conversation
            foreach (var message in response.Messages)
            {
                conversation.Add(message);
            }
            
            // Display the final response
            Console.WriteLine($"AI: {response.Text}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}\n");
        }
    }
}
