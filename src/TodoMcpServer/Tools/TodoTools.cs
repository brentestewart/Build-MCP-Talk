using System.ComponentModel;
using ModelContextProtocol.Server;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

[McpServerToolType]
public class TodoTools(TodoApiClient apiClient)
{
    [McpServerTool]
    [Description("Creates a new task and adds it to the to-do list. Use this when the user wants to track a new action item or reminder.")]
    public async Task<string> AddTask(
        [Description("A clear, concise description of the task to be completed. Should be between 1-500 characters. Examples: 'Review pull request #123', 'Schedule team meeting for Q2 planning'")] string description)
    {
        // Validate input before calling API
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Error: Description is required";
        }
        
        if (description.Length > 500)
        {
            return "Error: Description must be 500 characters or less";
        }
        
        var result = await apiClient.AddTaskAsync(description);
        
        if (!result.Success)
        {
            return $"Error: {result.Error}";
        }

        return $"Task added successfully: #{result.Task?.Id} - {result.Task?.Description}";
    }

    [McpServerTool]
    [Description("Marks an existing task as completed using its unique identifier. Use this when a user has finished a task and wants to remove it from their active list.")]
    public async Task<string> CompleteTask(
        [Description("The numeric ID of the task to complete. This ID is provided when listing tasks or creating new ones.")] int id)
    {
        // Validate input
        if (id <= 0)
        {
            return "Error: Task ID must be a positive integer";
        }
        
        var result = await apiClient.CompleteTaskAsync(id);
        
        if (!result.Success)
        {
            return $"Error: {result.Error}";
        }

        return $"Task #{result.Task?.Id} marked as completed";
    }

    [McpServerTool]
    [Description("Returns all incomplete tasks. Call this when the user wants to see or work with their open tasks.")]
    public async Task<string> ListTasks()
    {
        var result = await apiClient.ListTasksAsync();
        
        if (!result.Success)
        {
            return $"Error: {result.Error}";
        }

        if (result.Tasks.Count == 0)
        {
            return "No open tasks found.";
        }

        var taskList = string.Join("\n", result.Tasks.Select(t => $"#{t.Id}: {t.Description}"));
        return $"Open tasks ({result.Count}):\n{taskList}";
    }
}
