using TodoMcpServer.Models;

namespace TodoMcpServer.Services;

public static class ToolDefinitions
{
    public const string ToolAddTask = "add_task";
    public const string ToolCompleteTask = "complete_task";
    public const string ToolListTasks = "list_tasks";

    public const string PropertyDescription = "description";
    public const string PropertyId = "id";

    public static readonly List<ToolDefinition> Tools = new()
    {
        new()
        {
            Name = ToolAddTask,
            Description = "Add a new task to the to-do list",
            InputSchema = new InputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    [PropertyDescription] = new() { Type = "string", Description = "Description of the task to add (1-500 characters)" }
                },
                Required = new List<string> { PropertyDescription }
            }
        },
        new()
        {
            Name = ToolCompleteTask,
            Description = "Mark a task as completed by its ID",
            InputSchema = new InputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>
                {
                    [PropertyId] = new() { Type = "number", Description = "ID of the task to mark as completed" }
                },
                Required = new List<string> { PropertyId }
            }
        },
        new()
        {
            Name = ToolListTasks,
            Description = "List all open (incomplete) tasks",
            InputSchema = new InputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, PropertyDefinition>(),
                Required = new List<string>()
            }
        }
    };
}
