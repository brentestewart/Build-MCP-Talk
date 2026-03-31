using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

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

    // Interactive menu loop
    while (true)
    {
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("  1. Add a task");
        Console.WriteLine("  2. List all tasks");
        Console.WriteLine("  3. Complete a task");
        Console.WriteLine("  4. Exit");
        Console.Write("\nEnter your choice (1-4): ");
        
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
            case "4":
                Console.WriteLine("Goodbye!");
                return 0;
            default:
                Console.WriteLine("Invalid choice. Please enter 1-4.\n");
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
